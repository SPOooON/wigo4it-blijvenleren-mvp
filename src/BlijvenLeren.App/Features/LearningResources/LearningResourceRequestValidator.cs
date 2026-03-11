using BlijvenLeren.App.Contracts.V1;

namespace BlijvenLeren.App.Features.LearningResources;

public static class LearningResourceRequestValidator
{
    public static Dictionary<string, string[]> Validate(CreateLearningResourceRequest request)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);

        AddErrorIfInvalid(errors, nameof(request.Title), request.Title, 200, "Title is required.");
        AddErrorIfInvalid(errors, nameof(request.Description), request.Description, 2000, "Description is required.");
        AddErrorIfInvalid(errors, nameof(request.Url), request.Url, 2048, "Url is required.");

        Uri? uri = null;
        if (!string.IsNullOrWhiteSpace(request.Url)
            && !Uri.TryCreate(request.Url.Trim(), UriKind.Absolute, out uri))
        {
            AddError(errors, nameof(request.Url), "Url must be a valid absolute URI.");
        }
        else if (uri is not null && uri.Scheme is not ("http" or "https"))
        {
            AddError(errors, nameof(request.Url), "Url must use http or https.");
        }

        return errors;
    }

    private static void AddErrorIfInvalid(
        IDictionary<string, string[]> errors,
        string fieldName,
        string? value,
        int maxLength,
        string requiredMessage)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            AddError(errors, fieldName, requiredMessage);
            return;
        }

        if (value.Trim().Length > maxLength)
        {
            AddError(errors, fieldName, $"{fieldName} must be {maxLength} characters or fewer.");
        }
    }

    private static void AddError(IDictionary<string, string[]> errors, string fieldName, string message)
    {
        if (errors.TryGetValue(fieldName, out var existing))
        {
            errors[fieldName] = [.. existing, message];
            return;
        }

        errors[fieldName] = [message];
    }
}
