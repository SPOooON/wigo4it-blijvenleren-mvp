using BlijvenLeren.App.Contracts.V1;
using BlijvenLeren.App.Data.Entities;
using BlijvenLeren.App.Features.LearningResources;

namespace BlijvenLeren.App.Tests;

public sealed class LearningResourceContractMapperTests
{
    [Fact]
    public void ToListItemResponse_MapsCommentCountsByStatus()
    {
        var resource = BuildResource();

        var response = LearningResourceContractMapper.ToListItemResponse(resource);

        Assert.Equal(resource.Id, response.Id);
        Assert.Equal("Resource title", response.Title);
        Assert.Equal(1, response.ApprovedCommentCount);
        Assert.Equal(1, response.PendingCommentCount);
    }

    [Fact]
    public void ToDetailResponse_MapsCommentsInCreatedOrder()
    {
        var resource = BuildResource();

        var response = LearningResourceContractMapper.ToDetailResponse(resource);

        Assert.Equal(3, response.Comments.Count);
        Assert.Equal("First reviewer", response.Comments[0].AuthorDisplayName);
        Assert.Equal("Second reviewer", response.Comments[1].AuthorDisplayName);
        Assert.Equal("Third reviewer", response.Comments[2].AuthorDisplayName);
        Assert.Equal("Approved", response.Comments[0].Status);
        Assert.Equal("External", response.Comments[1].AuthorType);
    }

    [Fact]
    public void ToEntity_TrimsIncomingValues()
    {
        var request = new CreateLearningResourceRequest("  Title  ", "  Description  ", "  https://example.com/resource  ");

        var entity = LearningResourceContractMapper.ToEntity(request, DateTimeOffset.Parse("2026-03-11T12:00:00Z"));

        Assert.Equal("Title", entity.Title);
        Assert.Equal("Description", entity.Description);
        Assert.Equal("https://example.com/resource", entity.Url);
    }

    private static LearningResource BuildResource()
    {
        return new LearningResource
        {
            Id = Guid.Parse("e3d9f8f2-e811-4abd-bf64-e9d6d6ada8de"),
            Title = "Resource title",
            Description = "Resource description",
            Url = "https://example.com/resource",
            CreatedUtc = DateTimeOffset.Parse("2026-03-10T09:00:00Z"),
            Comments =
            [
                new Comment
                {
                    Id = Guid.NewGuid(),
                    AuthorDisplayName = "Second reviewer",
                    AuthorType = CommentAuthorType.External,
                    Body = "Pending feedback",
                    Status = CommentStatus.Pending,
                    CreatedUtc = DateTimeOffset.Parse("2026-03-10T11:00:00Z")
                },
                new Comment
                {
                    Id = Guid.NewGuid(),
                    AuthorDisplayName = "First reviewer",
                    AuthorType = CommentAuthorType.Internal,
                    Body = "Approved feedback",
                    Status = CommentStatus.Approved,
                    CreatedUtc = DateTimeOffset.Parse("2026-03-10T10:00:00Z"),
                    ModeratedUtc = DateTimeOffset.Parse("2026-03-10T10:00:00Z")
                },
                new Comment
                {
                    Id = Guid.NewGuid(),
                    AuthorDisplayName = "Third reviewer",
                    AuthorType = CommentAuthorType.External,
                    Body = "Rejected feedback",
                    Status = CommentStatus.Rejected,
                    CreatedUtc = DateTimeOffset.Parse("2026-03-10T12:00:00Z"),
                    ModeratedUtc = DateTimeOffset.Parse("2026-03-10T12:30:00Z")
                }
            ]
        };
    }
}
