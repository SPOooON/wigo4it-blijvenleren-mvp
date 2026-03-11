using BlijvenLeren.App.Contracts.V1;
using BlijvenLeren.App.Features.Comments;

namespace BlijvenLeren.App.Tests;

public sealed class CommentRequestValidatorTests
{
    [Fact]
    public void Validate_ReturnsErrorForMissingBody()
    {
        var result = CommentRequestValidator.Validate(new CreateCommentRequest(" "));

        Assert.Single(result);
        Assert.Contains("Body", result.Keys);
    }

    [Fact]
    public void Validate_ReturnsNoErrorsForValidBody()
    {
        var result = CommentRequestValidator.Validate(new CreateCommentRequest("Useful follow-up material."));

        Assert.Empty(result);
    }
}
