using System;

namespace CrmSolid.Models;

/// <summary>
/// Request body for <c>POST /v1/telegram/messages</c>. Provide one of
/// <see cref="ContactId"/>, <see cref="Username"/>, or <see cref="TelegramUserId"/>
/// to identify the recipient.
/// </summary>
public sealed class SendMessageRequest
{
    /// <summary>Id of the Telegram account (owned by the caller) to send from.</summary>
    public int AccountId { get; set; }

    /// <summary>CRM contact id. The contact's stored username/telegramUserId is used as the target.</summary>
    public int? ContactId { get; set; }

    /// <summary>Telegram @username of the recipient (leading @ is stripped server-side).</summary>
    public string? Username { get; set; }

    /// <summary>Telegram numeric user id of the recipient.</summary>
    public long? TelegramUserId { get; set; }

    /// <summary>Message body. Maximum 4000 characters.</summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>Scheduled delivery time (UTC). Null = immediate dispatch.</summary>
    public DateTimeOffset? RunAt { get; set; }
}
