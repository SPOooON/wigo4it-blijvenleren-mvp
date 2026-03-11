using System.Security.Claims;
using BlijvenLeren.App.Contracts.V1;
using BlijvenLeren.App.Data;
using BlijvenLeren.App.Data.Entities;
using BlijvenLeren.App.Features.LearningResources;
using BlijvenLeren.App.OpenApi;
using Microsoft.EntityFrameworkCore;

namespace BlijvenLeren.App.Features.Comments;

public static class CommentEndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapCommentEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(
            "/api/v1/learning-resources/{id:guid}/comments",
            async (Guid id, CreateCommentRequest request, ClaimsPrincipal user, AppDbContext dbContext, CancellationToken cancellationToken) =>
            {
                var errors = CommentRequestValidator.Validate(request);
                if (errors.Count > 0)
                {
                    return Results.ValidationProblem(errors);
                }

                var resourceExists = await dbContext.LearningResources
                    .AnyAsync(learningResource => learningResource.Id == id, cancellationToken);

                if (!resourceExists)
                {
                    return Results.NotFound();
                }

                var comment = CommentSubmissionFactory.Create(id, user, request, DateTimeOffset.UtcNow);
                dbContext.Comments.Add(comment);
                await dbContext.SaveChangesAsync(cancellationToken);

                return Results.Created(
                    $"/api/v1/learning-resources/{id}",
                    LearningResourceContractMapper.ToCommentResponse(comment));
            })
            .RequireAuthorization()
            .WithBearerAuthOpenApi("Requires a bearer token from the local Keycloak realm.")
            .WithSummary("Add a comment to a learning resource. Internal comments are auto-approved; external comments stay pending.");

        endpoints.MapGet(
            "/api/v1/comments/pending",
            async (AppDbContext dbContext, CancellationToken cancellationToken) =>
            {
                var comments = await dbContext.Comments
                    .AsNoTracking()
                    .Include(comment => comment.LearningResource)
                    .Where(comment => comment.AuthorType == CommentAuthorType.External && comment.Status == CommentStatus.Pending)
                    .OrderBy(comment => comment.CreatedUtc)
                    .ToListAsync(cancellationToken);

                return Results.Ok(comments.Select(LearningResourceContractMapper.ToPendingCommentResponse));
            })
            .RequireAuthorization("InternalUser")
            .WithBearerAuthOpenApi("Requires an internal-user bearer token.")
            .WithSummary("List pending external comments for moderation.");

        endpoints.MapPost(
            "/api/v1/comments/{id:guid}/moderation",
            async (Guid id, ModerateCommentRequest request, AppDbContext dbContext, CancellationToken cancellationToken) =>
            {
                var errors = CommentModerationValidator.ValidateRequest(request);
                if (errors.Count > 0)
                {
                    return Results.ValidationProblem(errors);
                }

                var comment = await dbContext.Comments
                    .Include(savedComment => savedComment.LearningResource)
                    .SingleOrDefaultAsync(savedComment => savedComment.Id == id, cancellationToken);

                if (comment is null)
                {
                    return Results.NotFound();
                }

                var transitionError = CommentModerationValidator.ValidateTransition(comment);
                if (transitionError is not null)
                {
                    return Results.Conflict(new { error = transitionError });
                }

                CommentModerationValidator.TryParseAction(request.Action!, out var targetStatus);
                comment.Status = targetStatus;
                comment.ModeratedUtc = DateTimeOffset.UtcNow;
                await dbContext.SaveChangesAsync(cancellationToken);

                return Results.Ok(LearningResourceContractMapper.ToCommentResponse(comment));
            })
            .RequireAuthorization("InternalUser")
            .WithBearerAuthOpenApi("Requires an internal-user bearer token.")
            .WithSummary("Approve or reject a pending external comment.");

        return endpoints;
    }
}
