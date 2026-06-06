using System;
using System.Collections.Generic;

namespace CrmSolid.Models;

/// <summary>A recently active conversation — a contact plus its last-message preview.</summary>
public sealed record RecentConversation
{
    public int ContactId { get; init; }
    public string Platform { get; init; } = string.Empty;
    public string? Name { get; init; }
    public string? Username { get; init; }
    public DateTimeOffset? LastMessageAt { get; init; }
    public bool HasUnread { get; init; }
    public string? Preview { get; init; }
    public string Stage { get; init; } = string.Empty;
}

/// <summary>A single message in a conversation thread. Direction is "incoming"/"outgoing".</summary>
public sealed record ConversationMessage
{
    public int Id { get; init; }
    public string Direction { get; init; } = string.Empty;
    public string? Text { get; init; }
    public DateTimeOffset At { get; init; }
    public int? AccountId { get; init; }
    public int? XAccountId { get; init; }
    public string? MediaType { get; init; }
    public string? MediaUrl { get; init; }
}

/// <summary>
/// A page of a contact's conversation thread (newest-first). Pass <see cref="NextBefore"/>
/// as the <c>before</c> argument to fetch the next (older) page; null when no more remain.
/// </summary>
public sealed record ConversationThread
{
    public IReadOnlyList<ConversationMessage> Items { get; init; } = Array.Empty<ConversationMessage>();
    public int? NextBefore { get; init; }
}
