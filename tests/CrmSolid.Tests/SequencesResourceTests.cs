using System.Net;
using System.Threading.Tasks;
using NUnit.Framework;

namespace CrmSolid.Tests;

[TestFixture]
public class SequencesResourceTests
{
    [Test]
    public async Task ListAsync_ParsesItemsAndFiltersStatus()
    {
        const string json = """
        { "items": [ { "id": 1, "name": "Promo", "status": "active", "dailyLimit": 50, "totalTargets": 100, "processedTargets": 10, "successfulTargets": 8, "failedTargets": 2, "createdAt": "2024-03-01T10:00:00Z", "lastProcessedAt": null } ] }
        """;
        var mock = new MockHttpMessageHandler(HttpStatusCode.OK, json);
        var client = TestClientFactory.Create(mock);

        var items = await client.Sequences.ListAsync(status: "active");

        Assert.That(items, Has.Count.EqualTo(1));
        Assert.That(items[0].Name, Is.EqualTo("Promo"));
        Assert.That(items[0].SuccessfulTargets, Is.EqualTo(8));
        Assert.That(mock.Requests[0].RequestUri!.Query, Does.Contain("status=active"));
    }

    [Test]
    public async Task GetAsync_ParsesDetailWithSteps()
    {
        const string json = """
        { "id": 1, "name": "Promo", "status": "paused", "dailyLimit": 50, "totalTargets": 100, "processedTargets": 10, "successfulTargets": 8, "failedTargets": 2, "createdAt": "2024-03-01T10:00:00Z", "lastProcessedAt": null, "description": "demo", "pendingJobs": 3, "processedJobs": 7, "steps": [ { "step": 1, "delayDays": 0, "preview": "Hello" } ] }
        """;
        var mock = new MockHttpMessageHandler(HttpStatusCode.OK, json);
        var client = TestClientFactory.Create(mock);

        var detail = await client.Sequences.GetAsync(1);

        Assert.That(detail.PendingJobs, Is.EqualTo(3));
        Assert.That(detail.Steps, Has.Count.EqualTo(1));
        Assert.That(detail.Steps[0].Preview, Is.EqualTo("Hello"));
    }

    [Test]
    public async Task PauseAsync_PostsToPausePath()
    {
        const string json = """{ "id": 1, "status": "paused", "message": "Sequence paused" }""";
        var mock = new MockHttpMessageHandler(HttpStatusCode.OK, json);
        var client = TestClientFactory.Create(mock);

        var result = await client.Sequences.PauseAsync(1);

        Assert.That(result.Status, Is.EqualTo("paused"));
        Assert.That(mock.Requests[0].RequestUri!.AbsolutePath, Is.EqualTo("/v1/sequences/1/pause"));
    }
}
