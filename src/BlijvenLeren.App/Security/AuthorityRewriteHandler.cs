namespace BlijvenLeren.App.Security;

internal sealed class AuthorityRewriteHandler(string? publicAuthority, string? backchannelAuthority) : HttpClientHandler
{
    private readonly Uri? _publicAuthority = string.IsNullOrWhiteSpace(publicAuthority) ? null : new Uri(publicAuthority.TrimEnd('/'));
    private readonly Uri? _backchannelAuthority = string.IsNullOrWhiteSpace(backchannelAuthority) ? null : new Uri(backchannelAuthority.TrimEnd('/'));

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (_publicAuthority is not null
            && _backchannelAuthority is not null
            && request.RequestUri is not null
            && string.Equals(request.RequestUri.Host, _publicAuthority.Host, StringComparison.OrdinalIgnoreCase)
            && request.RequestUri.Port == _publicAuthority.Port)
        {
            var builder = new UriBuilder(request.RequestUri)
            {
                Scheme = _backchannelAuthority.Scheme,
                Host = _backchannelAuthority.Host,
                Port = _backchannelAuthority.Port
            };

            request.RequestUri = builder.Uri;
        }

        return base.SendAsync(request, cancellationToken);
    }
}
