using System;
using System.Collections.Generic;

namespace CrmSolid.Models;

/// <summary>
/// An AI Agent persona. Status is "draft"/"active"/"paused"/"archived". TriggerMode and
/// ResponseMode are kept as text so the SDK never breaks if the server adds a mode.
/// </summary>
public sealed record AiAgent
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string Status { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public IReadOnlyList<string> Channels { get; init; } = Array.Empty<string>();
    public string TriggerMode { get; init; } = string.Empty;
    public string ResponseMode { get; init; } = string.Empty;
    public string? Provider { get; init; }
    public string? Model { get; init; }
    public int RunsLast24h { get; init; }
    public DateTimeOffset? LastRunAt { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
}

/// <summary>Cursor-paginated list of AI agents.</summary>
public sealed record AiAgentList
{
    public IReadOnlyList<AiAgent> Items { get; init; } = Array.Empty<AiAgent>();
    public int? NextCursor { get; init; }
    public bool HasMore { get; init; }
}

/// <summary>A prior conversation turn fed into an agent test run.</summary>
public sealed class TestAgentHistoryItem
{
    public bool IsOutgoing { get; set; }
    public string Text { get; set; } = string.Empty;
}

/// <summary>Request body for <c>POST /v1/ai-agents/{id}/test</c> — a dry-run that never sends.</summary>
public sealed class TestAgentRequest
{
    public string IncomingText { get; set; } = string.Empty;
    public List<TestAgentHistoryItem>? History { get; set; }
}

/// <summary>The reply an agent WOULD send for the sample message, plus usage telemetry.</summary>
public sealed record TestAgentResult
{
    public string Output { get; init; } = string.Empty;
    public long LatencyMs { get; init; }
    public int? TokensInput { get; init; }
    public int? TokensOutput { get; init; }
    public int? TokensTotal { get; init; }
    public string? Provider { get; init; }
    public string? Model { get; init; }
}
