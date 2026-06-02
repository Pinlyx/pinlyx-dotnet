using System;
using System.Collections.Generic;

namespace CrmSolid.Models;

/// <summary>
/// A CRM task / reminder. Named <c>CrmTask</c> to avoid clashing with
/// <see cref="System.Threading.Tasks.Task"/>. Status is "Open"/"InProgress"/"Done";
/// priority is "Low"/"Medium"/"High".
/// </summary>
public sealed record CrmTask
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int? ContactId { get; init; }
    public string? ContactName { get; init; }
    public int? DealId { get; init; }
    public string? DealTitle { get; init; }
    public string Priority { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset? DueAt { get; init; }
    public DateTimeOffset? CompletedAt { get; init; }
    public bool IsOverdue { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
}

/// <summary>Cursor-paginated list of tasks.</summary>
public sealed record TaskList
{
    public IReadOnlyList<CrmTask> Items { get; init; } = Array.Empty<CrmTask>();
    public int? NextCursor { get; init; }
    public bool HasMore { get; init; }
}

/// <summary>Request body for <c>POST /v1/tasks</c>. Title is required.</summary>
public sealed class CreateTaskRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? ContactId { get; set; }
    public int? DealId { get; set; }
    /// <summary>low | medium | high. Defaults to medium server-side.</summary>
    public string? Priority { get; set; }
    public DateTimeOffset? DueAt { get; set; }
}
