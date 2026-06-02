using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using CrmSolid.Http;
using CrmSolid.Models;

namespace CrmSolid.Resources;

/// <summary>Sales pipeline deals. Backed by <c>/v1/deals</c>.</summary>
public sealed class DealsResource
{
    private readonly CrmSolidHttpClient _http;

    internal DealsResource(CrmSolidHttpClient http) { _http = http; }

    internal sealed record StageBody(string Stage);

    /// <summary>Cursor-paginated deals. Pass <see cref="DealList.NextCursor"/> as <paramref name="after"/> to advance.</summary>
    public Task<DealList> ListAsync(
        int? after = null,
        int limit = 25,
        string? stage = null,
        int? contactId = null,
        string? query = null,
        CancellationToken cancellationToken = default)
    {
        var qs = new QueryStringBuilder();
        if (after.HasValue) qs.Add("after", after.Value.ToString(CultureInfo.InvariantCulture));
        if (limit != 25) qs.Add("limit", limit.ToString(CultureInfo.InvariantCulture));
        if (!string.IsNullOrEmpty(stage)) qs.Add("stage", stage!);
        if (contactId.HasValue) qs.Add("contactId", contactId.Value.ToString(CultureInfo.InvariantCulture));
        if (!string.IsNullOrEmpty(query)) qs.Add("q", query!);
        return _http.GetAsync<DealList>("v1/deals" + qs, cancellationToken);
    }

    /// <summary>Returns a single deal with its linked tasks.</summary>
    public Task<Deal> GetAsync(int id, CancellationToken cancellationToken = default)
        => _http.GetAsync<Deal>("v1/deals/" + id.ToString(CultureInfo.InvariantCulture), cancellationToken);

    /// <summary>Creates a deal in an active stage. Creating one already won/lost is rejected server-side.</summary>
    public Task<Deal> CreateAsync(CreateDealRequest request, CancellationToken cancellationToken = default)
    {
        if (request is null) throw new ArgumentNullException(nameof(request));
        return _http.PostJsonAsync<CreateDealRequest, Deal>("v1/deals", request, cancellationToken);
    }

    /// <summary>
    /// Moves a deal between stages. <c>won</c> is rejected by the API (it books an income
    /// ledger entry — do that from the panel); lead/qualified/proposal/negotiation/lost are allowed.
    /// </summary>
    public Task<Deal> ChangeStageAsync(int id, string stage, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(stage)) throw new ArgumentException("stage is required", nameof(stage));
        return _http.PostJsonAsync<StageBody, Deal>(
            "v1/deals/" + id.ToString(CultureInfo.InvariantCulture) + "/stage",
            new StageBody(stage),
            cancellationToken);
    }

    /// <summary>Streams every deal across all pages using cursor pagination.</summary>
    public async IAsyncEnumerable<Deal> StreamAllAsync(
        int pageSize = 100,
        string? stage = null,
        int? contactId = null,
        string? query = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        int? cursor = null;
        while (true)
        {
            var page = await ListAsync(cursor, pageSize, stage, contactId, query, cancellationToken).ConfigureAwait(false);
            foreach (var deal in page.Items) yield return deal;
            if (!page.HasMore || page.NextCursor is null) yield break;
            cursor = page.NextCursor;
        }
    }
}
