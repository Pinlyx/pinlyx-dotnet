using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using CrmSolid.Exceptions;
using NUnit.Framework;

namespace CrmSolid.Tests;

[TestFixture]
public class ErrorMappingTests
{
    [Test]
    public void Throws_AuthException_On401()
    {
        var mock = new MockHttpMessageHandler(HttpStatusCode.Unauthorized,
            """{"error":"unauthorized","message":"Invalid bearer principal"}""");
        var client = TestClientFactory.Create(mock);

        var ex = Assert.ThrowsAsync<CrmSolidAuthException>(async () => await client.Me.GetAsync());
        Assert.That(ex!.Error?.Error, Is.EqualTo("unauthorized"));
    }

    [Test]
    public void Throws_ForbiddenException_On403()
    {
        var mock = new MockHttpMessageHandler(HttpStatusCode.Forbidden,
            """{"error":"forbidden","message":"scope contacts:write is required"}""");
        var client = TestClientFactory.Create(mock);

        Assert.ThrowsAsync<CrmSolidForbiddenException>(async () => await client.Me.GetAsync());
    }

    [Test]
    public void Throws_NotFoundException_On404()
    {
        var mock = new MockHttpMessageHandler(HttpStatusCode.NotFound,
            """{"error":"not_found","message":"contact not found"}""");
        var client = TestClientFactory.Create(mock);

        Assert.ThrowsAsync<CrmSolidNotFoundException>(async () => await client.Contacts.GetAsync(999));
    }

    [Test]
    public void Throws_ValidationException_On400()
    {
        var mock = new MockHttpMessageHandler(HttpStatusCode.BadRequest,
            """{"error":"bad_request","message":"text is required"}""");
        var client = TestClientFactory.Create(mock);

        Assert.ThrowsAsync<CrmSolidValidationException>(async () => await client.Me.GetAsync());
    }

    [Test]
    public void Throws_RateLimitException_On429_WithHeaders()
    {
        var headers = new Dictionary<string, string>
        {
            ["Retry-After"] = "30",
            ["X-RateLimit-Limit"] = "60",
            ["X-RateLimit-Remaining"] = "0",
            ["X-RateLimit-Reset"] = "1700000000",
            ["X-Request-Id"] = "req-abc123",
        };
        var mock = new MockHttpMessageHandler((HttpStatusCode)429,
            """{"error":"rate_limited","message":"rate limit exceeded"}""",
            headers);
        var client = TestClientFactory.Create(mock);

        var ex = Assert.ThrowsAsync<CrmSolidRateLimitException>(async () => await client.Me.GetAsync());
        Assert.That(ex!.Limit, Is.EqualTo(60));
        Assert.That(ex.Remaining, Is.EqualTo(0));
        Assert.That(ex.ResetAtUnix, Is.EqualTo(1700000000));
        Assert.That(ex.RetryAfter, Is.EqualTo(System.TimeSpan.FromSeconds(30)));
        Assert.That(ex.RequestId, Is.EqualTo("req-abc123"));
    }
}
