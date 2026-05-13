using System;
using System.Net.Http;
using CrmSolid;
using CrmSolid.Http;

namespace CrmSolid.Tests;

internal static class TestClientFactory
{
    public const string ValidBearerToken =
        "csk_test_abc123def456" + // 12-char keyId
        "abcdefghijklmnopqrstuvwxyz012345"; // 32-char secret

    /// <summary>
    /// Builds a <see cref="CrmSolidClient"/> wired to the given mock handler,
    /// preserving the full handler chain (rate-limit → auth → mock) so tests
    /// exercise the real request pipeline.
    /// </summary>
    public static CrmSolidClient Create(MockHttpMessageHandler mock, CrmSolidOptions? options = null)
    {
        options ??= new CrmSolidOptions
        {
            ApiKey = ValidBearerToken,
            BaseAddress = new Uri("https://api.test.local"),
            AutoRetryOnRateLimit = false,
        };

        var credentials = CrmSolidClient.ResolveCredentials(options);
        var auth = new CrmSolidAuthHandler(credentials) { InnerHandler = mock };
        var rate = new RateLimitHandler(options) { InnerHandler = auth };

        var http = new HttpClient(rate) { BaseAddress = options.BaseAddress };
        return new CrmSolidClient(http, options);
    }
}
