using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace CrmSolid.Webhooks;

/// <summary>
/// Verifies the <c>X-Webhook-Signature</c> header on incoming CrmSolid webhook deliveries.
/// The header value is <c>sha256=&lt;hex&gt;</c> where <c>hex</c> is the lowercase
/// HMAC-SHA256 of the <b>raw request body bytes</b> keyed by the endpoint's secret.
/// </summary>
/// <remarks>
/// Verify against the exact bytes you received, before any JSON parsing/re-serialization:
/// <code>
/// var ok = WebhookSignature.Verify(secret, rawBodyBytes, request.Headers["X-Webhook-Signature"]);
/// if (!ok) return Results.Unauthorized();
/// </code>
/// </remarks>
public static class WebhookSignature
{
    /// <summary>Computes the lowercase hex HMAC-SHA256 of <paramref name="body"/> keyed by <paramref name="secret"/>.</summary>
    public static string Compute(string secret, byte[] body)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret ?? string.Empty));
        var hash = hmac.ComputeHash(body ?? Array.Empty<byte>());
        var sb = new StringBuilder(hash.Length * 2);
        foreach (var b in hash) sb.Append(b.ToString("x2", CultureInfo.InvariantCulture));
        return sb.ToString();
    }

    /// <summary>Computes the signature over the UTF-8 bytes of <paramref name="body"/>.</summary>
    public static string Compute(string secret, string body)
        => Compute(secret, Encoding.UTF8.GetBytes(body ?? string.Empty));

    /// <summary>
    /// Verifies a signature header against the raw body bytes. Tolerates the optional
    /// <c>sha256=</c> prefix and compares in constant time.
    /// </summary>
    public static bool Verify(string secret, byte[] body, string? signatureHeader)
    {
        if (string.IsNullOrEmpty(signatureHeader)) return false;
        var given = signatureHeader!.Trim().ToLowerInvariant();
        if (given.StartsWith("sha256=", StringComparison.Ordinal))
            given = given.Substring("sha256=".Length);
        return FixedTimeEquals(Compute(secret, body), given);
    }

    /// <summary>Verifies a signature header against a body string (UTF-8 encoded).</summary>
    public static bool Verify(string secret, string body, string? signatureHeader)
        => Verify(secret, Encoding.UTF8.GetBytes(body ?? string.Empty), signatureHeader);

    private static bool FixedTimeEquals(string a, string b)
    {
        var ab = Encoding.UTF8.GetBytes(a);
        var bb = Encoding.UTF8.GetBytes(b);
        if (ab.Length != bb.Length) return false;
        var diff = 0;
        for (var i = 0; i < ab.Length; i++) diff |= ab[i] ^ bb[i];
        return diff == 0;
    }
}
