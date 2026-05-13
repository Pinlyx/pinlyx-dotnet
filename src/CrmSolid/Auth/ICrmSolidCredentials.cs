using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace CrmSolid.Auth;

/// <summary>
/// Represents authentication material applied to outgoing CRM Solid API requests.
/// Implementations are responsible for setting the appropriate headers
/// (Authorization, X-API-Key + X-Signature, etc.).
/// </summary>
public interface ICrmSolidCredentials
{
    /// <summary>
    /// Apply credentials to the outgoing request. Called once per request, after the
    /// request body has been written but before the request is sent.
    /// </summary>
    Task ApplyAsync(HttpRequestMessage request, CancellationToken cancellationToken);
}
