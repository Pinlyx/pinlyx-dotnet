using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using CrmSolid.Http;
using CrmSolid.Models;

namespace CrmSolid.Resources;

/// <summary>
/// Read-only pipeline boards. Backed by <c>/v1/pipelines</c>. Requires <c>pipelines:read</c>.
/// Move a contact between stages with <c>Contacts.SetStageAsync</c> (<c>contacts:write</c>).
/// </summary>
public sealed class PipelinesResource
{
    private readonly CrmSolidHttpClient _http;

    internal PipelinesResource(CrmSolidHttpClient http) { _http = http; }

    /// <summary>Lists the caller's boards with their stage columns and per-column contact counts.</summary>
    public async Task<IReadOnlyList<Pipeline>> ListAsync(CancellationToken cancellationToken = default)
    {
        var res = await _http.GetAsync<ItemsResponse<Pipeline>>("v1/pipelines", cancellationToken).ConfigureAwait(false);
        return res.Items;
    }

    /// <summary>Returns a single board with its stages.</summary>
    public Task<Pipeline> GetAsync(int id, CancellationToken cancellationToken = default)
        => _http.GetAsync<Pipeline>("v1/pipelines/" + id.ToString(CultureInfo.InvariantCulture), cancellationToken);
}
