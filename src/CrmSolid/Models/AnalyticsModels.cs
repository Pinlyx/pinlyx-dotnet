using System;

namespace CrmSolid.Models;

/// <summary>Today's queued/sent/failed message counts.</summary>
public sealed record TodayCounts
{
    public int Queued { get; init; }
    public int Sent { get; init; }
    public int Failed { get; init; }
}

/// <summary>High-level account KPIs (<c>GET /v1/analytics/summary</c>).</summary>
public sealed record AnalyticsSummary
{
    public int ContactsTotal { get; init; }
    public int ConnectedAccounts { get; init; }
    public TodayCounts Today { get; init; } = new();
    public DateTimeOffset GeneratedAt { get; init; }
}

/// <summary>Windowed message totals.</summary>
public sealed record MessagingTotals
{
    public int Queued { get; init; }
    public int Sent { get; init; }
    public int Failed { get; init; }
    public int Total { get; init; }
}

/// <summary>Windowed messaging stats (<c>GET /v1/analytics/messaging</c>).</summary>
public sealed record MessagingStats
{
    public int WindowDays { get; init; }
    public int? AccountId { get; init; }
    public DateTimeOffset Since { get; init; }
    public MessagingTotals Totals { get; init; } = new();
    /// <summary>Sent / total as a percentage (0-100), or null when no messages in the window.</summary>
    public double? SuccessRate { get; init; }
    public DateTimeOffset GeneratedAt { get; init; }
}

/// <summary>A top contact ranked by outbound message volume over the last 30 days.</summary>
public sealed record TopContact
{
    public int Id { get; init; }
    public string Platform { get; init; } = string.Empty;
    public string? Name { get; init; }
    public string? Username { get; init; }
    public string Stage { get; init; } = string.Empty;
    public DateTimeOffset? LastMessageAt { get; init; }
    public int OutboundCount { get; init; }
}
