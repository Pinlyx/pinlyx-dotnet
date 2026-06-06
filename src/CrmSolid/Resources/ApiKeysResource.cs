using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CrmSolid.Http;
using CrmSolid.Models;

namespace CrmSolid.Resources;

/// <summary>
/// Self-service API keys. Backed by <c>/v1/api-keys</c>. Requires <c>keys:manage</c> — a
/// powerful scope that is off by default. Minted keys are ATTENUATED: a new key can only
/// carry scopes the calling key already holds.
/// </summary>
public sealed class ApiKeysResource
{
    private readonly CrmSolidHttpClient _http;

    internal ApiKeysResource(CrmSolidHttpClient http) { _http = http; }

    /// <summary>Lists the caller's bearer keys (secrets / tokens are never returned).</summary>
    public async Task<IReadOnlyList<ApiKeySummary>> ListAsync(CancellationToken cancellationToken = default)
    {
        var res = await _http.GetAsync<ItemsResponse<ApiKeySummary>>("v1/api-keys", cancellationToken).ConfigureAwait(false);
        return res.Items;
    }

    /// <summary>Mints a new bearer key. The full token is returned ONCE — store it immediately.</summary>
    public Task<CreatedApiKey> CreateAsync(CreateApiKeyRequest request, CancellationToken cancellationToken = default)
    {
        if (request is null) throw new ArgumentNullException(nameof(request));
        return _http.PostJsonAsync<CreateApiKeyRequest, CreatedApiKey>("v1/api-keys", request, cancellationToken);
    }

    /// <summary>Revokes one of the caller's keys (idempotent).</summary>
    public async Task RevokeAsync(int id, CancellationToken cancellationToken = default)
    {
        await _http.DeleteAsync<JsonElement>("v1/api-keys/" + id.ToString(CultureInfo.InvariantCulture), cancellationToken).ConfigureAwait(false);
    }
}
