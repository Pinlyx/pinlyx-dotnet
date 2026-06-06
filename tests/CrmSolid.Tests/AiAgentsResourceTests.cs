using System.Net;
using System.Threading.Tasks;
using CrmSolid.Models;
using NUnit.Framework;

namespace CrmSolid.Tests;

[TestFixture]
public class AiAgentsResourceTests
{
    [Test]
    public async Task ListAsync_ParsesAgentsWithChannels()
    {
        const string json = """
        { "items": [ { "id": 1, "name": "Support bot", "description": "d", "status": "active", "isActive": true, "channels": ["telegram","email"], "triggerMode": "AllMessages", "responseMode": "Suggest", "provider": "openai", "model": "gpt-4o", "runsLast24h": 5, "lastRunAt": "2024-03-01T10:00:00Z", "createdAt": "2024-02-01T10:00:00Z", "updatedAt": null } ], "nextCursor": 1, "hasMore": false }
        """;
        var mock = new MockHttpMessageHandler(HttpStatusCode.OK, json);
        var client = TestClientFactory.Create(mock);

        var page = await client.AiAgents.ListAsync();

        Assert.That(page.Items, Has.Count.EqualTo(1));
        Assert.That(page.Items[0].Channels, Does.Contain("telegram"));
        Assert.That(page.Items[0].RunsLast24h, Is.EqualTo(5));
    }

    [Test]
    public async Task TestAsync_PostsIncomingTextAndParsesOutput()
    {
        const string json = """
        { "output": "Hello! How can I help?", "latencyMs": 850, "tokensInput": 120, "tokensOutput": 12, "tokensTotal": 132, "provider": "openai", "model": "gpt-4o" }
        """;
        var mock = new MockHttpMessageHandler(HttpStatusCode.OK, json);
        var client = TestClientFactory.Create(mock);

        var result = await client.AiAgents.TestAsync(1, new TestAgentRequest { IncomingText = "hi" });

        Assert.That(result.Output, Does.Contain("help"));
        Assert.That(result.TokensTotal, Is.EqualTo(132));
        Assert.That(mock.Requests[0].RequestUri!.AbsolutePath, Is.EqualTo("/v1/ai-agents/1/test"));
        Assert.That(mock.RequestBodies[0], Does.Contain("\"incomingText\":\"hi\""));
    }
}
