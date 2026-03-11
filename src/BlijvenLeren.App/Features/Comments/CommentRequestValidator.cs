using BlijvenLeren.App.Contracts.V1;

namespace BlijvenLeren.App.Features.Comments;

public static class CommentRequestValidator
{
    public static Dictionary<string, string[]> Validate(CreateCommentRequest request)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);

        if (string.IsNullOrWhiteSpace(request.Body))
        {
            errors["Body"] = ["Body is required."];
            return errors;
        }

        if (request.Body.Trim().Length > 2000)
        {
            errors["Body"] = ["Body must be 2000 characters or fewer."];
        }

        return errors;
    }
}
