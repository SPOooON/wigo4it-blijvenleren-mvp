using BlijvenLeren.App.Features.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace BlijvenLeren.App.Tests;

public sealed class LoginRequestBuilderTests
{
    [Fact]
    public void Build_WithoutIdentityProviderHint_UsesProtectedFallback()
    {
        var properties = LoginRequestBuilder.Build(null, null);

        Assert.Equal("/protected", properties.RedirectUri);
        Assert.False(properties.Items.ContainsKey("identity_provider_hint"));
    }

    [Fact]
    public void Build_WithIdentityProviderHint_StoresHintForLaterRedirectMutation()
    {
        var properties = LoginRequestBuilder.Build("/LearningResources", "github");

        Assert.Equal("/LearningResources", properties.RedirectUri);
        Assert.Equal("github", properties.Items["identity_provider_hint"]);
    }

    [Fact]
    public void ApplyIdentityProviderHint_WritesKeycloakProviderHintToProtocolMessage()
    {
        var properties = LoginRequestBuilder.Build("/LearningResources", "github");
        var protocolMessage = new OpenIdConnectMessage();

        LoginRequestBuilder.ApplyIdentityProviderHint(properties, protocolMessage);

        Assert.Equal("github", protocolMessage.GetParameter("kc_idp_hint"));
    }
}
