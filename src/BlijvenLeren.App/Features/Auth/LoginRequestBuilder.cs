using Microsoft.AspNetCore.Authentication;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace BlijvenLeren.App.Features.Auth;

public static class LoginRequestBuilder
{
    private const string IdentityProviderHintItemKey = "identity_provider_hint";

    public static AuthenticationProperties Build(string? returnUrl, string? identityProviderAlias)
    {
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = true,
            RedirectUri = string.IsNullOrWhiteSpace(returnUrl) ? "/protected" : returnUrl
        };

        if (!string.IsNullOrWhiteSpace(identityProviderAlias))
        {
            authProperties.Items[IdentityProviderHintItemKey] = identityProviderAlias;
        }

        return authProperties;
    }

    public static void ApplyIdentityProviderHint(
        AuthenticationProperties? authProperties,
        OpenIdConnectMessage protocolMessage)
    {
        if (authProperties?.Items.TryGetValue(IdentityProviderHintItemKey, out var identityProviderAlias) != true
            || string.IsNullOrWhiteSpace(identityProviderAlias))
        {
            return;
        }

        protocolMessage.SetParameter("kc_idp_hint", identityProviderAlias);
    }
}
