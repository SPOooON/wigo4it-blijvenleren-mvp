using BlijvenLeren.App.Contracts.V1;
using BlijvenLeren.App.Data;
using BlijvenLeren.App.Features.LearningResources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BlijvenLeren.App.Pages.LearningResources;

[Authorize(Policy = "InternalUser")]
public sealed class CreateModel(AppDbContext dbContext) : PageModel
{
    [BindProperty]
    public LearningResourceFormModel Input { get; set; } = new();

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        var request = new CreateLearningResourceRequest(Input.Title, Input.Description, Input.Url);
        AddValidationErrors(LearningResourceRequestValidator.Validate(request));

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var resource = LearningResourceContractMapper.ToEntity(request, DateTimeOffset.UtcNow);
        dbContext.LearningResources.Add(resource);
        await dbContext.SaveChangesAsync(cancellationToken);

        TempData["StatusMessage"] = "Learning resource created.";
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
