using BlijvenLeren.App.Contracts.V1;
using BlijvenLeren.App.Data.Entities;

namespace BlijvenLeren.App.Features.Comments;

public static class CommentModerationValidator
{
    public static Dictionary<string, string[]> ValidateRequest(ModerateCommentRequest request)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);

        if (string.IsNullOrWhiteSpace(request.Action))
        {
            errors["Action"] = ["Action is required."];
            return errors;
        }

        if (!TryParseAction(request.Action, out _))
        {
            errors["Action"] = ["Action must be either approve or reject."];
        }

        return errors;
    }

    public static string? ValidateTransition(Comment comment)
    {
        if (comment.AuthorType != CommentAuthorType.External)
        {
            return "Only external comments can be moderated.";
        }

        if (comment.Status != CommentStatus.Pending)
        {
            return "Only pending comments can be moderated.";
        }

        return null;
    }

    public static bool TryParseAction(string action, out CommentStatus status)
    {
        if (string.Equals(action, "approve", StringComparison.OrdinalIgnoreCase))
        {
            status = CommentStatus.Approved;
            return true;
        }

        if (string.Equals(action, "reject", StringComparison.OrdinalIgnoreCase))
        {
            status = CommentStatus.Rejected;
            return true;
        }

        status = default;
        return false;
    }
}
