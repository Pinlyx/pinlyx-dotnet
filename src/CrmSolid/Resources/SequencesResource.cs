using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using CrmSolid.Http;
using CrmSolid.Models;

namespace CrmSolid.Resources;

/// <summary>
/// Outbound message sequences (campaigns). Backed by <c>/v1/sequences</c>. SAFE-WRITE boundary:
/// the API can list/inspect and pause/resume an existing sequence — creating, editing targets,
/// or deleting a sequence is a panel action.
/// </summary>
public sealed class SequencesResource
{
    private static readonly object EmptyBody = new();

    private readonly CrmSolidHttpClient _http;

    internal SequencesResource(CrmSolidHttpClient http) { _http = http; }

    /// <summary>Lists sequences, optionally filtered by status. Requires <c>sequences:read</c>.</summary>
    public async Task<IReadOnlyList<Sequence>> ListAsync(
        string? status = null,
        int limit = 25,
        CancellationToken cancellationToken = default)
    {
        var qs = new QueryStringBuilder();
        if (!string.IsNullOrEmpty(status)) qs.Add("status", status!);
        if (limit != 25) qs.Add("limit", limit.ToString(CultureInfo.InvariantCulture));
        var res = await _http.GetAsync<ItemsResponse<Sequence>>("v1/sequences" + qs, cancellationToken).ConfigureAwait(false);
        return res.Items;
    }

    /// <summary>Returns a sequence with its message steps and job counts. Requires <c>sequences:read</c>.</summary>
    public Task<SequenceDetail> GetAsync(int id, CancellationToken cancellationToken = default)
        => _http.GetAsync<SequenceDetail>("v1/sequences/" + id.ToString(CultureInfo.InvariantCulture), cancellationToken);

    /// <summary>Pauses an active sequence. Idempotent. Requires <c>sequences:write</c>.</summary>
    public Task<SequenceStatusResult> PauseAsync(int id, CancellationToken cancellationToken = default)
        => _http.PostJsonAsync<object, SequenceStatusResult>(
            "v1/sequences/" + id.ToString(CultureInfo.InvariantCulture) + "/pause", EmptyBody, cancellationToken);

    /// <summary>Resumes a paused sequence. Idempotent. Requires <c>sequences:write</c>.</summary>
    public Task<SequenceStatusResult> ResumeAsync(int id, CancellationToken cancellationToken = default)
        => _http.PostJsonAsync<object, SequenceStatusResult>(
            "v1/sequences/" + id.ToString(CultureInfo.InvariantCulture) + "/resume", EmptyBody, cancellationToken);
}
