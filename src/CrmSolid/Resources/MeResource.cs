using System.Threading;
using System.Threading.Tasks;
using CrmSolid.Http;
using CrmSolid.Models;

namespace CrmSolid.Resources;

/// <summary>
/// Operations on the authenticated workspace identity. Backed by <c>GET /v1/me</c>.
/// Useful as a connectivity smoke-test.
/// </summary>
public sealed class MeResource
{
    private readonly CrmSolidHttpClient _http;

    internal MeResource(CrmSolidHttpClient http)
    {
        _http = http;
    }

    /// <summary>Returns the workspace identity associated with the bearer key.</summary>
    public Task<User> GetAsync(CancellationToken cancellationToken = default)
        => _http.GetAsync<User>("v1/me", cancellationToken);
}
