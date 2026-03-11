using BlijvenLeren.App.Contracts.V1;
using BlijvenLeren.App.Features.LearningResources;

namespace BlijvenLeren.App.Tests;

public sealed class LearningResourceRequestValidatorTests
{
    [Fact]
    public void Validate_ReturnsErrorsForMissingFields()
    {
        var result = LearningResourceRequestValidator.Validate(new CreateLearningResourceRequest(null, "", " "));

        Assert.Equal(3, result.Count);
        Assert.Contains("Title", result.Keys);
        Assert.Contains("Description", result.Keys);
        Assert.Contains("Url", result.Keys);
    }

    [Fact]
    public void Validate_ReturnsErrorForUnsupportedUrlScheme()
    {
        var result = LearningResourceRequestValidator.Validate(
            new CreateLearningResourceRequest("Title", "Description", "ftp://example.com/resource"));

        Assert.Contains("Url must use http or https.", result["Url"]);
    }

    [Fact]
    public void Validate_ReturnsNoErrorsForValidRequest()
    {
        var result = LearningResourceRequestValidator.Validate(
            new CreateLearningResourceRequest("Title", "Description", "https://example.com/resource"));

        Assert.Empty(result);
    }
}
