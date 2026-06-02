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
