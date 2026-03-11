using System.Security.Claims;
using BlijvenLeren.App.Contracts.V1;
using BlijvenLeren.App.Data.Entities;

namespace BlijvenLeren.App.Features.Comments;

public static class CommentSubmissionFactory
{
    public static Comment Create(Guid learningResourceId, ClaimsPrincipal user, CreateCommentRequest request, DateTimeOffset createdUtc)
    {
        var authorIdentityName = user.FindFirstValue("preferred_username")
            ?? user.Identity?.Name
            ?? "unknown";
        var isInternalUser = user.IsInRole("internal-user");

        return new Comment
        {
            Id = Guid.NewGuid(),
            LearningResourceId = learningResourceId,
            AuthorDisplayName = authorIdentityName,
            AuthorIdentityName = authorIdentityName,
            AuthorType = isInternalUser ? CommentAuthorType.Internal : CommentAuthorType.External,
            Body = request.Body!.Trim(),
            Status = isInternalUser ? CommentStatus.Approved : CommentStatus.Pending,
            CreatedUtc = createdUtc,
            ModeratedUtc = isInternalUser ? createdUtc : null
        };
    }
}
