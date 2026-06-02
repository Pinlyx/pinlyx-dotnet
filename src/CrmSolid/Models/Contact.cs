using System;
using System.Collections.Generic;

namespace CrmSolid.Models;

/// <summary>A CRM contact record.</summary>
public sealed record Contact
{
    public int Id { get; init; }

    /// <summary>Social platform the contact belongs to.</summary>
    public Platform Platform { get; init; }

    public string? Name { get; init; }

    /// <summary>Handle without the leading @.</summary>
    public string? Username { get; init; }

    public string? Phone { get; init; }
    public string? Email { get; init; }
    public string? Company { get; init; }
    public string? Notes { get; init; }

    /// <summary>CRM pipeline stage (e.g. "Lead", "Customer").</summary>
    public string Stage { get; init; } = string.Empty;

    /// <summary>Lead score 0-100 (higher = hotter). Null when not scored.</summary>
    public int? LeadScore { get; init; }

    /// <summary>True when the score was last set by AI, false for a manual override.</summary>
    public bool LeadScoreIsAi { get; init; }

    /// <summary>Team member the contact is assigned to. Null when unassigned.</summary>
    public int? AssignedToUserId { get; init; }

    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? LastMessageAt { get; init; }
    public bool HasUnreadMessages { get; init; }

    /// <summary>Attached tags. Populated by <c>GetAsync(id)</c>; null in list responses.</summary>
    public IReadOnlyList<Tag>? Tags { get; init; }
}
