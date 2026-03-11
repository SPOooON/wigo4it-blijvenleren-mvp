using System.Net;
using System.Net.Http.Json;
using BlijvenLeren.App.Contracts.V1;
using BlijvenLeren.App.Tests.Infrastructure;

namespace BlijvenLeren.App.Tests;

public sealed class ApiResourceCrudIntegrationTests : IClassFixture<TestApplicationFactory>
{
    private readonly TestApplicationFactory _factory;

    public ApiResourceCrudIntegrationTests(TestApplicationFactory factory)
    {
        _factory = factory;
        _factory.ResetState();
    }

    [Fact]
    public async Task ListAndDetail_ReturnSeededResources()
    {
        using var client = _factory.CreateClient();

        var list = await client.GetFromJsonAsync<List<LearningResourceListItemResponse>>("/api/v1/learning-resources");
        Assert.NotNull(list);
        Assert.Equal(2, list.Count);

        var detail = await client.GetFromJsonAsync<LearningResourceDetailResponse>(
            "/api/v1/learning-resources/901a31cc-3ec7-4e8b-93cb-9cb6c49054af");

        Assert.NotNull(detail);
        Assert.Equal("API Design Basics", detail.Title);
        Assert.Single(detail.Comments);
    }

    [Fact]
    public async Task InternalUser_CanCreateUpdateAndDeleteResource()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-User", "internal.demo");
        client.DefaultRequestHeaders.Add("X-Test-Roles", "internal-user");

        var createResponse = await client.PostAsJsonAsync(
            "/api/v1/learning-resources",
            new CreateLearningResourceRequest(
                "Resource from integration test",
                "Created through the issue #9 API test.",
                "https://example.com/integration-create"));

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<LearningResourceDetailResponse>();
        Assert.NotNull(created);

        var updateResponse = await client.PutAsJsonAsync(
            $"/api/v1/learning-resources/{created.Id}",
            new UpdateLearningResourceRequest(
                "Updated resource title",
                "Updated description",
                "https://example.com/integration-update"));

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var updated = await updateResponse.Content.ReadFromJsonAsync<LearningResourceDetailResponse>();
        Assert.NotNull(updated);
        Assert.Equal("Updated resource title", updated.Title);

        var deleteResponse = await client.DeleteAsync($"/api/v1/learning-resources/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var notFoundResponse = await client.GetAsync($"/api/v1/learning-resources/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, notFoundResponse.StatusCode);
    }

    [Fact]
    public async Task ExternalUser_CannotManageResources()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-User", "external.demo");
        client.DefaultRequestHeaders.Add("X-Test-Roles", "external-contributor");

        var createResponse = await client.PostAsJsonAsync(
            "/api/v1/learning-resources",
            new CreateLearningResourceRequest(
                "Blocked create",
                "Should not be accepted.",
                "https://example.com/blocked-create"));

        var updateResponse = await client.PutAsJsonAsync(
            "/api/v1/learning-resources/901a31cc-3ec7-4e8b-93cb-9cb6c49054af",
            new UpdateLearningResourceRequest(
                "Blocked update",
                "Should not be accepted.",
                "https://example.com/blocked-update"));

        var deleteResponse = await client.DeleteAsync("/api/v1/learning-resources/901a31cc-3ec7-4e8b-93cb-9cb6c49054af");

        Assert.Equal(HttpStatusCode.Forbidden, createResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, updateResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task InternalUser_Comment_IsVisibleImmediately()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-User", "internal.demo");
        client.DefaultRequestHeaders.Add("X-Test-Roles", "internal-user");

        var createResponse = await client.PostAsJsonAsync(
            "/api/v1/learning-resources/901a31cc-3ec7-4e8b-93cb-9cb6c49054af/comments",
            new CreateCommentRequest("Internal comment from issue #10 API test."));

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<CommentResponse>();
        Assert.NotNull(created);
        Assert.Equal("Approved", created.Status);

        var detail = await client.GetFromJsonAsync<LearningResourceDetailResponse>(
            "/api/v1/learning-resources/901a31cc-3ec7-4e8b-93cb-9cb6c49054af");

        Assert.NotNull(detail);
        Assert.Contains(detail.Comments, comment => comment.Body == "Internal comment from issue #10 API test.");
    }

    [Fact]
    public async Task ExternalUser_Comment_IsStoredAsPendingAndHiddenFromDetail()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-User", "external.demo");
        client.DefaultRequestHeaders.Add("X-Test-Roles", "external-contributor");

        var createResponse = await client.PostAsJsonAsync(
            "/api/v1/learning-resources/901a31cc-3ec7-4e8b-93cb-9cb6c49054af/comments",
            new CreateCommentRequest("External comment waiting for moderation."));

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<CommentResponse>();
        Assert.NotNull(created);
        Assert.Equal("Pending", created.Status);

        var detail = await client.GetFromJsonAsync<LearningResourceDetailResponse>(
            "/api/v1/learning-resources/901a31cc-3ec7-4e8b-93cb-9cb6c49054af");

        Assert.NotNull(detail);
        Assert.DoesNotContain(detail.Comments, comment => comment.Body == "External comment waiting for moderation.");
    }
}
