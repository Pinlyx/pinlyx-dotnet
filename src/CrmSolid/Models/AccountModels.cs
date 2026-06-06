using System;
using System.Collections.Generic;

namespace CrmSolid.Models;

/// <summary>A connected messaging account (Telegram or Twitter/X). Secrets are never returned.</summary>
public sealed record ConnectedAccount
{
    public int Id { get; init; }
    /// <summary>"telegram" or "twitter".</summary>
    public string Type { get; init; } = string.Empty;
    public string? Name { get; init; }
    public string? Phone { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset LastUsedAt { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}

/// <summary>The caller's connected accounts with per-type counts (<c>GET /v1/accounts</c>).</summary>
public sealed record AccountList
{
    public IReadOnlyList<ConnectedAccount> Items { get; init; } = Array.Empty<ConnectedAccount>();
    public int Telegram { get; init; }
    public int Twitter { get; init; }
}
