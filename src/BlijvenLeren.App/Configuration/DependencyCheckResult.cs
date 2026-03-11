namespace BlijvenLeren.App.Configuration;

internal sealed record DependencyCheckResult(
    string? Host,
    int? Port,
    string? Authority,
    bool Healthy,
    int? StatusCode,
    string? Error);
