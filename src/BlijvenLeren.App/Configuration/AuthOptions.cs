namespace BlijvenLeren.App.Configuration;

public sealed class AuthOptions
{
    public const string SectionName = "Authentication";

    public string Authority { get; init; } = "http://localhost:8081/realms/blijvenleren";

    public string? MetadataAddress { get; init; }

    public string? BackchannelAuthority { get; init; }

    public string ClientId { get; init; } = "blijvenleren-app";

    public string InternalUserRole { get; init; } = "internal-user";

    public string ExternalContributorRole { get; init; } = "external-contributor";
}
