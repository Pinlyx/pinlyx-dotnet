using System.Net;
using System.Threading.Tasks;
using NUnit.Framework;

namespace CrmSolid.Tests;

[TestFixture]
public class AnalyticsResourceTests
{
    [Test]
    public async Task GetSummaryAsync_ParsesNestedToday()
    {
        const string json = """
        { "contactsTotal": 42, "connectedAccounts": 2, "today": { "queued": 1, "sent": 5, "failed": 0 }, "generatedAt": "2024-03-01T10:00:00Z" }
        """;
        var mock = new MockHttpMessageHandler(HttpStatusCode.OK, json);
        var client = TestClientFactory.Create(mock);

        var summary = await client.Analytics.GetSummaryAsync();

        Assert.That(summary.ContactsTotal, Is.EqualTo(42));
        Assert.That(summary.Today.Sent, Is.EqualTo(5));
        Assert.That(mock.Requests[0].RequestUri!.AbsolutePath, Is.EqualTo("/v1/analytics/summary"));
    }

    [Test]
    public async Task GetMessagingStatsAsync_AppliesWindowAndParsesTotals()
    {
        const string json = """
        { "windowDays": 30, "accountId": null, "since": "2024-02-01T10:00:00Z", "totals": { "queued": 1, "sent": 5, "failed": 0, "total": 6 }, "successRate": 83.3, "generatedAt": "2024-03-01T10:00:00Z" }
        """;
        var mock = new MockHttpMessageHandler(HttpStatusCode.OK, json);
        var client = TestClientFactory.Create(mock);

        var stats = await client.Analytics.GetMessagingStatsAsync(windowDays: 30);

        Assert.That(stats.Totals.Total, Is.EqualTo(6));
        Assert.That(stats.SuccessRate, Is.EqualTo(83.3));
        Assert.That(mock.Requests[0].RequestUri!.Query, Does.Contain("windowDays=30"));
    }

    [Test]
    public async Task ListTopContactsAsync_ParsesItems()
    {
        const string json = """
        { "items": [ { "id": 5, "platform": "telegram", "name": "Bob", "username": "bob", "stage": "Lead", "lastMessageAt": "2024-03-01T10:00:00Z", "outboundCount": 12 } ], "windowDays": 30 }
        """;
        var mock = new MockHttpMessageHandler(HttpStatusCode.OK, json);
        var client = TestClientFactory.Create(mock);

        var items = await client.Analytics.ListTopContactsAsync(limit: 5);

        Assert.That(items, Has.Count.EqualTo(1));
        Assert.That(items[0].OutboundCount, Is.EqualTo(12));
    }
}
