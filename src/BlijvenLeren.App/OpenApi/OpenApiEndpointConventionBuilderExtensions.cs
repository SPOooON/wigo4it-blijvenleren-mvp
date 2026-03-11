using Microsoft.AspNetCore.Builder;
using Microsoft.OpenApi;

namespace BlijvenLeren.App.OpenApi;

public static class OpenApiEndpointConventionBuilderExtensions
{
    public static TBuilder WithBearerAuthOpenApi<TBuilder>(this TBuilder builder, string? description = null)
        where TBuilder : IEndpointConventionBuilder
    {
        return builder.AddOpenApiOperationTransformer((operation, _, _) =>
        {
            if (!string.IsNullOrWhiteSpace(description))
            {
                operation.Description = string.IsNullOrWhiteSpace(operation.Description)
                    ? description
                    : $"{operation.Description}\n\n{description}";
            }

            operation.Responses ??= new OpenApiResponses();
            operation.Responses.TryAdd("401", new OpenApiResponse
            {
                Description = "Missing or invalid bearer token."
            });
            operation.Responses.TryAdd("403", new OpenApiResponse
            {
                Description = "Authenticated, but missing the required role."
            });

            return Task.CompletedTask;
        });
    }
}
