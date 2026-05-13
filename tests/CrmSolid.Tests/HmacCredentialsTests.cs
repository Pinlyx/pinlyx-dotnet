using System;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CrmSolid.Auth;
using NUnit.Framework;

namespace CrmSolid.Tests;

[TestFixture]
public class HmacCredentialsTests
{
    private const string EmptyBodySha256 =
        "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855";

    [Test]
    public async Task ApplyAsync_SetsAllThreeHeaders()
    {
        var creds = new HmacCredentials("abc123def456", "supersecret");
        using var req = new HttpRequestMessage(HttpMethod.Get, "https://api.test.local/public/foo");

        await creds.ApplyAsync(req, CancellationToken.None);

        Assert.That(req.Headers.GetValues("X-API-Key").Single(), Is.EqualTo("abc123def456"));
        Assert.That(req.Headers.Contains("X-Timestamp"), Is.True);
        Assert.That(req.Headers.Contains("X-Signature"), Is.True);
    }

    [Test]
    public async Task ApplyAsync_ProducesServerCompatibleSignature_NoBody()
    {
        var creds = new HmacCredentials("abc123def456", "supersecret");
        using var req = new HttpRequestMessage(HttpMethod.Get, "https://api.test.local/public/foo?bar=baz");

        await creds.ApplyAsync(req, CancellationToken.None);

        var ts = req.Headers.GetValues("X-Timestamp").Single();
        var sig = req.Headers.GetValues("X-Signature").Single();

        // Recompute the same way PublicApiHmacHandler does on the server.
        var canonical = $"GET\n/public/foo?bar=baz\n{EmptyBodySha256}\n{ts}";
        var expected = ComputeServerSignature("supersecret", canonical);
        Assert.That(sig, Is.EqualTo(expected));
    }

    [Test]
    public async Task ApplyAsync_ProducesServerCompatibleSignature_WithBody()
    {
        var creds = new HmacCredentials("abc123def456", "supersecret");
        const string body = "{\"hello\":\"world\"}";
        using var req = new HttpRequestMessage(HttpMethod.Post, "https://api.test.local/public/echo")
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json"),
        };

        await creds.ApplyAsync(req, CancellationToken.None);

        var ts = req.Headers.GetValues("X-Timestamp").Single();
        var sig = req.Headers.GetValues("X-Signature").Single();

        var bodyHash = Sha256Hex(body);
        var canonical = $"POST\n/public/echo\n{bodyHash}\n{ts}";
        var expected = ComputeServerSignature("supersecret", canonical);
        Assert.That(sig, Is.EqualTo(expected));
    }

    [Test]
    public void Ctor_RejectsNullOrEmpty()
    {
        Assert.Throws<ArgumentException>(() => _ = new HmacCredentials("", "x"));
        Assert.Throws<ArgumentException>(() => _ = new HmacCredentials("x", ""));
    }

    private static string Sha256Hex(string input)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
        return BytesToHexLower(bytes);
    }

    private static string BytesToHexLower(byte[] bytes)
    {
        var sb = new StringBuilder(bytes.Length * 2);
        foreach (var b in bytes) sb.AppendFormat("{0:x2}", b);
        return sb.ToString();
    }

    private static string ComputeServerSignature(string secret, string canonical)
    {
        using var mac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = mac.ComputeHash(Encoding.UTF8.GetBytes(canonical));
        return Convert.ToBase64String(hash).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }
}
