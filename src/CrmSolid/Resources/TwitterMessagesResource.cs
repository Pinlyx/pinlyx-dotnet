using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using CrmSolid.Http;
using CrmSolid.Models;

namespace CrmSolid.Resources;

/// <summary>
/// Twitter (X) direct messaging. Backed by <c>/v1/twitter/messages</c>. Unlike Telegram,
/// X DMs are sent synchronously — <see cref="SendAsync"/> returns the delivered message.
/// </summary>
public sealed class TwitterMessagesResource
{
    private readonly CrmSolidHttpClient _http;

    internal TwitterMessagesResource(CrmSolidHttpClient http) { _http = http; }

    /// <summary>Sends a Twitter (X) DM synchronously. Requires <c>twitter:send</c>.</summary>
    public Task<TwitterDmResult> SendAsync(SendDmRequest request, CancellationToken cancellationToken = default)
    {
        if (request is null) throw new ArgumentNullException(nameof(request));
        return _http.PostJsonAsync<SendDmRequest, TwitterDmResult>("v1/twitter/messages", request, cancellationToken);
    }

    /// <summary>Searches stored Twitter DMs by free text (case-insensitive), newest-first. Requires <c>contacts:read</c>.</summary>
    public async Task<IReadOnlyList<TwitterMessage>> SearchAsync(
        string query,
        int? contactId = null,
        int limit = 25,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(query)) throw new ArgumentException("query is required", nameof(query));
        var qs = new QueryStringBuilder();
        qs.Add("q", query);
        if (contactId.HasValue) qs.Add("contactId", contactId.Value.ToString(CultureInfo.InvariantCulture));
        if (limit != 25) qs.Add("limit", limit.ToString(CultureInfo.InvariantCulture));
        var res = await _http.GetAsync<ItemsResponse<TwitterMessage>>("v1/twitter/messages" + qs, cancellationToken).ConfigureAwait(false);
        return res.Items;
    }
}
