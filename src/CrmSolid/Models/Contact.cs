using System;

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
    public string? Notes { get; init; }

    /// <summary>CRM pipeline stage (e.g. "Lead", "Customer").</summary>
    public string Stage { get; init; } = string.Empty;

    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? LastMessageAt { get; init; }
    public bool HasUnreadMessages { get; init; }
}
