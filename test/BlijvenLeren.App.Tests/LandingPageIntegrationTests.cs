using BlijvenLeren.App.Tests.Infrastructure;

namespace BlijvenLeren.App.Tests;

public sealed class LandingPageIntegrationTests : IClassFixture<TestApplicationFactory>
{
    private readonly TestApplicationFactory _factory;

    public LandingPageIntegrationTests(TestApplicationFactory factory)
    {
        _factory = factory;
        _factory.ResetState();
    }

    [Fact]
    public async Task LandingPage_ShowsReviewerFacingRuntimeUrls_InsteadOfInternalComposeAddresses()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/");
        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync();

        Assert.Contains("localhost:5432", html);
        Assert.Contains("http://localhost:8081/realms/blijvenleren", html);
        Assert.DoesNotContain("db:5432", html);
        Assert.DoesNotContain("http://idp:8080/realms/blijvenleren", html);
    }
}
