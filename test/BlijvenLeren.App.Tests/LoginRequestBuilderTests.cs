using BlijvenLeren.App.Features.Auth;

namespace BlijvenLeren.App.Tests;

public sealed class LoginRequestBuilderTests
{
    [Fact]
    public void Build_WithoutIdentityProviderHint_UsesProtectedFallback()
    {
        var properties = LoginRequestBuilder.Build(null, null);

        Assert.Equal("/protected", properties.RedirectUri);
        Assert.False(properties.Parameters.ContainsKey("kc_idp_hint"));
    }

    [Fact]
    public void Build_WithIdentityProviderHint_AddsKeycloakProviderHint()
    {
        var properties = LoginRequestBuilder.Build("/LearningResources", "github");

        Assert.Equal("/LearningResources", properties.RedirectUri);
        Assert.Equal("github", properties.Parameters["kc_idp_hint"]);
    }
}
