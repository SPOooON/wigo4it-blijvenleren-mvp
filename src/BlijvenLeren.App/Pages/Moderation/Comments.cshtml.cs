using BlijvenLeren.App.Contracts.V1;
using BlijvenLeren.App.Data;
using BlijvenLeren.App.Data.Entities;
using BlijvenLeren.App.Features.Comments;
using BlijvenLeren.App.Features.LearningResources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BlijvenLeren.App.Pages.Moderation;

[Authorize(Policy = "InternalUser")]
public sealed class CommentsModel(AppDbContext dbContext) : PageModel
{
    public IReadOnlyList<PendingCommentResponse> PendingComments { get; private set; } = [];

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        PendingComments = await LoadPendingCommentsAsync(cancellationToken);
    }

    public async Task<IActionResult> OnPostModerateAsync(Guid id, string action, CancellationToken cancellationToken)
    {
        var request = new ModerateCommentRequest(action);
        var requestErrors = CommentModerationValidator.ValidateRequest(request);
        if (requestErrors.Count > 0)
        {
            TempData["StatusMessage"] = requestErrors["Action"][0];
            return RedirectToPage();
        }

        var comment = await dbContext.Comments
            .Include(savedComment => savedComment.LearningResource)
            .SingleOrDefaultAsync(savedComment => savedComment.Id == id, cancellationToken);

        if (comment is null)
        {
            return NotFound();
        }

        var transitionError = CommentModerationValidator.ValidateTransition(comment);
        if (transitionError is not null)
        {
            TempData["StatusMessage"] = transitionError;
            return RedirectToPage();
        }

        CommentModerationValidator.TryParseAction(action, out var targetStatus);
        comment.Status = targetStatus;
        comment.ModeratedUtc = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        TempData["StatusMessage"] = targetStatus == CommentStatus.Approved
            ? "Comment approved."
            : "Comment rejected.";

        return RedirectToPage();
    }

    private async Task<IReadOnlyList<PendingCommentResponse>> LoadPendingCommentsAsync(CancellationToken cancellationToken)
    {
        var comments = await dbContext.Comments
            .AsNoTracking()
            .Include(comment => comment.LearningResource)
            .Where(comment => comment.AuthorType == CommentAuthorType.External && comment.Status == CommentStatus.Pending)
            .OrderBy(comment => comment.CreatedUtc)
            .ToListAsync(cancellationToken);

        return comments.Select(LearningResourceContractMapper.ToPendingCommentResponse).ToList();
    }
}
