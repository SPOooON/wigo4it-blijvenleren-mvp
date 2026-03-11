using BlijvenLeren.App.Contracts.V1;
using BlijvenLeren.App.Data;
using BlijvenLeren.App.Features.LearningResources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BlijvenLeren.App.Pages.LearningResources;

[Authorize(Policy = "InternalUser")]
public sealed class EditModel(AppDbContext dbContext) : PageModel
{
    [BindProperty]
    public LearningResourceFormModel Input { get; set; } = new();

    public Guid ResourceId { get; private set; }

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken cancellationToken)
    {
        var resource = await dbContext.LearningResources
            .AsNoTracking()
            .SingleOrDefaultAsync(learningResource => learningResource.Id == id, cancellationToken);

        if (resource is null)
        {
            return NotFound();
        }

        ResourceId = resource.Id;
        Input = new LearningResourceFormModel
        {
            Title = resource.Title,
            Description = resource.Description,
            Url = resource.Url
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(Guid id, CancellationToken cancellationToken)
    {
        ResourceId = id;

        var request = new UpdateLearningResourceRequest(Input.Title, Input.Description, Input.Url);
        AddValidationErrors(LearningResourceRequestValidator.Validate(request));

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var resource = await dbContext.LearningResources
            .SingleOrDefaultAsync(learningResource => learningResource.Id == id, cancellationToken);

        if (resource is null)
        {
            return NotFound();
        }

        LearningResourceContractMapper.ApplyUpdate(resource, request);
        await dbContext.SaveChangesAsync(cancellationToken);

        TempData["StatusMessage"] = "Learning resource updated.";
        return RedirectToPage("/LearningResources/Details", new { id = resource.Id });
    }

    private void AddValidationErrors(IReadOnlyDictionary<string, string[]> errors)
    {
        foreach (var (fieldName, messages) in errors)
        {
            foreach (var message in messages)
            {
                ModelState.AddModelError($"Input.{fieldName}", message);
            }
        }
    }
}
