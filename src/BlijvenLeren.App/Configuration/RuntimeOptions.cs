namespace BlijvenLeren.App.Configuration;

public sealed class RuntimeOptions
{
    public const string SectionName = "Runtime";

    public AuthenticationRuntimeOptions Authentication { get; init; } = new();

    public DatabaseOptions Database { get; init; } = new();

    public IdentityProviderOptions IdentityProvider { get; init; } = new();
}

public sealed class AuthenticationRuntimeOptions
{
    public string CookieName { get; init; } = "BlijvenLeren.Auth";
}

public sealed class DatabaseOptions
{
    public bool ApplyMigrationsOnStartup { get; init; }

    public bool SeedDemoDataOnStartup { get; init; }

    public string Host { get; init; } = "localhost";

    public int Port { get; init; } = 5432;
}

public sealed class IdentityProviderOptions
{
    public string Authority { get; init; } = "http://localhost:8081/realms/blijvenleren";
}
