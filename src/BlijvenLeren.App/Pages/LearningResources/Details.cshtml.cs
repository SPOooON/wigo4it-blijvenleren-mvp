using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using BlijvenLeren.App.Contracts.V1;
using BlijvenLeren.App.Data;
using BlijvenLeren.App.Data.Entities;
using BlijvenLeren.App.Features.Comments;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BlijvenLeren.App.Pages.LearningResources;

public sealed class DetailsModel(AppDbContext dbContext) : PageModel
{
    [BindProperty]
    public CommentFormModel CommentInput { get; set; } = new();

    public LearningResourceDetailsViewModel? Resource { get; private set; }

    public bool CanManageResources => User.IsInRole("internal-user");

    public bool CanComment => User.Identity?.IsAuthenticated ?? false;

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

    public async Task<IActionResult> OnPostCommentAsync(Guid id, CancellationToken cancellationToken)
    {
        if (!(User.Identity?.IsAuthenticated ?? false))
        {
            return Challenge(
                new AuthenticationProperties
                {
                    RedirectUri = $"/LearningResources/Details/{id}"
                },
                [OpenIdConnectDefaults.AuthenticationScheme]);
        }

        var request = new CreateCommentRequest(CommentInput.Body);
        AddValidationErrors(CommentRequestValidator.Validate(request));

        if (!ModelState.IsValid)
        {
            Resource = await LoadResourceAsync(id, cancellationToken);
            return Resource is null ? NotFound() : Page();
        }

        var resourceExists = await dbContext.LearningResources
            .AnyAsync(learningResource => learningResource.Id == id, cancellationToken);

        if (!resourceExists)
        {
            return NotFound();
        }

        var comment = CommentSubmissionFactory.Create(id, User, request, DateTimeOffset.UtcNow);
        dbContext.Comments.Add(comment);
        await dbContext.SaveChangesAsync(cancellationToken);

        TempData["StatusMessage"] = comment.Status == CommentStatus.Approved
            ? "Comment added and visible immediately."
            : "Comment submitted and waiting for moderation.";

        return RedirectToPage("/LearningResources/Details", new { id });
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
                    .Where(comment => comment.Status == CommentStatus.Approved)
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

    private void AddValidationErrors(IReadOnlyDictionary<string, string[]> errors)
    {
        foreach (var (fieldName, messages) in errors)
        {
            foreach (var message in messages)
            {
                ModelState.AddModelError($"CommentInput.{fieldName}", message);
            }
        }
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

public sealed class CommentFormModel
{
    public string? Body { get; set; }
}
