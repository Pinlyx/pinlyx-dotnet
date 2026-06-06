using System;

namespace CrmSolid.Models;

/// <summary>Request body for <c>POST /v1/twitter/messages</c> — send a Twitter (X) DM.</summary>
public sealed class SendDmRequest
{
    /// <summary>X account id owned by the caller (from <c>Accounts.ListAsync</c>).</summary>
    public int XAccountId { get; set; }

    /// <summary>Recipient contact id. Must be a Twitter contact with a resolvable XUserId.</summary>
    public int ContactId { get; set; }

    /// <summary>DM body. Plain text, max 4000 chars.</summary>
    public string Text { get; set; } = string.Empty;
}

/// <summary>Result of a synchronous Twitter DM send.</summary>
public sealed record TwitterDmResult
{
    public string Status { get; init; } = string.Empty;
    public int? XDmId { get; init; }
    public string? MessageId { get; init; }
    public string? Text { get; init; }
    public DateTimeOffset? SentAt { get; init; }
}

/// <summary>A stored Twitter (X) direct message.</summary>
public sealed record TwitterMessage
{
    public int Id { get; init; }
    public int XAccountId { get; init; }
    public int? ContactId { get; init; }
    public string? ContactName { get; init; }
    public string? ContactUsername { get; init; }
    public string? Text { get; init; }
    public DateTimeOffset At { get; init; }
    public string? SenderId { get; init; }
    public string? RecipientId { get; init; }
}
