using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CrmSolid.Auth;

namespace CrmSolid.Http;

/// <summary>
/// Delegating handler that applies <see cref="ICrmSolidCredentials"/> to every
/// outgoing request just before it is sent.
/// </summary>
public sealed class CrmSolidAuthHandler : DelegatingHandler
{
    private readonly ICrmSolidCredentials _credentials;

    public CrmSolidAuthHandler(ICrmSolidCredentials credentials)
    {
        _credentials = credentials;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        await _credentials.ApplyAsync(request, cancellationToken).ConfigureAwait(false);
        return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }
}
