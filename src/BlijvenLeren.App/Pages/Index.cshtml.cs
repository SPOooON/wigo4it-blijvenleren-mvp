using BlijvenLeren.App.Configuration;
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

    public string DatabaseHost => _runtimeOptions.Database.Host;

    public int DatabasePort => _runtimeOptions.Database.Port;

    public string IdentityProviderAuthority => _runtimeOptions.IdentityProvider.Authority;

    public void OnGet()
    {
    }
}
