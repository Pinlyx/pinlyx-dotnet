using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using CrmSolid.Http;
using CrmSolid.Models;

namespace CrmSolid.Resources;

/// <summary>
/// Read-only conversations. Backed by <c>/v1/conversations</c>. Every method requires
/// <c>contacts:read</c>.
/// </summary>
public sealed class ConversationsResource
{
    private readonly CrmSolidHttpClient _http;

    internal ConversationsResource(CrmSolidHttpClient http) { _http = http; }

    /// <summary>Recently active conversations with a last-message preview, ordered by latest activity.</summary>
    public async Task<IReadOnlyList<RecentConversation>> ListRecentAsync(
        int limit = 20,
        bool? unreadOnly = null,
        CancellationToken cancellationToken = default)
    {
        var qs = new QueryStringBuilder();
        if (limit != 20) qs.Add("limit", limit.ToString(CultureInfo.InvariantCulture));
        if (unreadOnly == true) qs.Add("unreadOnly", "true");
        var res = await _http.GetAsync<ItemsResponse<RecentConversation>>("v1/conversations" + qs, cancellationToken).ConfigureAwait(false);
        return res.Items;
    }

    /// <summary>
    /// One contact's message thread, newest-first. Pass <see cref="ConversationThread.NextBefore"/>
    /// as <paramref name="before"/> to fetch the next (older) page.
    /// </summary>
    public Task<ConversationThread> GetThreadAsync(
        int contactId,
        int? before = null,
        int limit = 25,
        CancellationToken cancellationToken = default)
    {
        var qs = new QueryStringBuilder();
        if (before.HasValue) qs.Add("before", before.Value.ToString(CultureInfo.InvariantCulture));
        if (limit != 25) qs.Add("limit", limit.ToString(CultureInfo.InvariantCulture));
        return _http.GetAsync<ConversationThread>(
            "v1/conversations/" + contactId.ToString(CultureInfo.InvariantCulture) + qs, cancellationToken);
    }

    /// <summary>Streams a contact's entire message thread (newest-first) across pages.</summary>
    public async IAsyncEnumerable<ConversationMessage> StreamThreadAsync(
        int contactId,
        int pageSize = 50,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        int? before = null;
        while (true)
        {
            var page = await GetThreadAsync(contactId, before, pageSize, cancellationToken).ConfigureAwait(false);
            foreach (var msg in page.Items) yield return msg;
            if (page.NextBefore is null) yield break;
            before = page.NextBefore;
        }
    }
}
