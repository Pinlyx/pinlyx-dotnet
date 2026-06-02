using System;
using System.Collections.Generic;

namespace CrmSolid.Models;

/// <summary>
/// A sales pipeline deal. Stage is a string ("Lead", "Qualified", "Proposal",
/// "Negotiation", "Won", "Lost") — the SDK keeps it as text so it never breaks if
/// the server adds a stage.
/// </summary>
public sealed record Deal
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public int? ContactId { get; init; }
    public string? ContactName { get; init; }
    public decimal Value { get; init; }
    public string Currency { get; init; } = "USD";
    public string Stage { get; init; } = string.Empty;
    /// <summary>Win probability, 0-100.</summary>
    public int Probability { get; init; }
    public DateTimeOffset? ExpectedCloseAt { get; init; }
    public DateTimeOffset? ClosedAt { get; init; }
    public string? Notes { get; init; }
    public int OpenTaskCount { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
    /// <summary>Linked tasks. Populated by <c>GetAsync(id)</c>; null in list responses.</summary>
    public IReadOnlyList<DealTask>? Tasks { get; init; }
}

/// <summary>A task summary nested inside a <see cref="Deal"/> detail response.</summary>
public sealed record DealTask
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string Priority { get; init; } = string.Empty;
    public DateTimeOffset? DueAt { get; init; }
    public bool IsOverdue { get; init; }
}

/// <summary>Cursor-paginated list of deals.</summary>
public sealed record DealList
{
    public IReadOnlyList<Deal> Items { get; init; } = Array.Empty<Deal>();
    public int? NextCursor { get; init; }
    public bool HasMore { get; init; }
}

/// <summary>
/// Request body for <c>POST /v1/deals</c>. Title is required. Stage may be one of
/// lead/qualified/proposal/negotiation — creating a deal already won/lost is rejected.
/// </summary>
public sealed class CreateDealRequest
{
    public string Title { get; set; } = string.Empty;
    public int? ContactId { get; set; }
    public decimal Value { get; set; }
    public string? Currency { get; set; }
    public string? Stage { get; set; }
    public int Probability { get; set; }
    public DateTimeOffset? ExpectedCloseAt { get; set; }
    public string? Notes { get; set; }
}
