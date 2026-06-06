using System;
using System.Collections.Generic;

namespace CrmSolid.Models;

/// <summary>A registered outbound webhook endpoint. The signing secret is never returned in full.</summary>
public sealed record WebhookEndpoint
{
    public int Id { get; init; }
    public string Url { get; init; } = string.Empty;
    /// <summary>Subscribed event types, or <c>["*"]</c> for all.</summary>
    public IReadOnlyList<string> EventTypes { get; init; } = Array.Empty<string>();
    public bool IsActive { get; init; }
    public string? Description { get; init; }
    /// <summary>First few characters of the signing secret, for display only.</summary>
    public string SecretPreview { get; init; } = string.Empty;
    public int FailureCount { get; init; }
    public DateTimeOffset? LastDeliveredAt { get; init; }
    public DateTimeOffset? LastFailedAt { get; init; }
    public string? LastFailureReason { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
}

/// <summary>An endpoint plus its signing secret — returned ONCE at creation / rotation.</summary>
public sealed record WebhookEndpointWithSecret
{
    public WebhookEndpoint Endpoint { get; init; } = new();
    /// <summary>The HMAC signing secret. Store it now — it cannot be retrieved again.</summary>
    public string Secret { get; init; } = string.Empty;
}

/// <summary>A single delivery attempt record for a webhook endpoint.</summary>
public sealed record WebhookDelivery
{
    public long Id { get; init; }
    public int EndpointId { get; init; }
    public string EventId { get; init; } = string.Empty;
    public string EventType { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public int Attempts { get; init; }
    public int? LastResponseCode { get; init; }
    public string? LastError { get; init; }
    public DateTimeOffset? NextAttemptAt { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? DeliveredAt { get; init; }
}

/// <summary>A page of deliveries. Pass <see cref="NextBefore"/> as the next <c>before</c> cursor.</summary>
public sealed record WebhookDeliveryPage
{
    public IReadOnlyList<WebhookDelivery> Items { get; init; } = Array.Empty<WebhookDelivery>();
    public long? NextBefore { get; init; }
}

/// <summary>Result of queuing a test delivery.</summary>
public sealed record WebhookTestResult
{
    public long DeliveryId { get; init; }
    public string EventId { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
}

/// <summary>Request body for <c>POST /v1/webhooks</c>. Null/empty <see cref="EventTypes"/> subscribes to all.</summary>
public sealed class CreateWebhookRequest
{
    public string Url { get; set; } = string.Empty;
    public string[]? EventTypes { get; set; }
    public string? Description { get; set; }
}

/// <summary>Request body for <c>PATCH /v1/webhooks/{id}</c>. Only non-null fields are applied.</summary>
public sealed class UpdateWebhookRequest
{
    public string? Url { get; set; }
    public string[]? EventTypes { get; set; }
    public string? Description { get; set; }
    public bool? IsActive { get; set; }
}
