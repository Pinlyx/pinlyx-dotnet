using System;
using System.Globalization;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CrmSolid.Auth;

/// <summary>
/// HMAC-SHA256 credentials for the legacy <c>/public/*</c> endpoints. Sets the headers
/// <c>X-API-Key</c>, <c>X-Timestamp</c>, and <c>X-Signature</c>.
/// </summary>
/// <remarks>
/// The canonical signing string is:
/// <c>{METHOD}\n{path-and-query}\n{sha256-hex(body)}\n{unix-timestamp-seconds}</c>.
/// The HMAC-SHA256 output is then base64-url encoded (without padding) and sent as
/// <c>X-Signature</c>. The server allows ±5 minutes of clock skew by default and
/// rejects replays within a 5-minute window.
/// </remarks>
public sealed class HmacCredentials : ICrmSolidCredentials
{
    // SHA256("") in lowercase hex — used when the request has no body.
    private const string EmptyBodySha256 =
        "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855";

    private readonly string _keyId;
    private readonly byte[] _secretBytes;

    /// <param name="keyId">The public key id (matches <c>PublicApiKeys.KeyId</c> server-side).</param>
    /// <param name="secret">The raw secret string associated with <paramref name="keyId"/>.</param>
    public HmacCredentials(string keyId, string secret)
    {
        if (string.IsNullOrWhiteSpace(keyId))
            throw new ArgumentException("keyId required", nameof(keyId));
        if (string.IsNullOrWhiteSpace(secret))
            throw new ArgumentException("secret required", nameof(secret));

        _keyId = keyId;
        _secretBytes = Encoding.UTF8.GetBytes(secret);
    }

    /// <inheritdoc />
    public async Task ApplyAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            .ToString(CultureInfo.InvariantCulture);
        var method = request.Method.Method.ToUpperInvariant();
        var pathAndQuery = request.RequestUri?.PathAndQuery ?? "/";

        var bodyHashHex = await ComputeBodyHashAsync(request, cancellationToken).ConfigureAwait(false);
        var canonical = string.Concat(
            method, "\n",
            pathAndQuery, "\n",
            bodyHashHex, "\n",
            timestamp);

        using var mac = new HMACSHA256(_secretBytes);
        var signatureBytes = mac.ComputeHash(Encoding.UTF8.GetBytes(canonical));
        var signature = Base64Url(signatureBytes);

        request.Headers.Remove("X-API-Key");
        request.Headers.Remove("X-Timestamp");
        request.Headers.Remove("X-Signature");
        request.Headers.Add("X-API-Key", _keyId);
        request.Headers.Add("X-Timestamp", timestamp);
        request.Headers.Add("X-Signature", signature);
    }

    private static async Task<string> ComputeBodyHashAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (request.Content == null) return EmptyBodySha256;

        byte[] bytes;
#if NET6_0_OR_GREATER
        bytes = await request.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
#else
        bytes = await request.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
#endif

#if NET5_0_OR_GREATER
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
#else
        using var sha = SHA256.Create();
        var hash = sha.ComputeHash(bytes);
        var sb = new StringBuilder(hash.Length * 2);
        foreach (var b in hash)
            sb.Append(b.ToString("x2", CultureInfo.InvariantCulture));
        return sb.ToString();
#endif
    }

    private static string Base64Url(byte[] data) =>
        Convert.ToBase64String(data)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
}
