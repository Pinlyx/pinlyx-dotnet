using System.Net;
using System.Threading.Tasks;
using NUnit.Framework;

namespace CrmSolid.Tests;

[TestFixture]
public class MeResourceTests
{
    [Test]
    public async Task GetAsync_DeserializesUserAndApiKey()
    {
        const string json = """
        {
          "id": 42,
          "email": "jane@example.com",
          "name": "Acme Inc.",
          "createdAt": "2024-01-15T09:00:00Z",
          "apiKey": {
            "keyId": "abc123def456",
            "scopes": ["contacts:read", "telegram:send"]
          }
        }
        """;
        var mock = new MockHttpMessageHandler(HttpStatusCode.OK, json);
        var client = TestClientFactory.Create(mock);

        var me = await client.Me.GetAsync();

        Assert.That(me.Id, Is.EqualTo(42));
        Assert.That(me.Email, Is.EqualTo("jane@example.com"));
        Assert.That(me.Name, Is.EqualTo("Acme Inc."));
        Assert.That(me.ApiKey, Is.Not.Null);
        Assert.That(me.ApiKey!.KeyId, Is.EqualTo("abc123def456"));
        Assert.That(me.ApiKey.Scopes, Is.EquivalentTo(new[] { "contacts:read", "telegram:send" }));

        Assert.That(mock.Requests, Has.Count.EqualTo(1));
        Assert.That(mock.Requests[0].RequestUri!.AbsolutePath, Is.EqualTo("/v1/me"));
        Assert.That(mock.Requests[0].Headers.Authorization!.Scheme, Is.EqualTo("Bearer"));
    }
}
