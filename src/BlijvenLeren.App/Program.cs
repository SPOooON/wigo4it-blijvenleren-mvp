using System.Security.Claims;
using System.Net.Sockets;
using BlijvenLeren.App.Configuration;
using BlijvenLeren.App.Contracts.V1;
using BlijvenLeren.App.Data;
using BlijvenLeren.App.Data.Entities;
using BlijvenLeren.App.Features.Comments;
using BlijvenLeren.App.Features.LearningResources;
using BlijvenLeren.App.OpenApi;
using BlijvenLeren.App.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.Configure<RuntimeOptions>(
    builder.Configuration.GetSection(RuntimeOptions.SectionName));
builder.Services.Configure<AuthOptions>(
    builder.Configuration.GetSection(AuthOptions.SectionName));
var authOptions = builder.Configuration.GetSection(AuthOptions.SectionName).Get<AuthOptions>()
    ?? throw new InvalidOperationException("Authentication settings are not configured.");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("BlijvenLeren")));
builder.Services.AddScoped<ClaimsPrincipalFactory>();
builder.Services.AddScoped<DemoDataSeeder>();
builder.Services.AddOpenApi(OpenApiDocumentConfiguration.Configure);
builder.Services.AddRazorPages();
builder.Services.AddHttpClient();
builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = "AppOrBearer";
        options.DefaultChallengeScheme = "AppOrBearer";
        options.DefaultForbidScheme = "AppOrBearer";
        options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultSignOutScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    })
    .AddPolicyScheme("AppOrBearer", "App or bearer token", options =>
    {
        options.ForwardDefaultSelector = context =>
        {
            var authorizationHeader = context.Request.Headers.Authorization.ToString();
            if (authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return JwtBearerDefaults.AuthenticationScheme;
            }

            return context.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase)
                ? JwtBearerDefaults.AuthenticationScheme
                : CookieAuthenticationDefaults.AuthenticationScheme;
        };
    })
    .AddCookie(options =>
    {
        options.Cookie.Name = builder.Configuration["Runtime:Authentication:CookieName"] ?? "BlijvenLeren.Auth";
        options.AccessDeniedPath = "/";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.ForwardChallenge = OpenIdConnectDefaults.AuthenticationScheme;
    })
    .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
    {
        options.Authority = authOptions.Authority;
        options.ClientId = authOptions.ClientId;
        options.RequireHttpsMetadata = false;
        options.ResponseType = "code";
        options.UsePkce = true;
        options.SaveTokens = true;
        options.GetClaimsFromUserInfoEndpoint = false;
        if (!string.IsNullOrWhiteSpace(authOptions.MetadataAddress))
        {
            options.MetadataAddress = authOptions.MetadataAddress;
        }

        options.TokenValidationParameters = new TokenValidationParameters
        {
            NameClaimType = "preferred_username",
            RoleClaimType = ClaimTypes.Role,
            ValidIssuer = authOptions.Authority
        };

        options.Scope.Add("openid");
        options.Scope.Add("profile");
        options.Scope.Add("email");
        options.BackchannelHttpHandler = new AuthorityRewriteHandler(authOptions.Authority, authOptions.BackchannelAuthority);
        options.Events = new OpenIdConnectEvents
        {
            OnTokenValidated = context =>
            {
                if (context.Principal?.Identity is ClaimsIdentity identity)
                {
                    var claimsFactory = context.HttpContext.RequestServices.GetRequiredService<ClaimsPrincipalFactory>();
                    var accessToken = context.TokenEndpointResponse?.AccessToken;
                    if (!string.IsNullOrWhiteSpace(accessToken))
                    {
                        claimsFactory.AddRoleClaimsFromAccessToken(accessToken, identity);
                    }
                    else
                    {
                        claimsFactory.AddRoleClaimsFromRealmAccess(identity);
                    }
                }

                return Task.CompletedTask;
            }
        };
    })
    .AddJwtBearer(options =>
    {
        options.Authority = authOptions.Authority;
        if (!string.IsNullOrWhiteSpace(authOptions.MetadataAddress))
        {
            options.MetadataAddress = authOptions.MetadataAddress;
        }
        options.RequireHttpsMetadata = false;
        options.BackchannelHttpHandler = new AuthorityRewriteHandler(authOptions.Authority, authOptions.BackchannelAuthority);
        options.TokenValidationParameters = new TokenValidationParameters
        {
            NameClaimType = "preferred_username",
            RoleClaimType = ClaimTypes.Role,
            ValidateAudience = false,
            ValidIssuer = authOptions.Authority
        };
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                if (context.Principal?.Identity is ClaimsIdentity identity)
                {
                    var claimsFactory = context.HttpContext.RequestServices.GetRequiredService<ClaimsPrincipalFactory>();
                    claimsFactory.AddRoleClaimsFromRealmAccess(identity);
                }

                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("InternalUser", policy => policy.RequireRole(builder.Configuration[$"{AuthOptions.SectionName}:InternalUserRole"] ?? "internal-user"));
    options.AddPolicy("ExternalContributor", policy => policy.RequireRole(builder.Configuration[$"{AuthOptions.SectionName}:ExternalContributorRole"] ?? "external-contributor"));
});

var app = builder.Build();

await ApplyDatabaseMigrationsAsync(app);
await SeedDemoDataAsync(app);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapOpenApi("/openapi/{documentName}.json");
app.MapScalarApiReference(
    "/docs",
    options =>
    {
        options.Title = "BlijvenLeren API Docs";
        options.OpenApiRoutePattern = "/openapi/{documentName}.json";
    });

app.MapGet(
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

app.MapPost(
    "/account/logout",
    (HttpContext httpContext) =>
    {
        var authProperties = new AuthenticationProperties
        {
            RedirectUri = "/"
        };
        return Results.SignOut(
            authProperties,
            [CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme]);
    });

app.MapGet("/api/health", () => Results.Ok(new
{
    status = "ok",
    application = "BlijvenLeren",
    mode = "bootstrap"
}))
    .WithSummary("Return a simple application health response.");

app.MapGet(
    "/api/health/dependencies",
    async (
        IOptions<RuntimeOptions> runtimeOptions,
        IHttpClientFactory httpClientFactory,
        CancellationToken cancellationToken) =>
    {
        var runtime = runtimeOptions.Value;

        var database = await CheckTcpDependencyAsync(
            runtime.Database.Host,
            runtime.Database.Port,
            cancellationToken);

        var identityProvider = await CheckHttpDependencyAsync(
            runtime.IdentityProvider.Authority,
            httpClientFactory,
            cancellationToken);

        return Results.Ok(new
        {
            status = database.Healthy && identityProvider.Healthy ? "ok" : "degraded",
            dependencies = new
            {
                database,
                identityProvider
            }
        });
    })
    .WithSummary("Check whether the app can reach the database and identity provider.");

app.MapPost(
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

app.MapGet(
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

app.MapGet(
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

app.MapPost(
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

app.MapGet(
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

app.MapPost(
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

app.MapPut(
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

app.MapDelete(
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

app.MapPost(
    "/api/health/persistence-smoke",
    async (AppDbContext dbContext, CancellationToken cancellationToken) =>
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        var resource = new LearningResource
        {
            Id = Guid.NewGuid(),
            Title = "Persistence smoke resource",
            Description = "Temporary record written to validate the initial schema.",
            Url = "https://example.invalid/resources/persistence-smoke",
            CreatedUtc = DateTimeOffset.UtcNow
        };

        var comment = new Comment
        {
            Id = Guid.NewGuid(),
            LearningResourceId = resource.Id,
            AuthorDisplayName = "Smoke Tester",
            AuthorType = CommentAuthorType.External,
            Body = "Pending moderation comment for schema validation.",
            Status = CommentStatus.Pending,
            CreatedUtc = DateTimeOffset.UtcNow
        };

        dbContext.LearningResources.Add(resource);
        dbContext.Comments.Add(comment);
        await dbContext.SaveChangesAsync(cancellationToken);

        var persisted = await dbContext.LearningResources
            .Include(learningResource => learningResource.Comments)
            .SingleAsync(learningResource => learningResource.Id == resource.Id, cancellationToken);

        await transaction.RollbackAsync(cancellationToken);

        return Results.Ok(new
        {
            status = "ok",
            resourceId = persisted.Id,
            commentStatuses = persisted.Comments.Select(savedComment => savedComment.Status.ToString())
        });
    })
    .WithSummary("Write and read temporary persistence data inside a rolled-back transaction.");

app.MapPost(
    "/api/demo/seed-data",
    async (bool? reset, DemoDataSeeder seeder, CancellationToken cancellationToken) =>
    {
        var result = await seeder.SeedAsync(reset ?? false, cancellationToken);
        return Results.Ok(result);
    })
    .RequireAuthorization("InternalUser")
    .WithBearerAuthOpenApi("Requires an internal-user bearer token.")
    .WithSummary("Seed or reseed the demo dataset. Requires the internal-user role.");

app.MapGet(
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

app.MapGet("/api/auth/internal", () => Results.Ok(new { status = "ok", role = "internal-user" }))
    .RequireAuthorization("InternalUser")
    .WithBearerAuthOpenApi("Requires an internal-user bearer token.")
    .WithSummary("Verify access for the internal-user role.");

app.MapGet("/api/auth/external", () => Results.Ok(new { status = "ok", role = "external-contributor" }))
    .RequireAuthorization("ExternalContributor")
    .WithBearerAuthOpenApi("Requires an external-contributor bearer token.")
    .WithSummary("Verify access for the external-contributor role.");

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();

static async Task<DependencyCheckResult> CheckTcpDependencyAsync(
    string host,
    int port,
    CancellationToken cancellationToken)
{
    using var client = new TcpClient();
    using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
    timeout.CancelAfter(TimeSpan.FromSeconds(3));

    try
    {
        await client.ConnectAsync(host, port, timeout.Token);
        return new DependencyCheckResult(host, port, null, true, null, null);
    }
    catch (Exception ex)
    {
        return new DependencyCheckResult(host, port, null, false, null, ex.Message);
    }
}

static async Task<DependencyCheckResult> CheckHttpDependencyAsync(
    string authority,
    IHttpClientFactory httpClientFactory,
    CancellationToken cancellationToken)
{
    var client = httpClientFactory.CreateClient();
    client.Timeout = TimeSpan.FromSeconds(3);

    try
    {
        using var response = await client.GetAsync(authority, cancellationToken);
        return new DependencyCheckResult(null, null, authority, response.IsSuccessStatusCode, (int)response.StatusCode, null);
    }
    catch (Exception ex)
    {
        return new DependencyCheckResult(null, null, authority, false, null, ex.Message);
    }
}

static async Task ApplyDatabaseMigrationsAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var runtimeOptions = scope.ServiceProvider.GetRequiredService<IOptions<RuntimeOptions>>().Value;
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DatabaseStartup");

    if (!runtimeOptions.Database.ApplyMigrationsOnStartup)
    {
        logger.LogInformation("Database migrations on startup are disabled.");
        return;
    }

    for (var attempt = 1; attempt <= 10; attempt++)
    {
        try
        {
            await dbContext.Database.MigrateAsync();
            logger.LogInformation("Database migrations applied successfully.");
            return;
        }
        catch (Exception ex) when (attempt < 10)
        {
            logger.LogWarning(ex, "Database migration attempt {Attempt} failed. Retrying.", attempt);
            await Task.Delay(TimeSpan.FromSeconds(3));
        }
    }

    await dbContext.Database.MigrateAsync();
}

static async Task SeedDemoDataAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var runtimeOptions = scope.ServiceProvider.GetRequiredService<IOptions<RuntimeOptions>>().Value;
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DemoDataStartup");

    if (!runtimeOptions.Database.SeedDemoDataOnStartup)
    {
        logger.LogInformation("Demo data seeding on startup is disabled.");
        return;
    }

    var seeder = scope.ServiceProvider.GetRequiredService<DemoDataSeeder>();
    var result = await seeder.SeedAsync(resetExistingData: false, CancellationToken.None);
    logger.LogInformation(
        "Demo data seeding completed with status {Status}. Resources: {ResourceCount}, Comments: {CommentCount}.",
        result.Status,
        result.ResourceCount,
        result.CommentCount);
}
