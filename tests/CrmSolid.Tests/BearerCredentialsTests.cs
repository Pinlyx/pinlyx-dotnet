using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CrmSolid.Auth;
using NUnit.Framework;

namespace CrmSolid.Tests;

[TestFixture]
public class BearerCredentialsTests
{
    [Test]
    public async Task ApplyAsync_SetsAuthorizationHeader()
    {
        var creds = new BearerCredentials(TestClientFactory.ValidBearerToken);
        using var req = new HttpRequestMessage(HttpMethod.Get, "https://api.test.local/v1/me");

        await creds.ApplyAsync(req, CancellationToken.None);

        Assert.That(req.Headers.Authorization, Is.Not.Null);
        Assert.That(req.Headers.Authorization!.Scheme, Is.EqualTo("Bearer"));
        Assert.That(req.Headers.Authorization.Parameter, Is.EqualTo(TestClientFactory.ValidBearerToken));
    }

    [Test]
    public void Ctor_RejectsNull()
    {
        Assert.Throws<ArgumentException>(() => _ = new BearerCredentials(null!));
    }

    [Test]
    public void Ctor_RejectsEmpty()
    {
        Assert.Throws<ArgumentException>(() => _ = new BearerCredentials(""));
    }

    [Test]
    public void Ctor_RejectsWrongPrefix()
    {
        Assert.Throws<ArgumentException>(() => _ = new BearerCredentials("Bearer abc"));
        Assert.Throws<ArgumentException>(() => _ = new BearerCredentials("sk_live_abc"));
    }
}
