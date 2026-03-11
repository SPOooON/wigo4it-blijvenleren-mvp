using System.Text.Json;
using BlijvenLeren.App.Tests.Infrastructure;

namespace BlijvenLeren.App.Tests;

public sealed class OpenApiIntegrationTests : IClassFixture<TestApplicationFactory>
{
    private readonly TestApplicationFactory _factory;

    public OpenApiIntegrationTests(TestApplicationFactory factory)
    {
        _factory = factory;
        _factory.ResetState();
    }

    [Fact]
    public async Task OpenApiDocument_ExposesBearerSchemeAndProtectedOperationMetadata()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/openapi/v1.json");
        response.EnsureSuccessStatusCode();

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = document.RootElement;

        var securitySchemes = root.GetProperty("components").GetProperty("securitySchemes");
        Assert.True(securitySchemes.TryGetProperty("BearerAuth", out var bearerScheme));
        Assert.Equal("http", bearerScheme.GetProperty("type").GetString());
        Assert.Equal("bearer", bearerScheme.GetProperty("scheme").GetString());

        var protectedOperation = root.GetProperty("paths")
            .GetProperty("/api/v1/comments/pending")
            .GetProperty("get");

        Assert.Contains("Requires an internal-user bearer token.", protectedOperation.GetProperty("description").GetString());
        Assert.True(protectedOperation.GetProperty("responses").TryGetProperty("401", out _));
        Assert.True(protectedOperation.GetProperty("responses").TryGetProperty("403", out _));
    }

    [Fact]
    public async Task DocsUi_RendersScalarPage()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/docs");
        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync();

        Assert.Contains("BlijvenLeren API Docs", html);
        Assert.Contains("scalar", html, StringComparison.OrdinalIgnoreCase);
    }
}
