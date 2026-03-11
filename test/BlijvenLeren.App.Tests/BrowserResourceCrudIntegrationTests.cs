using System.Net;
using System.Text.RegularExpressions;
using BlijvenLeren.App.Tests.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BlijvenLeren.App.Tests;

public sealed partial class BrowserResourceCrudIntegrationTests : IClassFixture<TestApplicationFactory>
{
    private readonly TestApplicationFactory _factory;

    public BrowserResourceCrudIntegrationTests(TestApplicationFactory factory)
    {
        _factory = factory;
        _factory.ResetState();
    }

    [Fact]
    public async Task ListingPage_RendersSeededResources()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/LearningResources");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("API Design Basics", html);
        Assert.Contains("Browser Accessibility Checklist", html);
    }

    [Fact]
    public async Task InternalUser_CanCreateEditAndDeleteThroughBrowser()
    {
        using var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
        client.DefaultRequestHeaders.Add("X-Test-User", "internal.demo");
        client.DefaultRequestHeaders.Add("X-Test-Roles", "internal-user");

        var createGet = await client.GetAsync("/LearningResources/Create");
        var createToken = await ExtractRequestVerificationTokenAsync(createGet);
        var createResponse = await client.PostAsync(
            "/LearningResources/Create",
            BuildFormContent(
                createToken,
                new Dictionary<string, string>
                {
                    ["Input.Title"] = "Browser-created resource",
                    ["Input.Description"] = "Created through the Razor Pages happy-path test.",
                    ["Input.Url"] = "https://example.com/browser-create"
                }));

        Assert.Equal(HttpStatusCode.Redirect, createResponse.StatusCode);
        var createdLocation = createResponse.Headers.Location;
        Assert.NotNull(createdLocation);
        var resourceId = createdLocation.OriginalString.Split('/', StringSplitOptions.RemoveEmptyEntries)[^1];

        var detailsResponse = await client.GetAsync(createdLocation);
        var detailsHtml = await detailsResponse.Content.ReadAsStringAsync();
        Assert.Contains("Browser-created resource", detailsHtml);

        var editLocation = $"/LearningResources/Edit/{resourceId}";
        var editGet = await client.GetAsync(editLocation);
        var editToken = await ExtractRequestVerificationTokenAsync(editGet);
        var editResponse = await client.PostAsync(
            editLocation,
            BuildFormContent(
                editToken,
                new Dictionary<string, string>
                {
                    ["Input.Title"] = "Browser-updated resource",
                    ["Input.Description"] = "Updated through the Razor Pages happy-path test.",
                    ["Input.Url"] = "https://example.com/browser-update"
                }));

        Assert.Equal(HttpStatusCode.Redirect, editResponse.StatusCode);
        var updatedLocation = editResponse.Headers.Location;
        Assert.NotNull(updatedLocation);

        var updatedDetailsResponse = await client.GetAsync(updatedLocation);
        var updatedDetailsHtml = await updatedDetailsResponse.Content.ReadAsStringAsync();
        Assert.Contains("Browser-updated resource", updatedDetailsHtml);

        var deleteGet = await client.GetAsync(updatedLocation);
        var deleteToken = await ExtractRequestVerificationTokenAsync(deleteGet);
        var deleteResponse = await client.PostAsync(
            $"{updatedLocation}?handler=Delete",
            BuildFormContent(deleteToken, new Dictionary<string, string>()));

        Assert.Equal(HttpStatusCode.Redirect, deleteResponse.StatusCode);

        var deletedDetailsResponse = await client.GetAsync(updatedLocation);
        Assert.Equal(HttpStatusCode.NotFound, deletedDetailsResponse.StatusCode);
    }

    [Fact]
    public async Task InternalUser_CanAddCommentThroughBrowserAndSeeItImmediately()
    {
        using var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
        client.DefaultRequestHeaders.Add("X-Test-User", "internal.demo");
        client.DefaultRequestHeaders.Add("X-Test-Roles", "internal-user");

        var detailsGet = await client.GetAsync("/LearningResources/Details/901a31cc-3ec7-4e8b-93cb-9cb6c49054af");
        var token = await ExtractRequestVerificationTokenAsync(detailsGet);

        var commentResponse = await client.PostAsync(
            "/LearningResources/Details/901a31cc-3ec7-4e8b-93cb-9cb6c49054af?handler=Comment",
            BuildFormContent(
                token,
                new Dictionary<string, string>
                {
                    ["CommentInput.Body"] = "Internal browser comment"
                }));

        Assert.Equal(HttpStatusCode.Redirect, commentResponse.StatusCode);

        var updatedDetails = await client.GetAsync("/LearningResources/Details/901a31cc-3ec7-4e8b-93cb-9cb6c49054af");
        var html = await updatedDetails.Content.ReadAsStringAsync();
        Assert.Contains("Internal browser comment", html);
    }

    [Fact]
    public async Task ExternalUser_CanSubmitCommentThroughBrowserButDoesNotSeeItImmediately()
    {
        using var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
        client.DefaultRequestHeaders.Add("X-Test-User", "external.demo");
        client.DefaultRequestHeaders.Add("X-Test-Roles", "external-contributor");

        var detailsGet = await client.GetAsync("/LearningResources/Details/901a31cc-3ec7-4e8b-93cb-9cb6c49054af");
        var token = await ExtractRequestVerificationTokenAsync(detailsGet);

        var commentResponse = await client.PostAsync(
            "/LearningResources/Details/901a31cc-3ec7-4e8b-93cb-9cb6c49054af?handler=Comment",
            BuildFormContent(
                token,
                new Dictionary<string, string>
                {
                    ["CommentInput.Body"] = "External browser comment"
                }));

        Assert.Equal(HttpStatusCode.Redirect, commentResponse.StatusCode);

        var updatedDetails = await client.GetAsync("/LearningResources/Details/901a31cc-3ec7-4e8b-93cb-9cb6c49054af");
        var html = await updatedDetails.Content.ReadAsStringAsync();
        Assert.DoesNotContain("External browser comment", html);
    }

    private static FormUrlEncodedContent BuildFormContent(string requestVerificationToken, Dictionary<string, string> fields)
    {
        var formFields = new Dictionary<string, string>(fields)
        {
            ["__RequestVerificationToken"] = requestVerificationToken
        };

        return new FormUrlEncodedContent(formFields);
    }

    private static async Task<string> ExtractRequestVerificationTokenAsync(HttpResponseMessage response)
    {
        var html = await response.Content.ReadAsStringAsync();
        var match = RequestVerificationTokenRegex().Match(html);
        Assert.True(match.Success, "Expected an antiforgery token in the rendered HTML.");

        return match.Groups["token"].Value;
    }

    [GeneratedRegex("<input name=\"__RequestVerificationToken\" type=\"hidden\" value=\"(?<token>[^\"]+)\" ?/?>")]
    private static partial Regex RequestVerificationTokenRegex();
}
