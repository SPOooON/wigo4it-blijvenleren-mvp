using System.Net.Sockets;
using BlijvenLeren.App.Configuration;
using BlijvenLeren.App.Data;
using BlijvenLeren.App.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.Configure<RuntimeOptions>(
    builder.Configuration.GetSection(RuntimeOptions.SectionName));
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("BlijvenLeren")));
builder.Services.AddRazorPages();
builder.Services.AddHttpClient();

var app = builder.Build();

await ApplyDatabaseMigrationsAsync(app);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseRouting();

app.UseAuthorization();

app.MapGet("/api/health", () => Results.Ok(new
{
    status = "ok",
    application = "BlijvenLeren",
    mode = "bootstrap"
}));

app.MapGet(
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
    });

app.MapPost(
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
    });

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();

static async Task<DependencyCheckResult> CheckTcpDependencyAsync(
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

static async Task<DependencyCheckResult> CheckHttpDependencyAsync(
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

static async Task ApplyDatabaseMigrationsAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var runtimeOptions = scope.ServiceProvider.GetRequiredService<IOptions<RuntimeOptions>>().Value;
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DatabaseStartup");

    if (!runtimeOptions.Database.ApplyMigrationsOnStartup)
    {
        logger.LogInformation("Database migrations on startup are disabled.");
        return;
    }

    for (var attempt = 1; attempt <= 10; attempt++)
    {
        try
        {
            await dbContext.Database.MigrateAsync();
            logger.LogInformation("Database migrations applied successfully.");
            return;
        }
        catch (Exception ex) when (attempt < 10)
        {
            logger.LogWarning(ex, "Database migration attempt {Attempt} failed. Retrying.", attempt);
            await Task.Delay(TimeSpan.FromSeconds(3));
        }
    }

    await dbContext.Database.MigrateAsync();
}
