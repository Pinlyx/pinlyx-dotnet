using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using CrmSolid.Http;
using CrmSolid.Models;

namespace CrmSolid.Resources;

/// <summary>
/// Email inbox threads. Backed by <c>/v1/email/threads</c>. Read with <c>email:read</c>;
/// set status / assignee with <c>email:write</c>. There is no send operation by design.
/// </summary>
public sealed class EmailResource
{
    private readonly CrmSolidHttpClient _http;

    internal EmailResource(CrmSolidHttpClient http) { _http = http; }

    internal sealed record StatusBody(string Status);
    internal sealed record AssignBody(int? UserId);

    /// <summary>Cursor-paginated inbox threads.</summary>
    public Task<EmailThreadList> ListThreadsAsync(
        int? after = null,
        int limit = 25,
        string? status = null,
        int? contactId = null,
        bool? unreadOnly = null,
        string? query = null,
        CancellationToken cancellationToken = default)
    {
        var qs = new QueryStringBuilder();
        if (after.HasValue) qs.Add("after", after.Value.ToString(CultureInfo.InvariantCulture));
        if (limit != 25) qs.Add("limit", limit.ToString(CultureInfo.InvariantCulture));
        if (!string.IsNullOrEmpty(status)) qs.Add("status", status!);
        if (contactId.HasValue) qs.Add("contactId", contactId.Value.ToString(CultureInfo.InvariantCulture));
        if (unreadOnly.HasValue) qs.Add("unreadOnly", unreadOnly.Value ? "true" : "false");
        if (!string.IsNullOrEmpty(query)) qs.Add("q", query!);
        return _http.GetAsync<EmailThreadList>("v1/email/threads" + qs, cancellationToken);
    }

    /// <summary>Returns a thread with its messages (plain text) and any AI summary.</summary>
    public Task<EmailThreadDetail> GetThreadAsync(int id, int limit = 20, CancellationToken cancellationToken = default)
    {
        var qs = new QueryStringBuilder();
        if (limit != 20) qs.Add("limit", limit.ToString(CultureInfo.InvariantCulture));
        return _http.GetAsync<EmailThreadDetail>("v1/email/threads/" + id.ToString(CultureInfo.InvariantCulture) + qs, cancellationToken);
    }

    /// <summary>Sets a thread's workflow status (open/pending/closed). Does not send mail.</summary>
    public Task<EmailThread> SetStatusAsync(int id, string status, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(status)) throw new ArgumentException("status is required", nameof(status));
        return _http.PostJsonAsync<StatusBody, EmailThread>(
            "v1/email/threads/" + id.ToString(CultureInfo.InvariantCulture) + "/status",
            new StatusBody(status),
            cancellationToken);
    }

    /// <summary>Assigns a thread to a team member, or unassigns when <paramref name="userId"/> is null.</summary>
    public Task<EmailThread> AssignAsync(int id, int? userId, CancellationToken cancellationToken = default)
        => _http.PostJsonAsync<AssignBody, EmailThread>(
            "v1/email/threads/" + id.ToString(CultureInfo.InvariantCulture) + "/assignee",
            new AssignBody(userId),
            cancellationToken);

    /// <summary>Streams every thread across all pages using cursor pagination.</summary>
    public async IAsyncEnumerable<EmailThread> StreamThreadsAsync(
        int pageSize = 50,
        string? status = null,
        int? contactId = null,
        bool? unreadOnly = null,
        string? query = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        int? cursor = null;
        while (true)
        {
            var page = await ListThreadsAsync(cursor, pageSize, status, contactId, unreadOnly, query, cancellationToken).ConfigureAwait(false);
            foreach (var thread in page.Items) yield return thread;
            if (!page.HasMore || page.NextCursor is null) yield break;
            cursor = page.NextCursor;
        }
    }
}
