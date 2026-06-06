using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using CrmSolid.Http;
using CrmSolid.Models;

namespace CrmSolid.Resources;

/// <summary>
/// Read-only message-job monitor. Backed by <c>/v1/jobs</c>. Requires <c>jobs:read</c>.
/// Create jobs via <c>TelegramMessages.SendAsync</c>, not here.
/// </summary>
public sealed class JobsResource
{
    private readonly CrmSolidHttpClient _http;

    internal JobsResource(CrmSolidHttpClient http) { _http = http; }

    /// <summary>Cursor-paginated jobs (newest first). Filter by status ("queued"/"sent"/"failed") and/or account.</summary>
    public Task<JobList> ListAsync(
        int? after = null,
        int limit = 25,
        string? status = null,
        int? accountId = null,
        CancellationToken cancellationToken = default)
    {
        var qs = new QueryStringBuilder();
        if (after.HasValue) qs.Add("after", after.Value.ToString(CultureInfo.InvariantCulture));
        if (limit != 25) qs.Add("limit", limit.ToString(CultureInfo.InvariantCulture));
        if (!string.IsNullOrEmpty(status)) qs.Add("status", status!);
        if (accountId.HasValue) qs.Add("accountId", accountId.Value.ToString(CultureInfo.InvariantCulture));
        return _http.GetAsync<JobList>("v1/jobs" + qs, cancellationToken);
    }

    /// <summary>Returns a single job with its status and last error.</summary>
    public Task<Job> GetAsync(int id, CancellationToken cancellationToken = default)
        => _http.GetAsync<Job>("v1/jobs/" + id.ToString(CultureInfo.InvariantCulture), cancellationToken);

    /// <summary>Streams every job across pages using cursor pagination.</summary>
    public async IAsyncEnumerable<Job> StreamAllAsync(
        int pageSize = 100,
        string? status = null,
        int? accountId = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        int? cursor = null;
        while (true)
        {
            var page = await ListAsync(cursor, pageSize, status, accountId, cancellationToken).ConfigureAwait(false);
            foreach (var job in page.Items) yield return job;
            if (!page.HasMore || page.NextCursor is null) yield break;
            cursor = page.NextCursor;
        }
    }
}
