using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using BlijvenLeren.App.Configuration;
using Microsoft.Extensions.Options;

namespace BlijvenLeren.App.Security;

public sealed class ClaimsPrincipalFactory(IOptions<AuthOptions> authOptions)
{
    private readonly AuthOptions _authOptions = authOptions.Value;

    public ClaimsPrincipal CreateFromAccessToken(string accessToken)
    {
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(accessToken);

        var claims = token.Claims.ToList();

        var preferredUsername = token.Claims.FirstOrDefault(claim => claim.Type == "preferred_username")?.Value;
        if (!string.IsNullOrWhiteSpace(preferredUsername))
        {
            claims.Add(new Claim(ClaimTypes.Name, preferredUsername));
        }

        foreach (var role in GetRealmRoles(token.Claims))
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
            claims.Add(new Claim("roles", role));
        }

        var identity = new ClaimsIdentity(claims, "Cookies", ClaimTypes.Name, ClaimTypes.Role);
        return new ClaimsPrincipal(identity);
    }

    public void AddRoleClaimsFromRealmAccess(ClaimsIdentity identity)
    {
        foreach (var role in GetRealmRoles(identity.Claims))
        {
            if (!identity.HasClaim(ClaimTypes.Role, role))
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, role));
            }

            if (!identity.HasClaim("roles", role))
            {
                identity.AddClaim(new Claim("roles", role));
            }
        }
    }

    public void AddRoleClaimsFromAccessToken(string accessToken, ClaimsIdentity identity)
    {
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(accessToken);

        foreach (var role in GetRealmRoles(token.Claims))
        {
            if (!identity.HasClaim(ClaimTypes.Role, role))
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, role));
            }

            if (!identity.HasClaim("roles", role))
            {
                identity.AddClaim(new Claim("roles", role));
            }
        }

        var preferredUsername = token.Claims.FirstOrDefault(claim => claim.Type == "preferred_username")?.Value;
        if (!string.IsNullOrWhiteSpace(preferredUsername) && !identity.HasClaim(ClaimTypes.Name, preferredUsername))
        {
            identity.AddClaim(new Claim(ClaimTypes.Name, preferredUsername));
        }
    }

    public bool IsInternalUser(ClaimsPrincipal user) => user.IsInRole(_authOptions.InternalUserRole);

    public bool IsExternalContributor(ClaimsPrincipal user) => user.IsInRole(_authOptions.ExternalContributorRole);

    private static IEnumerable<string> GetRealmRoles(IEnumerable<Claim> claims)
    {
        var realmAccess = claims.FirstOrDefault(claim => claim.Type == "realm_access")?.Value;
        if (string.IsNullOrWhiteSpace(realmAccess))
        {
            return [];
        }

        try
        {
            using var document = JsonDocument.Parse(realmAccess);
            if (!document.RootElement.TryGetProperty("roles", out var rolesElement) || rolesElement.ValueKind != JsonValueKind.Array)
            {
                return [];
            }

            return rolesElement.EnumerateArray()
                .Where(role => role.ValueKind == JsonValueKind.String)
                .Select(role => role.GetString())
                .Where(role => !string.IsNullOrWhiteSpace(role))
                .Cast<string>()
                .ToArray();
        }
        catch (JsonException)
        {
            return [];
        }
    }
}
