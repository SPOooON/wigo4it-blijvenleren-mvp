using System.Net.Sockets;
using BlijvenLeren.App.Configuration;
using BlijvenLeren.App.Data;
using BlijvenLeren.App.Data.Entities;
using BlijvenLeren.App.OpenApi;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace BlijvenLeren.App.Features.Runtime;

public static class RuntimeEndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapRuntimeEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/health", () => Results.Ok(new
        {
            status = "ok",
            application = "BlijvenLeren",
            mode = "mvp"
        }))
            .WithSummary("Return a simple application health response.");

        endpoints.MapGet(
            "/api/health/dependencies",
            async (
                IOptions<RuntimeOptions> runtimeOptions,
                IHttpClientFactory httpClientFactory,
                CancellationToken cancellationToken) =>
            {
                var runtime = runtimeOptions.Value;

                var database = await CheckTcpDependencyAsync(
                    runtime.Database.Host,
                    runtime.Database.Port,
                    cancellationToken);

                var identityProvider = await CheckHttpDependencyAsync(
                    runtime.IdentityProvider.Authority,
                    httpClientFactory,
                    cancellationToken);

                return Results.Ok(new
                {
                    status = database.Healthy && identityProvider.Healthy ? "ok" : "degraded",
                    dependencies = new
                    {
                        database,
                        identityProvider
                    }
                });
            })
            .WithSummary("Check whether the app can reach the database and identity provider.");

        endpoints.MapPost(
            "/api/health/persistence-smoke",
            async (AppDbContext dbContext, CancellationToken cancellationToken) =>
            {
                await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

                var resource = new LearningResource
                {
                    Id = Guid.NewGuid(),
                    Title = "Persistence smoke resource",
                    Description = "Temporary record written to validate the initial schema.",
                    Url = "https://example.invalid/resources/persistence-smoke",
                    CreatedUtc = DateTimeOffset.UtcNow
                };

                var comment = new Comment
                {
                    Id = Guid.NewGuid(),
                    LearningResourceId = resource.Id,
                    AuthorDisplayName = "Smoke Tester",
                    AuthorType = CommentAuthorType.External,
                    Body = "Pending moderation comment for schema validation.",
                    Status = CommentStatus.Pending,
                    CreatedUtc = DateTimeOffset.UtcNow
                };

                dbContext.LearningResources.Add(resource);
                dbContext.Comments.Add(comment);
                await dbContext.SaveChangesAsync(cancellationToken);

                var persisted = await dbContext.LearningResources
                    .Include(learningResource => learningResource.Comments)
                    .SingleAsync(learningResource => learningResource.Id == resource.Id, cancellationToken);

                await transaction.RollbackAsync(cancellationToken);

                return Results.Ok(new
                {
                    status = "ok",
                    resourceId = persisted.Id,
                    commentStatuses = persisted.Comments.Select(savedComment => savedComment.Status.ToString())
                });
            })
            .WithSummary("Write and read temporary persistence data inside a rolled-back transaction.");

        endpoints.MapPost(
            "/api/demo/seed-data",
            async (bool? reset, DemoDataSeeder seeder, CancellationToken cancellationToken) =>
            {
                var result = await seeder.SeedAsync(reset ?? false, cancellationToken);
                return Results.Ok(result);
            })
            .RequireAuthorization("InternalUser")
            .WithBearerAuthOpenApi("Requires an internal-user bearer token.")
            .WithSummary("Seed or reseed the demo dataset. Requires the internal-user role.");

        return endpoints;
    }

    private static async Task<DependencyCheckResult> CheckTcpDependencyAsync(
        string host,
        int port,
        CancellationToken cancellationToken)
    {
        using var client = new TcpClient();
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(3));

        try
        {
            await client.ConnectAsync(host, port, timeout.Token);
            return new DependencyCheckResult(host, port, null, true, null, null);
        }
        catch (Exception ex)
        {
            return new DependencyCheckResult(host, port, null, false, null, ex.Message);
        }
    }

    private static async Task<DependencyCheckResult> CheckHttpDependencyAsync(
        string authority,
        IHttpClientFactory httpClientFactory,
        CancellationToken cancellationToken)
    {
        var client = httpClientFactory.CreateClient();
        client.Timeout = TimeSpan.FromSeconds(3);

        try
        {
            using var response = await client.GetAsync(authority, cancellationToken);
            return new DependencyCheckResult(null, null, authority, response.IsSuccessStatusCode, (int)response.StatusCode, null);
        }
        catch (Exception ex)
        {
            return new DependencyCheckResult(null, null, authority, false, null, ex.Message);
        }
    }
}
