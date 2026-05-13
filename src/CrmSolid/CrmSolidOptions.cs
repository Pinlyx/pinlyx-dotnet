using System;

namespace CrmSolid;

/// <summary>
/// Configuration for <see cref="CrmSolidClient"/>. Set <see cref="ApiKey"/> for Bearer
/// auth (recommended), or <see cref="HmacKeyId"/> + <see cref="HmacSecret"/> for HMAC
/// auth against the legacy <c>/public/*</c> endpoints.
/// </summary>
public sealed class CrmSolidOptions
{
    /// <summary>API base URL. Defaults to <c>https://api.crmsolid.com</c>.</summary>
    public Uri BaseAddress { get; set; } = new Uri("https://api.crmsolid.com");

    /// <summary>
    /// Bearer API key (token format <c>csk_{env}_{12-char-keyId}{32-char-secret}</c>).
    /// Used for <c>/v1/*</c> and <c>/mcp</c>.
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>Key id portion of an HMAC credential (for legacy <c>/public/*</c>).</summary>
    public string? HmacKeyId { get; set; }

    /// <summary>Raw secret portion of an HMAC credential.</summary>
    public string? HmacSecret { get; set; }

    /// <summary>If true (default), the SDK automatically retries on HTTP 429 up to <see cref="MaxRetries"/>.</summary>
    public bool AutoRetryOnRateLimit { get; set; } = true;

    /// <summary>Maximum number of retries on 429. Default: 3.</summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>Fallback delay when <c>Retry-After</c> / <c>X-RateLimit-Reset</c> are absent. Default: 1s.</summary>
    public TimeSpan DefaultRetryDelay { get; set; } = TimeSpan.FromSeconds(1);

    /// <summary>Upper bound on retry delays (caps server-suggested waits). Default: 60s.</summary>
    public TimeSpan MaxRetryDelay { get; set; } = TimeSpan.FromSeconds(60);

    /// <summary>HTTP request timeout. Default: 30s.</summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>User-Agent header. Defaults to <c>CrmSolid-dotnet/{version}</c>.</summary>
    public string? UserAgent { get; set; }
}
