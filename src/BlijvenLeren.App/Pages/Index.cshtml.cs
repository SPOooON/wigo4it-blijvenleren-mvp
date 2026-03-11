using BlijvenLeren.App.Configuration;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;

namespace BlijvenLeren.App.Pages;

public class IndexModel : PageModel
{
    private readonly RuntimeOptions _runtimeOptions;
    private readonly AuthOptions _authOptions;

    public IndexModel(IOptions<RuntimeOptions> runtimeOptions, IOptions<AuthOptions> authOptions)
    {
        _runtimeOptions = runtimeOptions.Value;
        _authOptions = authOptions.Value;
    }

    public string AppBaseUrl => $"{Request.Scheme}://{Request.Host}";

    public string ApiDocsUrl => $"{AppBaseUrl}/docs";

    public string AuthMeUrl => $"{AppBaseUrl}/api/auth/me";

    public string DependencyProbeUrl => $"{AppBaseUrl}/api/health/dependencies";

    public string LearningResourcesUrl => $"{AppBaseUrl}/LearningResources";

    public string ReviewerFacingDatabaseHost => $"{Request.Host.Host}:{_runtimeOptions.Database.Port}";

    public string ReviewerFacingIdentityProviderAuthority => _authOptions.Authority;

    public string? PreferredExternalIdentityProviderAlias { get; private set; }

    public string? PreferredExternalIdentityProviderDisplayName { get; private set; }

    public string SocialLoginUrl { get; private set; } = "/account/login?returnUrl=%2Fprotected";

    public string DemoLoginUrl { get; private set; } = "/account/login?returnUrl=%2Fprotected";

    public string? Error => Request.Query["error"];

    public string RoleSummary => string.Join(", ", User.FindAll(ClaimTypes.Role).Select(claim => claim.Value));

    public bool HasPreferredExternalIdentityProvider => !string.IsNullOrWhiteSpace(PreferredExternalIdentityProviderAlias);

    public void OnGet()
    {
        PreferredExternalIdentityProviderAlias = _authOptions.PreferredExternalIdentityProviderAlias;
        PreferredExternalIdentityProviderDisplayName = _authOptions.PreferredExternalIdentityProviderDisplayName;

        DemoLoginUrl = BuildLoginUrl(null);
        SocialLoginUrl = BuildLoginUrl(PreferredExternalIdentityProviderAlias);
    }

    private static string BuildLoginUrl(string? providerAlias)
    {
        var query = new Dictionary<string, string?>
        {
            ["returnUrl"] = "/protected"
        };

        if (!string.IsNullOrWhiteSpace(providerAlias))
        {
            query["provider"] = providerAlias;
        }

        return QueryHelpers.AddQueryString("/account/login", query);
    }
}
