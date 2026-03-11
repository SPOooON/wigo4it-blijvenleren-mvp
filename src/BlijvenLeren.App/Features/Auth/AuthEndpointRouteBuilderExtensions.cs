using System.Security.Claims;
using BlijvenLeren.App.OpenApi;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace BlijvenLeren.App.Features.Auth;

public static class AuthEndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(
            "/account/login",
            (string? returnUrl) =>
            {
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    RedirectUri = string.IsNullOrWhiteSpace(returnUrl) ? "/protected" : returnUrl
                };
                return Results.Challenge(authProperties, [OpenIdConnectDefaults.AuthenticationScheme]);
            });

        endpoints.MapPost(
            "/account/logout",
            () =>
            {
                var authProperties = new AuthenticationProperties
                {
                    RedirectUri = "/"
                };
                return Results.SignOut(
                    authProperties,
                    [CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme]);
            });

        endpoints.MapGet(
            "/api/auth/me",
            (ClaimsPrincipal user) => Results.Ok(new
            {
                authenticated = user.Identity?.IsAuthenticated ?? false,
                username = user.Identity?.Name,
                roles = user.FindAll(ClaimTypes.Role).Select(claim => claim.Value).Distinct()
            }))
            .RequireAuthorization()
            .WithBearerAuthOpenApi("Requires a bearer token from the local Keycloak realm.")
            .WithSummary("Return the current authenticated user and mapped roles.");

        endpoints.MapGet("/api/auth/internal", () => Results.Ok(new { status = "ok", role = "internal-user" }))
            .RequireAuthorization("InternalUser")
            .WithBearerAuthOpenApi("Requires an internal-user bearer token.")
            .WithSummary("Verify access for the internal-user role.");

        endpoints.MapGet("/api/auth/external", () => Results.Ok(new { status = "ok", role = "external-contributor" }))
            .RequireAuthorization("ExternalContributor")
            .WithBearerAuthOpenApi("Requires an external-contributor bearer token.")
            .WithSummary("Verify access for the external-contributor role.");

        return endpoints;
    }
}
