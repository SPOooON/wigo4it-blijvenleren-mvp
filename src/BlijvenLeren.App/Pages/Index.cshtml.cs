using BlijvenLeren.App.Configuration;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace BlijvenLeren.App.Pages;

public class IndexModel : PageModel
{
    private readonly RuntimeOptions _runtimeOptions;

    public IndexModel(IOptions<RuntimeOptions> runtimeOptions)
    {
        _runtimeOptions = runtimeOptions.Value;
    }

    public string AppBaseUrl => $"{Request.Scheme}://{Request.Host}";

    public string ApiDocsUrl => $"{AppBaseUrl}/docs";

    public string AuthMeUrl => $"{AppBaseUrl}/api/auth/me";

    public string DependencyProbeUrl => $"{AppBaseUrl}/api/health/dependencies";

    public string LearningResourcesUrl => $"{AppBaseUrl}/LearningResources";

    public string DatabaseHost => _runtimeOptions.Database.Host;

    public int DatabasePort => _runtimeOptions.Database.Port;

    public string IdentityProviderAuthority => _runtimeOptions.IdentityProvider.Authority;

    public string? Error => Request.Query["error"];

    public string RoleSummary => string.Join(", ", User.FindAll(ClaimTypes.Role).Select(claim => claim.Value));

    public void OnGet()
    {
    }
}
