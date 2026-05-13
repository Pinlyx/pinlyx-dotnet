using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CrmSolid;
using CrmSolid.Http;
using NUnit.Framework;

namespace CrmSolid.Tests;

[TestFixture]
public class RateLimitHandlerTests
{
    [Test]
    public async Task Retries_On429_RespectingRetryAfter()
    {
        var attempts = 0;
        var inner = new MockHttpMessageHandler(req =>
        {
            attempts++;
            if (attempts < 3)
            {
                var resp = new HttpResponseMessage((HttpStatusCode)429);
                resp.Headers.Add("Retry-After", "0"); // 0s — instant retry
                return Task.FromResult(resp);
            }
            return Task.FromResult(MockHttpMessageHandler.BuildResponse(HttpStatusCode.OK, "{}"));
        });

        var options = new CrmSolidOptions
        {
            AutoRetryOnRateLimit = true,
            MaxRetries = 3,
            DefaultRetryDelay = TimeSpan.FromMilliseconds(1),
            MaxRetryDelay = TimeSpan.FromMilliseconds(10),
        };
        var handler = new RateLimitHandler(options) { InnerHandler = inner };

        using var http = new HttpClient(handler) { BaseAddress = new Uri("https://api.test.local") };

        var response = await http.GetAsync("/v1/me");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(attempts, Is.EqualTo(3));
    }

    [Test]
    public async Task GivesUp_AfterMaxRetries()
    {
        var attempts = 0;
        var inner = new MockHttpMessageHandler(_ =>
        {
            attempts++;
            var resp = new HttpResponseMessage((HttpStatusCode)429);
            resp.Headers.Add("Retry-After", "0");
            return Task.FromResult(resp);
        });
        var options = new CrmSolidOptions
        {
            AutoRetryOnRateLimit = true,
            MaxRetries = 2,
            DefaultRetryDelay = TimeSpan.FromMilliseconds(1),
            MaxRetryDelay = TimeSpan.FromMilliseconds(10),
        };
        var handler = new RateLimitHandler(options) { InnerHandler = inner };
        using var http = new HttpClient(handler) { BaseAddress = new Uri("https://api.test.local") };

        var response = await http.GetAsync("/v1/me");

        Assert.That((int)response.StatusCode, Is.EqualTo(429));
        Assert.That(attempts, Is.EqualTo(3)); // initial + 2 retries
    }

    [Test]
    public async Task DoesNotRetry_WhenAutoRetryDisabled()
    {
        var attempts = 0;
        var inner = new MockHttpMessageHandler(_ =>
        {
            attempts++;
            return Task.FromResult(MockHttpMessageHandler.BuildResponse((HttpStatusCode)429, "{}"));
        });
        var handler = new RateLimitHandler(new CrmSolidOptions { AutoRetryOnRateLimit = false }) { InnerHandler = inner };
        using var http = new HttpClient(handler) { BaseAddress = new Uri("https://api.test.local") };

        await http.GetAsync("/v1/me");
        Assert.That(attempts, Is.EqualTo(1));
    }
}
