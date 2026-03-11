using BlijvenLeren.App.Data;
using BlijvenLeren.App.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BlijvenLeren.App.Pages.LearningResources;

public sealed class DetailsModel(AppDbContext dbContext) : PageModel
{
    public LearningResourceDetailsViewModel? Resource { get; private set; }

    public bool CanManageResources => User.IsInRole("internal-user");

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken cancellationToken)
    {
        Resource = await LoadResourceAsync(id, cancellationToken);
        return Resource is null ? NotFound() : Page();
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        if (!User.IsInRole("internal-user"))
        {
            return Forbid();
        }

        var resource = await dbContext.LearningResources
            .SingleOrDefaultAsync(learningResource => learningResource.Id == id, cancellationToken);

        if (resource is null)
        {
            return NotFound();
        }

        dbContext.LearningResources.Remove(resource);
        await dbContext.SaveChangesAsync(cancellationToken);

        TempData["StatusMessage"] = "Learning resource deleted.";
        return RedirectToPage("/LearningResources/Index");
    }

    private async Task<LearningResourceDetailsViewModel?> LoadResourceAsync(Guid id, CancellationToken cancellationToken)
    {
        return await dbContext.LearningResources
            .AsNoTracking()
            .Include(resource => resource.Comments)
            .Where(resource => resource.Id == id)
            .Select(resource => new LearningResourceDetailsViewModel(
                resource.Id,
                resource.Title,
                resource.Description,
                resource.Url,
                resource.CreatedUtc,
                resource.Comments
                    .OrderBy(comment => comment.CreatedUtc)
                    .Select(comment => new LearningResourceCommentViewModel(
                        comment.AuthorDisplayName,
                        comment.AuthorType.ToString(),
                        comment.Body,
                        comment.Status.ToString(),
                        comment.CreatedUtc,
                        comment.ModeratedUtc))
                    .ToList()))
            .SingleOrDefaultAsync(cancellationToken);
    }
}

public sealed record LearningResourceDetailsViewModel(
    Guid Id,
    string Title,
    string Description,
    string Url,
    DateTimeOffset CreatedUtc,
    IReadOnlyList<LearningResourceCommentViewModel> Comments);

public sealed record LearningResourceCommentViewModel(
    string AuthorDisplayName,
    string AuthorType,
    string Body,
    string Status,
    DateTimeOffset CreatedUtc,
    DateTimeOffset? ModeratedUtc);
