namespace BlijvenLeren.App.Configuration;

public sealed class RuntimeOptions
{
    public const string SectionName = "Runtime";

    public DatabaseOptions Database { get; init; } = new();

    public IdentityProviderOptions IdentityProvider { get; init; } = new();
}

public sealed class DatabaseOptions
{
    public bool ApplyMigrationsOnStartup { get; init; }

    public string Host { get; init; } = "localhost";

    public int Port { get; init; } = 5432;
}

public sealed class IdentityProviderOptions
{
    public string Authority { get; init; } = "http://localhost:8081";
}
