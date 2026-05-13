using System;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace CrmSolid.Http;

/// <summary>
/// Delegating handler that retries HTTP 429 responses up to
/// <see cref="CrmSolidOptions.MaxRetries"/> times, honoring the
/// <c>Retry-After</c> and <c>X-RateLimit-Reset</c> headers.
/// </summary>
public sealed class RateLimitHandler : DelegatingHandler
{
    private readonly CrmSolidOptions _options;

    public RateLimitHandler(CrmSolidOptions options)
    {
        _options = options;
    }

    public RateLimitHandler(IOptions<CrmSolidOptions> options) : this(options.Value) { }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (!_options.AutoRetryOnRateLimit)
            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

        var attempt = 0;
        while (true)
        {
            var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

            if ((int)response.StatusCode != 429 || attempt >= _options.MaxRetries)
                return response;

            var delay = ComputeDelay(response, attempt);
            response.Dispose();

            await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
            attempt++;
        }
    }

    private TimeSpan ComputeDelay(HttpResponseMessage response, int attempt)
    {
        TimeSpan? hinted = null;

        // Retry-After: seconds delta or HTTP-date
        if (response.Headers.RetryAfter != null)
        {
            if (response.Headers.RetryAfter.Delta.HasValue)
                hinted = response.Headers.RetryAfter.Delta;
            else if (response.Headers.RetryAfter.Date.HasValue)
                hinted = response.Headers.RetryAfter.Date.Value - DateTimeOffset.UtcNow;
        }

        // X-RateLimit-Reset (unix seconds)
        if (hinted == null && response.Headers.TryGetValues("X-RateLimit-Reset", out var resetVals))
        {
            var v = resetVals.FirstOrDefault();
            if (long.TryParse(v, NumberStyles.Integer, CultureInfo.InvariantCulture, out var unix))
                hinted = DateTimeOffset.FromUnixTimeSeconds(unix) - DateTimeOffset.UtcNow;
        }

        // Fallback: exponential backoff from DefaultRetryDelay.
        var fallback = TimeSpan.FromTicks(_options.DefaultRetryDelay.Ticks * (long)Math.Pow(2, attempt));

        var chosen = hinted.HasValue && hinted.Value > TimeSpan.Zero ? hinted.Value : fallback;
        if (chosen < TimeSpan.Zero) chosen = _options.DefaultRetryDelay;
        if (chosen > _options.MaxRetryDelay) chosen = _options.MaxRetryDelay;
        return chosen;
    }
}
