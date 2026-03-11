using System.Net.Sockets;
using BlijvenLeren.App.Configuration;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.Configure<RuntimeOptions>(
    builder.Configuration.GetSection(RuntimeOptions.SectionName));
builder.Services.AddRazorPages();
builder.Services.AddHttpClient();

var app = builder.Build();

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
            status = database.healthy && identityProvider.healthy ? "ok" : "degraded",
            dependencies = new
            {
                database,
                identityProvider
            }
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

internal sealed record DependencyCheckResult(
    string? Host,
    int? Port,
    string? Authority,
    bool healthy,
    int? StatusCode,
    string? Error);
