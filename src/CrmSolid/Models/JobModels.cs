using System;
using System.Collections.Generic;

namespace CrmSolid.Models;

/// <summary>
/// An outbound message job (the unit of work behind Telegram sends and sequences).
/// Status is "queued"/"sent"/"failed".
/// </summary>
public sealed record Job
{
    public int Id { get; init; }
    public int AccountId { get; init; }
    public string Status { get; init; } = string.Empty;
    public long? TargetId { get; init; }
    public string? TargetUsername { get; init; }
    public string? Text { get; init; }
    public DateTimeOffset RunAt { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
    public string? LastError { get; init; }
}

/// <summary>Cursor-paginated list of jobs.</summary>
public sealed record JobList
{
    public IReadOnlyList<Job> Items { get; init; } = Array.Empty<Job>();
    public int? NextCursor { get; init; }
    public bool HasMore { get; init; }
}
