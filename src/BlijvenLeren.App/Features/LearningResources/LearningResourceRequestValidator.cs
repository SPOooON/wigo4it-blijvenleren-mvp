using BlijvenLeren.App.Contracts.V1;

namespace BlijvenLeren.App.Features.LearningResources;

public static class LearningResourceRequestValidator
{
    public static Dictionary<string, string[]> Validate(CreateLearningResourceRequest request)
    {
        return Validate(request.Title, request.Description, request.Url);
    }

    public static Dictionary<string, string[]> Validate(UpdateLearningResourceRequest request)
    {
        return Validate(request.Title, request.Description, request.Url);
    }

    private static Dictionary<string, string[]> Validate(string? title, string? description, string? url)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);

        AddErrorIfInvalid(errors, "Title", title, 200, "Title is required.");
        AddErrorIfInvalid(errors, "Description", description, 2000, "Description is required.");
        AddErrorIfInvalid(errors, "Url", url, 2048, "Url is required.");

        Uri? uri = null;
        if (!string.IsNullOrWhiteSpace(url)
            && !Uri.TryCreate(url.Trim(), UriKind.Absolute, out uri))
        {
            AddError(errors, "Url", "Url must be a valid absolute URI.");
        }
        else if (uri is not null && uri.Scheme is not ("http" or "https"))
        {
            AddError(errors, "Url", "Url must use http or https.");
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
