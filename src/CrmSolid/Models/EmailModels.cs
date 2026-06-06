using System;
using System.Collections.Generic;

namespace CrmSolid.Models;

/// <summary>An email inbox thread. Status is "open"/"pending"/"closed".</summary>
public record EmailThread
{
    public int Id { get; init; }
    public string Subject { get; init; } = string.Empty;
    public string? Preview { get; init; }
    public string Status { get; init; } = string.Empty;
    public int UnreadCount { get; init; }
    public bool IsStarred { get; init; }
    public int? ContactId { get; init; }
    public int? AssignedToUserId { get; init; }
    public int? AiLeadScore { get; init; }
    public DateTimeOffset LastMessageAt { get; init; }
}

/// <summary>A thread plus its messages and AI summary, returned by <c>GET /v1/email/threads/{id}</c>.</summary>
public sealed record EmailThreadDetail : EmailThread
{
    public string? AiSummary { get; init; }
    public IReadOnlyList<EmailMessage> Messages { get; init; } = Array.Empty<EmailMessage>();
}

/// <summary>A single message in a thread. Direction is "inbound"/"outbound". Body is plain text only.</summary>
public sealed record EmailMessage
{
    public int Id { get; init; }
    public string Direction { get; init; } = string.Empty;
    public string FromAddress { get; init; } = string.Empty;
    public string? FromName { get; init; }
    public string Subject { get; init; } = string.Empty;
    public string? Body { get; init; }
    public bool IsRead { get; init; }
    public DateTimeOffset ReceivedAt { get; init; }
}

/// <summary>Cursor-paginated list of email threads.</summary>
public sealed record EmailThreadList
{
    public IReadOnlyList<EmailThread> Items { get; init; } = Array.Empty<EmailThread>();
    public int? NextCursor { get; init; }
    public bool HasMore { get; init; }
}

/// <summary>
/// Request body for <c>POST /v1/email/threads/{id}/reply</c> (requires <c>email:send</c>).
/// Sends real mail through the user's connected mailbox.
/// </summary>
public sealed class EmailReplyRequest
{
    /// <summary>Recipients. If omitted, defaults to the thread's last inbound sender.</summary>
    public List<string>? To { get; set; }
    public List<string>? Cc { get; set; }
    public List<string>? Bcc { get; set; }
    public string? BodyText { get; set; }
    public string? BodyHtml { get; set; }
    /// <summary>Override the auto "Re: &lt;subject&gt;" subject line.</summary>
    public string? SubjectOverride { get; set; }
}

/// <summary>Result of sending an email reply.</summary>
public sealed record EmailSendResult
{
    public int MessageId { get; init; }
    public int ThreadId { get; init; }
    public string MessageIdHeader { get; init; } = string.Empty;
}
