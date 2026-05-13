using System;

namespace CrmSolid.Models;

/// <summary>Queued message job returned by <c>POST /v1/telegram/messages</c> (202 Accepted).</summary>
public record MessageJob
{
    public int Id { get; init; }
    public int AccountId { get; init; }
    public JobStatus Status { get; init; }

    /// <summary>Scheduled or effective send time (UTC).</summary>
    public DateTimeOffset RunAt { get; init; }

    public DateTimeOffset CreatedAt { get; init; }
}

/// <summary>Full message job detail returned by <c>GET /v1/telegram/messages/{id}</c>.</summary>
public sealed record MessageJobDetail : MessageJob
{
    public DateTimeOffset? UpdatedAt { get; init; }

    /// <summary>Last error message if the job has failed.</summary>
    public string? LastError { get; init; }
}
