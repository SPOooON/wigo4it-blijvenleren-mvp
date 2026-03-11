using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace BlijvenLeren.App.OpenApi;

public static class OpenApiDocumentConfiguration
{
    public const string BearerSchemeName = "BearerAuth";

    public static void Configure(OpenApiOptions options)
    {
        options.AddDocumentTransformer((document, _, _) =>
        {
            document.Info = new OpenApiInfo
            {
                Title = "BlijvenLeren API",
                Version = "v1",
                Description = "Local MVP API surface for review. Use a bearer token from the local Keycloak realm for protected endpoints."
            };

            var apiPaths = new OpenApiPaths();
            foreach (var path in document.Paths.Where(path => path.Key.StartsWith("/api", StringComparison.OrdinalIgnoreCase)))
            {
                apiPaths.Add(path.Key, path.Value);
            }

            document.Paths = apiPaths;
            document.Components ??= new OpenApiComponents();
            document.Components.SecuritySchemes = new Dictionary<string, IOpenApiSecurityScheme>
            {
                [BearerSchemeName] = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Paste a bearer token from the local Keycloak realm. Protected endpoints use the internal-user or external-contributor roles."
                }
            };

            return Task.CompletedTask;
        });
    }
}
