using BlijvenLeren.App.Data;
using BlijvenLeren.App.Data.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BlijvenLeren.App.Tests.Infrastructure;

public sealed class TestApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"blijvenleren-tests-{Guid.NewGuid():N}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Runtime:Database:ApplyMigrationsOnStartup"] = "false",
                ["Runtime:Database:SeedDemoDataOnStartup"] = "false"
            });
        });

        builder.ConfigureServices(services =>
        {
            var dbContextDescriptors = services
                .Where(descriptor =>
                    descriptor.ServiceType == typeof(DbContextOptions<AppDbContext>)
                    || descriptor.ServiceType == typeof(AppDbContext)
                    || (descriptor.ServiceType.IsGenericType
                        && descriptor.ServiceType.GetGenericTypeDefinition() == typeof(IDbContextOptionsConfiguration<>)
                        && descriptor.ServiceType.GenericTypeArguments[0] == typeof(AppDbContext)))
                .ToList();

            foreach (var descriptor in dbContextDescriptors)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(_databaseName));

            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                    options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
                    options.DefaultForbidScheme = TestAuthHandler.SchemeName;
                    options.DefaultSignInScheme = TestAuthHandler.SchemeName;
                    options.DefaultSignOutScheme = TestAuthHandler.SchemeName;
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                    TestAuthHandler.SchemeName,
                    _ => { });

            using var scope = services.BuildServiceProvider().CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            ResetDatabase(dbContext);
        });
    }

    public void ResetState()
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        ResetDatabase(dbContext);
    }

    private static void ResetDatabase(AppDbContext dbContext)
    {
        dbContext.Database.EnsureDeleted();
        dbContext.Database.EnsureCreated();
        Seed(dbContext);
    }

    private static void Seed(AppDbContext dbContext)
    {
        if (dbContext.LearningResources.Any())
        {
            return;
        }

        dbContext.LearningResources.AddRange(
            new LearningResource
            {
                Id = Guid.Parse("901a31cc-3ec7-4e8b-93cb-9cb6c49054af"),
                Title = "API Design Basics",
                Description = "Compact guide to resource-oriented HTTP APIs.",
                Url = "https://example.com/api-design-basics",
                CreatedUtc = DateTimeOffset.Parse("2026-03-10T08:00:00Z"),
                Comments =
                [
                    new Comment
                    {
                        Id = Guid.Parse("e2c7c846-36cf-4ac7-ae3d-34e98837031c"),
                        AuthorDisplayName = "Internal Reviewer",
                        AuthorIdentityName = "internal.reviewer",
                        AuthorType = CommentAuthorType.Internal,
                        Body = "Good primer for the API-first part of the demo.",
                        Status = CommentStatus.Approved,
                        CreatedUtc = DateTimeOffset.Parse("2026-03-10T08:15:00Z"),
                        ModeratedUtc = DateTimeOffset.Parse("2026-03-10T08:15:00Z")
                    }
                ]
            },
            new LearningResource
            {
                Id = Guid.Parse("f116d693-f390-45ec-8d0b-23f6784d65b4"),
                Title = "Browser Accessibility Checklist",
                Description = "Review checklist for form semantics and keyboard access.",
                Url = "https://example.com/browser-accessibility",
                CreatedUtc = DateTimeOffset.Parse("2026-03-10T09:00:00Z"),
                Comments =
                [
                    new Comment
                    {
                        Id = Guid.Parse("6b8b684d-a9d7-4b3f-8ebf-12d6736103f4"),
                        AuthorDisplayName = "External Contributor",
                        AuthorIdentityName = "external.contributor",
                        AuthorType = CommentAuthorType.External,
                        Body = "Could be useful when the UI grows beyond simple forms.",
                        Status = CommentStatus.Pending,
                        CreatedUtc = DateTimeOffset.Parse("2026-03-10T09:30:00Z")
                    }
                ]
            });

        dbContext.SaveChanges();
    }
}
