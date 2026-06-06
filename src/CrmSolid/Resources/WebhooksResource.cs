using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CrmSolid.Http;
using CrmSolid.Models;

namespace CrmSolid.Resources;

/// <summary>
/// Self-service webhooks. Backed by <c>/v1/webhooks</c>. Read with <c>webhooks:read</c>,
/// register/rotate/delete with <c>webhooks:write</c>. Verify incoming deliveries with
/// <see cref="CrmSolid.Webhooks.WebhookSignature"/>.
/// </summary>
public sealed class WebhooksResource
{
    private static readonly object EmptyBody = new();

    private readonly CrmSolidHttpClient _http;

    internal WebhooksResource(CrmSolidHttpClient http) { _http = http; }

    /// <summary>Lists the event types an endpoint may subscribe to.</summary>
    public async Task<IReadOnlyList<string>> ListEventTypesAsync(CancellationToken cancellationToken = default)
    {
        var res = await _http.GetAsync<ItemsResponse<string>>("v1/webhooks/event-types", cancellationToken).ConfigureAwait(false);
        return res.Items;
    }

    /// <summary>Lists the caller's registered endpoints.</summary>
    public async Task<IReadOnlyList<WebhookEndpoint>> ListAsync(CancellationToken cancellationToken = default)
    {
        var res = await _http.GetAsync<ItemsResponse<WebhookEndpoint>>("v1/webhooks", cancellationToken).ConfigureAwait(false);
        return res.Items;
    }

    /// <summary>Returns a single endpoint.</summary>
    public Task<WebhookEndpoint> GetAsync(int id, CancellationToken cancellationToken = default)
        => _http.GetAsync<WebhookEndpoint>("v1/webhooks/" + id.ToString(CultureInfo.InvariantCulture), cancellationToken);

    /// <summary>Registers a new endpoint. The signing secret is returned ONCE — store it.</summary>
    public Task<WebhookEndpointWithSecret> CreateAsync(CreateWebhookRequest request, CancellationToken cancellationToken = default)
    {
        if (request is null) throw new ArgumentNullException(nameof(request));
        return _http.PostJsonAsync<CreateWebhookRequest, WebhookEndpointWithSecret>("v1/webhooks", request, cancellationToken);
    }

    /// <summary>Updates an endpoint. Only non-null fields on <paramref name="request"/> are applied.</summary>
    public Task<WebhookEndpoint> UpdateAsync(int id, UpdateWebhookRequest request, CancellationToken cancellationToken = default)
    {
        if (request is null) throw new ArgumentNullException(nameof(request));
        return _http.PatchJsonAsync<UpdateWebhookRequest, WebhookEndpoint>(
            "v1/webhooks/" + id.ToString(CultureInfo.InvariantCulture), request, cancellationToken);
    }

    /// <summary>Disables an endpoint (soft), or deletes it permanently when <paramref name="hard"/> is true.</summary>
    public async Task DeleteAsync(int id, bool hard = false, CancellationToken cancellationToken = default)
    {
        var qs = new QueryStringBuilder();
        if (hard) qs.Add("hard", "true");
        await _http.DeleteAsync<JsonElement>("v1/webhooks/" + id.ToString(CultureInfo.InvariantCulture) + qs, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Rotates the signing secret. The new secret is returned ONCE.</summary>
    public Task<WebhookEndpointWithSecret> RotateSecretAsync(int id, CancellationToken cancellationToken = default)
        => _http.PostJsonAsync<object, WebhookEndpointWithSecret>(
            "v1/webhooks/" + id.ToString(CultureInfo.InvariantCulture) + "/rotate-secret", EmptyBody, cancellationToken);

    /// <summary>Queues a <c>webhook.test</c> delivery so you can verify your receiver end-to-end.</summary>
    public Task<WebhookTestResult> SendTestAsync(int id, CancellationToken cancellationToken = default)
        => _http.PostJsonAsync<object, WebhookTestResult>(
            "v1/webhooks/" + id.ToString(CultureInfo.InvariantCulture) + "/test", EmptyBody, cancellationToken);

    /// <summary>Lists recent delivery attempts for an endpoint, newest-first.</summary>
    public Task<WebhookDeliveryPage> ListDeliveriesAsync(int id, long? before = null, int limit = 50, CancellationToken cancellationToken = default)
    {
        var qs = new QueryStringBuilder();
        if (before.HasValue) qs.Add("before", before.Value.ToString(CultureInfo.InvariantCulture));
        if (limit != 50) qs.Add("limit", limit.ToString(CultureInfo.InvariantCulture));
        return _http.GetAsync<WebhookDeliveryPage>(
            "v1/webhooks/" + id.ToString(CultureInfo.InvariantCulture) + "/deliveries" + qs, cancellationToken);
    }
}
