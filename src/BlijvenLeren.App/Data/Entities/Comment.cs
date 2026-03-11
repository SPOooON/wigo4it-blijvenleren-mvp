namespace BlijvenLeren.App.Data.Entities;

public sealed class Comment
{
    public Guid Id { get; set; }

    public Guid LearningResourceId { get; set; }

    public string AuthorDisplayName { get; set; } = string.Empty;

    public CommentAuthorType AuthorType { get; set; }

    public string Body { get; set; } = string.Empty;

    public CommentStatus Status { get; set; }

    public DateTimeOffset CreatedUtc { get; set; }

    public DateTimeOffset? ModeratedUtc { get; set; }

    public LearningResource LearningResource { get; set; } = null!;
}
