using System;

namespace CrmSolid.Models;

/// <summary>A bearer API key the caller owns. Secrets / token are never returned here.</summary>
public sealed record ApiKeySummary
{
    public int Id { get; init; }
    public string KeyId { get; init; } = string.Empty;
    public string? Name { get; init; }
    public string Scopes { get; init; } = string.Empty;
    public int RateLimitPerMinute { get; init; }
    public string? EnvLabel { get; init; }
    public bool IsActive { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? LastUsedAt { get; init; }
    public DateTimeOffset? ExpiresAt { get; init; }
    public DateTimeOffset? RevokedAt { get; init; }
}

/// <summary>A freshly minted key — the full <see cref="Token"/> is present ONCE.</summary>
public sealed record CreatedApiKey
{
    public int Id { get; init; }
    public string KeyId { get; init; } = string.Empty;
    /// <summary>The full bearer token (<c>csk_…</c>). Returned once — store it now.</summary>
    public string Token { get; init; } = string.Empty;
    public string Prefix { get; init; } = string.Empty;
    public string EnvLabel { get; init; } = string.Empty;
    public string Scopes { get; init; } = string.Empty;
    public int RateLimitPerMinute { get; init; }
    public DateTimeOffset? ExpiresAt { get; init; }
}

/// <summary>
/// Request body for <c>POST /v1/api-keys</c> (requires <c>keys:manage</c>). Minted keys are
/// attenuated: <see cref="Scopes"/> must be a subset of the calling key's scopes.
/// </summary>
public sealed class CreateApiKeyRequest
{
    public string? Name { get; set; }
    /// <summary>Space-separated scopes. Null inherits the parent key's concrete scopes.</summary>
    public string? Scopes { get; set; }
    /// <summary>1–2000 requests/minute. Default 60.</summary>
    public int? RateLimitPerMinute { get; set; }
    public string? AllowIps { get; set; }
    /// <summary>"live" or "test". Default "live".</summary>
    public string? EnvLabel { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
}
