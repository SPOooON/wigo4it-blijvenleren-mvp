using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BlijvenLeren.App.Pages;

[Authorize]
public sealed class ProtectedModel : PageModel
{
    public string Username => User.Identity?.Name ?? "(unknown)";

    public string RoleSummary => string.Join(", ", User.FindAll(ClaimTypes.Role).Select(claim => claim.Value));

    public void OnGet()
    {
    }
}
