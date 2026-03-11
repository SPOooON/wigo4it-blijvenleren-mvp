using BlijvenLeren.App.Contracts.V1;
using BlijvenLeren.App.Data;
using BlijvenLeren.App.OpenApi;
using Microsoft.EntityFrameworkCore;

namespace BlijvenLeren.App.Features.LearningResources;

public static class LearningResourceEndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapLearningResourceEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(
            "/api/v1/learning-resources",
            async (CreateLearningResourceRequest request, AppDbContext dbContext, CancellationToken cancellationToken) =>
            {
                var errors = LearningResourceRequestValidator.Validate(request);
                if (errors.Count > 0)
                {
                    return Results.ValidationProblem(errors);
                }

                var resource = LearningResourceContractMapper.ToEntity(request, DateTimeOffset.UtcNow);
                dbContext.LearningResources.Add(resource);
                await dbContext.SaveChangesAsync(cancellationToken);

                return Results.Created($"/api/v1/learning-resources/{resource.Id}", LearningResourceContractMapper.ToDetailResponse(resource));
            })
            .RequireAuthorization("InternalUser")
            .WithBearerAuthOpenApi("Requires an internal-user bearer token.")
            .WithSummary("Create a learning resource with MVP validation rules.");

        endpoints.MapGet(
            "/api/v1/learning-resources",
            async (AppDbContext dbContext, CancellationToken cancellationToken) =>
            {
                var resources = await dbContext.LearningResources
                    .AsNoTracking()
                    .Include(resource => resource.Comments)
                    .OrderBy(resource => resource.Title)
                    .ToListAsync(cancellationToken);

                return Results.Ok(resources.Select(LearningResourceContractMapper.ToListItemResponse));
            })
            .WithSummary("List learning resources using the versioned API contract.");

        endpoints.MapGet(
            "/api/v1/learning-resources/{id:guid}",
            async (Guid id, AppDbContext dbContext, CancellationToken cancellationToken) =>
            {
                var resource = await dbContext.LearningResources
                    .AsNoTracking()
                    .Include(resource => resource.Comments)
                    .SingleOrDefaultAsync(resource => resource.Id == id, cancellationToken);

                return resource is null
                    ? Results.NotFound()
                    : Results.Ok(LearningResourceContractMapper.ToDetailResponse(resource));
            })
            .WithSummary("Get one learning resource with its comments using the versioned API contract.");

        endpoints.MapPut(
            "/api/v1/learning-resources/{id:guid}",
            async (Guid id, UpdateLearningResourceRequest request, AppDbContext dbContext, CancellationToken cancellationToken) =>
            {
                var errors = LearningResourceRequestValidator.Validate(request);
                if (errors.Count > 0)
                {
                    return Results.ValidationProblem(errors);
                }

                var resource = await dbContext.LearningResources
                    .Include(learningResource => learningResource.Comments)
                    .SingleOrDefaultAsync(learningResource => learningResource.Id == id, cancellationToken);

                if (resource is null)
                {
                    return Results.NotFound();
                }

                LearningResourceContractMapper.ApplyUpdate(resource, request);
                await dbContext.SaveChangesAsync(cancellationToken);

                return Results.Ok(LearningResourceContractMapper.ToDetailResponse(resource));
            })
            .RequireAuthorization("InternalUser")
            .WithBearerAuthOpenApi("Requires an internal-user bearer token.")
            .WithSummary("Update a learning resource with MVP validation rules.");

        endpoints.MapDelete(
            "/api/v1/learning-resources/{id:guid}",
            async (Guid id, AppDbContext dbContext, CancellationToken cancellationToken) =>
            {
                var resource = await dbContext.LearningResources
                    .SingleOrDefaultAsync(learningResource => learningResource.Id == id, cancellationToken);

                if (resource is null)
                {
                    return Results.NotFound();
                }

                dbContext.LearningResources.Remove(resource);
                await dbContext.SaveChangesAsync(cancellationToken);

                return Results.NoContent();
            })
            .RequireAuthorization("InternalUser")
            .WithBearerAuthOpenApi("Requires an internal-user bearer token.")
            .WithSummary("Delete a learning resource.");

        return endpoints;
    }
}
