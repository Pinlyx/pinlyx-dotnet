using System;
using System.Collections.Generic;

namespace CrmSolid.Models;

/// <summary>
/// An outbound message sequence (campaign). Status is "active"/"paused"/"completed"/"cancelled" —
/// kept as text so the SDK never breaks if the server adds a status.
/// </summary>
public record Sequence
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public int DailyLimit { get; init; }
    public int TotalTargets { get; init; }
    public int ProcessedTargets { get; init; }
    public int SuccessfulTargets { get; init; }
    public int FailedTargets { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? LastProcessedAt { get; init; }
}

/// <summary>A single message step within a sequence.</summary>
public sealed record SequenceStep
{
    public int Step { get; init; }
    public int DelayDays { get; init; }
    public string Preview { get; init; } = string.Empty;
}

/// <summary>Full sequence detail with steps and job counts (<c>GET /v1/sequences/{id}</c>).</summary>
public sealed record SequenceDetail : Sequence
{
    public string? Description { get; init; }
    public int PendingJobs { get; init; }
    public int ProcessedJobs { get; init; }
    public IReadOnlyList<SequenceStep> Steps { get; init; } = Array.Empty<SequenceStep>();
}

/// <summary>Result of a pause/resume operation (idempotent).</summary>
public sealed record SequenceStatusResult
{
    public int Id { get; init; }
    public string Status { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}
