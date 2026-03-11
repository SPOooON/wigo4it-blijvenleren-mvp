using Microsoft.AspNetCore.Authentication;

namespace BlijvenLeren.App.Features.Auth;

public static class LoginRequestBuilder
{
    public static AuthenticationProperties Build(string? returnUrl, string? identityProviderAlias)
    {
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = true,
            RedirectUri = string.IsNullOrWhiteSpace(returnUrl) ? "/protected" : returnUrl
        };

        if (!string.IsNullOrWhiteSpace(identityProviderAlias))
        {
            authProperties.Parameters["kc_idp_hint"] = identityProviderAlias;
        }

        return authProperties;
    }
}
