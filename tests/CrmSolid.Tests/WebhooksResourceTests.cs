using System.Net;
using System.Threading.Tasks;
using CrmSolid.Models;
using NUnit.Framework;

namespace CrmSolid.Tests;

[TestFixture]
public class WebhooksResourceTests
{
    [Test]
    public async Task CreateAsync_ReturnsEndpointAndSecretOnce()
    {
        const string json = """
        { "endpoint": { "id": 3, "url": "https://example.com/hook", "eventTypes": ["*"], "isActive": true, "description": null, "secretPreview": "abc123…", "failureCount": 0, "createdAt": "2024-03-01T10:00:00Z" }, "secret": "s3cr3t-value" }
        """;
        var mock = new MockHttpMessageHandler(HttpStatusCode.Created, json);
        var client = TestClientFactory.Create(mock);

        var created = await client.Webhooks.CreateAsync(new CreateWebhookRequest { Url = "https://example.com/hook" });

        Assert.That(created.Secret, Is.EqualTo("s3cr3t-value"));
        Assert.That(created.Endpoint.Id, Is.EqualTo(3));
        Assert.That(mock.Requests[0].RequestUri!.AbsolutePath, Is.EqualTo("/v1/webhooks"));
        Assert.That(mock.RequestBodies[0], Does.Contain("https://example.com/hook"));
    }

    [Test]
    public async Task UpdateAsync_UsesPatchVerb()
    {
        const string json = """
        { "id": 3, "url": "https://example.com/hook", "eventTypes": ["contact.created"], "isActive": false, "secretPreview": "abc123…", "failureCount": 0, "createdAt": "2024-03-01T10:00:00Z" }
        """;
        var mock = new MockHttpMessageHandler(HttpStatusCode.OK, json);
        var client = TestClientFactory.Create(mock);

        var updated = await client.Webhooks.UpdateAsync(3, new UpdateWebhookRequest { IsActive = false });

        Assert.That(updated.IsActive, Is.False);
        Assert.That(mock.Requests[0].Method.Method, Is.EqualTo("PATCH"));
        Assert.That(mock.Requests[0].RequestUri!.AbsolutePath, Is.EqualTo("/v1/webhooks/3"));
    }
}
