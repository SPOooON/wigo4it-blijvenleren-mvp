using BlijvenLeren.App.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace BlijvenLeren.App.Data;

public sealed class DemoDataSeeder(AppDbContext dbContext)
{
    public async Task<SeedResult> SeedAsync(bool resetExistingData, CancellationToken cancellationToken)
    {
        if (resetExistingData)
        {
            dbContext.Comments.RemoveRange(dbContext.Comments);
            dbContext.LearningResources.RemoveRange(dbContext.LearningResources);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        else if (await dbContext.LearningResources.AnyAsync(cancellationToken))
        {
            return await BuildResultAsync("unchanged", cancellationToken);
        }

        var seededResources = BuildSeedResources();
        dbContext.LearningResources.AddRange(seededResources);
        await dbContext.SaveChangesAsync(cancellationToken);

        return await BuildResultAsync(resetExistingData ? "reseeded" : "seeded", cancellationToken);
    }

    private async Task<SeedResult> BuildResultAsync(string status, CancellationToken cancellationToken)
    {
        var resourceCount = await dbContext.LearningResources.CountAsync(cancellationToken);
        var commentCount = await dbContext.Comments.CountAsync(cancellationToken);

        var statusCounts = await dbContext.Comments
            .GroupBy(comment => comment.Status)
            .Select(group => new CommentStatusCount(group.Key.ToString(), group.Count()))
            .ToListAsync(cancellationToken);

        return new SeedResult(status, resourceCount, commentCount, statusCounts);
    }

    private static IReadOnlyList<LearningResource> BuildSeedResources()
    {
        return
        [
            new LearningResource
            {
                Id = Guid.Parse("22f5008e-e4c0-4494-92e1-e2c5b4f76c91"),
                Title = "Intro to Azure Fundamentals",
                Description = "Starter material for colleagues who need a fast cloud platform overview.",
                Url = "https://learn.microsoft.com/training/paths/microsoft-azure-fundamentals-describe-cloud-concepts/",
                CreatedUtc = DateTimeOffset.Parse("2026-03-01T09:00:00Z"),
                Comments =
                [
                    new Comment
                    {
                        Id = Guid.Parse("6226c6d0-84af-4cca-8d70-a2501002ab7b"),
                        AuthorDisplayName = "Platform Coach",
                        AuthorType = CommentAuthorType.Internal,
                        Body = "Good first stop for new teammates before deeper platform tracks.",
                        Status = CommentStatus.Approved,
                        CreatedUtc = DateTimeOffset.Parse("2026-03-02T08:30:00Z"),
                        ModeratedUtc = DateTimeOffset.Parse("2026-03-02T08:30:00Z")
                    },
                    new Comment
                    {
                        Id = Guid.Parse("858147fc-37c2-4d69-a85d-c9fdab5d46aa"),
                        AuthorDisplayName = "External Guest",
                        AuthorType = CommentAuthorType.External,
                        Body = "Could use a note about which modules are most relevant for non-engineers.",
                        Status = CommentStatus.Pending,
                        CreatedUtc = DateTimeOffset.Parse("2026-03-03T14:15:00Z")
                    }
                ]
            },
            new LearningResource
            {
                Id = Guid.Parse("74f2ef58-f5fb-4760-ae9d-95c82c6e0a6f"),
                Title = "Secure Coding in .NET",
                Description = "Practical reference material for common ASP.NET Core security pitfalls and mitigations.",
                Url = "https://learn.microsoft.com/aspnet/core/security/",
                CreatedUtc = DateTimeOffset.Parse("2026-03-01T10:00:00Z"),
                Comments =
                [
                    new Comment
                    {
                        Id = Guid.Parse("a11f6c9f-2e98-43a0-95da-70bd767f73c9"),
                        AuthorDisplayName = "Security Reviewer",
                        AuthorType = CommentAuthorType.Internal,
                        Body = "Keep this visible in the demo because it connects well to the documented security shortcuts.",
                        Status = CommentStatus.Approved,
                        CreatedUtc = DateTimeOffset.Parse("2026-03-02T10:45:00Z"),
                        ModeratedUtc = DateTimeOffset.Parse("2026-03-02T10:45:00Z")
                    },
                    new Comment
                    {
                        Id = Guid.Parse("86c8cdab-0c38-4b97-bc47-9bafcf046ea7"),
                        AuthorDisplayName = "Community Contributor",
                        AuthorType = CommentAuthorType.External,
                        Body = "I submitted a related OWASP resource, but this one is probably enough for the MVP.",
                        Status = CommentStatus.Rejected,
                        CreatedUtc = DateTimeOffset.Parse("2026-03-04T11:20:00Z"),
                        ModeratedUtc = DateTimeOffset.Parse("2026-03-05T09:00:00Z")
                    }
                ]
            },
            new LearningResource
            {
                Id = Guid.Parse("fb8fb6b5-ccf8-4e48-abfe-0ea184d37b90"),
                Title = "Effective Feedback for Peer Reviews",
                Description = "Short training material focused on giving actionable code-review feedback.",
                Url = "https://google.github.io/eng-practices/review/reviewer/",
                CreatedUtc = DateTimeOffset.Parse("2026-03-01T11:00:00Z"),
                Comments =
                [
                    new Comment
                    {
                        Id = Guid.Parse("d2060ff2-f743-4f34-a3c9-c8f69bceffb0"),
                        AuthorDisplayName = "Team Lead",
                        AuthorType = CommentAuthorType.Internal,
                        Body = "Useful reference for the reviewability goals in this assignment repo.",
                        Status = CommentStatus.Approved,
                        CreatedUtc = DateTimeOffset.Parse("2026-03-02T12:10:00Z"),
                        ModeratedUtc = DateTimeOffset.Parse("2026-03-02T12:10:00Z")
                    },
                    new Comment
                    {
                        Id = Guid.Parse("71f04c2c-8af7-42cf-9778-6b4ee2559f5b"),
                        AuthorDisplayName = "External Reviewer",
                        AuthorType = CommentAuthorType.External,
                        Body = "Would be nice to add one Dutch-language review article later.",
                        Status = CommentStatus.Pending,
                        CreatedUtc = DateTimeOffset.Parse("2026-03-06T07:50:00Z")
                    }
                ]
            }
        ];
    }
}

public sealed record SeedResult(
    string Status,
    int ResourceCount,
    int CommentCount,
    IReadOnlyList<CommentStatusCount> CommentStatusCounts);

public sealed record CommentStatusCount(string Status, int Count);
