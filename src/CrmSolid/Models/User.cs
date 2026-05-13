using System;
using System.Collections.Generic;

namespace CrmSolid.Models;

/// <summary>Workspace identity returned by <c>GET /v1/me</c>.</summary>
public sealed record User
{
    /// <summary>Internal user id.</summary>
    public int Id { get; init; }

    /// <summary>Workspace email address.</summary>
    public string? Email { get; init; }

    /// <summary>Display name (maps to FullName on the user record).</summary>
    public string? Name { get; init; }

    /// <summary>Account creation timestamp (UTC).</summary>
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>Metadata about the API key used in this request.</summary>
    public ApiKeyMetadata? ApiKey { get; init; }
}

/// <summary>Metadata about the API key authenticating the current request.</summary>
public sealed record ApiKeyMetadata
{
    /// <summary>12-character key identifier (the public part of the bearer token).</summary>
    public string KeyId { get; init; } = string.Empty;

    /// <summary>Scopes granted to this key.</summary>
    public IReadOnlyList<string> Scopes { get; init; } = Array.Empty<string>();
}
