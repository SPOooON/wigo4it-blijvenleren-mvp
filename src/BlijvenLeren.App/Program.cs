using System.Security.Claims;
using BlijvenLeren.App.Configuration;
using BlijvenLeren.App.Data;
using BlijvenLeren.App.Features.Auth;
using BlijvenLeren.App.Features.Comments;
using BlijvenLeren.App.Features.LearningResources;
using BlijvenLeren.App.Features.Runtime;
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
app.MapAuthEndpoints();
app.MapRuntimeEndpoints();
app.MapLearningResourceEndpoints();
app.MapCommentEndpoints();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();

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
