using BlijvenLeren.App.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BlijvenLeren.App.Pages.LearningResources;

public sealed class IndexModel(AppDbContext dbContext) : PageModel
{
    public IReadOnlyList<LearningResourceListItemViewModel> Resources { get; private set; } = [];

    public bool CanManageResources => User.IsInRole("internal-user");

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        Resources = await dbContext.LearningResources
            .AsNoTracking()
            .Include(resource => resource.Comments)
            .OrderBy(resource => resource.Title)
            .Select(resource => new LearningResourceListItemViewModel(
                resource.Id,
                resource.Title,
                resource.Description,
                resource.Url,
                resource.CreatedUtc,
                resource.Comments.Count(comment => comment.Status == Data.Entities.CommentStatus.Approved),
                resource.Comments.Count(comment => comment.Status == Data.Entities.CommentStatus.Pending)))
            .ToListAsync(cancellationToken);
    }
}

public sealed record LearningResourceListItemViewModel(
    Guid Id,
    string Title,
    string Description,
    string Url,
    DateTimeOffset CreatedUtc,
    int ApprovedCommentCount,
    int PendingCommentCount);
