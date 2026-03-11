namespace BlijvenLeren.App.Data.Entities;

public sealed class LearningResource
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Url { get; set; } = string.Empty;

    public DateTimeOffset CreatedUtc { get; set; }

    public List<Comment> Comments { get; set; } = [];
}
