using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using CrmSolid.Http;
using CrmSolid.Models;

namespace CrmSolid.Resources;

/// <summary>
/// Read-only messaging analytics. Backed by <c>/v1/analytics/*</c>. Every method requires
/// <c>analytics:read</c> and is idempotent / side-effect free.
/// </summary>
public sealed class AnalyticsResource
{
    private readonly CrmSolidHttpClient _http;

    internal AnalyticsResource(CrmSolidHttpClient http) { _http = http; }

    /// <summary>High-level KPIs: total contacts, connected accounts, and today's message counts.</summary>
    public Task<AnalyticsSummary> GetSummaryAsync(CancellationToken cancellationToken = default)
        => _http.GetAsync<AnalyticsSummary>("v1/analytics/summary", cancellationToken);

    /// <summary>Sent/failed/queued counts over a 1, 7, or 30 day window, optionally scoped to one account.</summary>
    public Task<MessagingStats> GetMessagingStatsAsync(
        int windowDays = 7,
        int? accountId = null,
        CancellationToken cancellationToken = default)
    {
        var qs = new QueryStringBuilder();
        if (windowDays != 7) qs.Add("windowDays", windowDays.ToString(CultureInfo.InvariantCulture));
        if (accountId.HasValue) qs.Add("accountId", accountId.Value.ToString(CultureInfo.InvariantCulture));
        return _http.GetAsync<MessagingStats>("v1/analytics/messaging" + qs, cancellationToken);
    }

    /// <summary>Most-messaged contacts over the last 30 days, ranked by outbound message volume.</summary>
    public async Task<IReadOnlyList<TopContact>> ListTopContactsAsync(int limit = 10, CancellationToken cancellationToken = default)
    {
        var qs = new QueryStringBuilder();
        if (limit != 10) qs.Add("limit", limit.ToString(CultureInfo.InvariantCulture));
        var res = await _http.GetAsync<ItemsResponse<TopContact>>("v1/analytics/top-contacts" + qs, cancellationToken).ConfigureAwait(false);
        return res.Items;
    }
}
