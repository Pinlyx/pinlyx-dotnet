using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using CrmSolid.Http;
using CrmSolid.Models;

namespace CrmSolid.Resources;

/// <summary>Outbound Telegram messaging. Backed by <c>/v1/telegram/messages</c>.</summary>
public sealed class TelegramMessagesResource
{
    private readonly CrmSolidHttpClient _http;

    internal TelegramMessagesResource(CrmSolidHttpClient http)
    {
        _http = http;
    }

    /// <summary>
    /// Enqueues a Telegram outbound message and returns immediately with the
    /// queued job. The recipient is resolved from <c>contactId</c>, <c>username</c>,
    /// or <c>telegramUserId</c> — at least one must be supplied.
    /// </summary>
    public Task<MessageJob> SendAsync(
        SendMessageRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request is null) throw new ArgumentNullException(nameof(request));
        return _http.PostJsonAsync<SendMessageRequest, MessageJob>(
            "v1/telegram/messages",
            request,
            cancellationToken);
    }

    /// <summary>Returns the current state of a previously enqueued message job.</summary>
    public Task<MessageJobDetail> GetAsync(int jobId, CancellationToken cancellationToken = default)
        => _http.GetAsync<MessageJobDetail>(
            "v1/telegram/messages/" + jobId.ToString(CultureInfo.InvariantCulture),
            cancellationToken);
}
