using BlijvenLeren.App.Contracts.V1;
using BlijvenLeren.App.Data.Entities;

namespace BlijvenLeren.App.Features.LearningResources;

public static class LearningResourceContractMapper
{
    public static LearningResourceListItemResponse ToListItemResponse(LearningResource resource)
    {
        return new LearningResourceListItemResponse(
            resource.Id,
            resource.Title,
            resource.Description,
            resource.Url,
            resource.CreatedUtc,
            resource.Comments.Count(comment => comment.Status == CommentStatus.Approved),
            resource.Comments.Count(comment => comment.Status == CommentStatus.Pending));
    }

    public static LearningResourceDetailResponse ToDetailResponse(LearningResource resource)
    {
        return ToDetailResponse(resource, resource.Comments.Where(comment => comment.Status == CommentStatus.Approved));
    }

    public static LearningResourceDetailResponse ToDetailResponse(
        LearningResource resource,
        IEnumerable<Comment> comments)
    {
        return new LearningResourceDetailResponse(
            resource.Id,
            resource.Title,
            resource.Description,
            resource.Url,
            resource.CreatedUtc,
            comments
                .OrderBy(comment => comment.CreatedUtc)
                .Select(ToCommentResponse)
                .ToArray());
    }

    public static LearningResource ToEntity(CreateLearningResourceRequest request, DateTimeOffset createdUtc)
    {
        return new LearningResource
        {
            Id = Guid.NewGuid(),
            Title = request.Title!.Trim(),
            Description = request.Description!.Trim(),
            Url = request.Url!.Trim(),
            CreatedUtc = createdUtc
        };
    }

    public static void ApplyUpdate(LearningResource resource, UpdateLearningResourceRequest request)
    {
        resource.Title = request.Title!.Trim();
        resource.Description = request.Description!.Trim();
        resource.Url = request.Url!.Trim();
    }

    public static CommentResponse ToCommentResponse(Comment comment)
    {
        return new CommentResponse(
            comment.Id,
            comment.AuthorDisplayName,
            comment.AuthorType.ToString(),
            comment.Body,
            comment.Status.ToString(),
            comment.CreatedUtc,
            comment.ModeratedUtc);
    }

    public static PendingCommentResponse ToPendingCommentResponse(Comment comment)
    {
        return new PendingCommentResponse(
            comment.Id,
            comment.LearningResourceId,
            comment.LearningResource.Title,
            comment.AuthorDisplayName,
            comment.AuthorIdentityName,
            comment.AuthorType.ToString(),
            comment.Body,
            comment.Status.ToString(),
            comment.CreatedUtc);
    }
}
