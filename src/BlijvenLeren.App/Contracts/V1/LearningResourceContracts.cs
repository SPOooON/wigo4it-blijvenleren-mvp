namespace BlijvenLeren.App.Contracts.V1;

public sealed record CreateLearningResourceRequest(
    string? Title,
    string? Description,
    string? Url);

public sealed record LearningResourceListItemResponse(
    Guid Id,
    string Title,
    string Description,
    string Url,
    DateTimeOffset CreatedUtc,
    int ApprovedCommentCount,
    int PendingCommentCount);

public sealed record LearningResourceDetailResponse(
    Guid Id,
    string Title,
    string Description,
    string Url,
    DateTimeOffset CreatedUtc,
    IReadOnlyList<CommentResponse> Comments);

public sealed record CommentResponse(
    Guid Id,
    string AuthorDisplayName,
    string AuthorType,
    string Body,
    string Status,
    DateTimeOffset CreatedUtc,
    DateTimeOffset? ModeratedUtc);
