using System.Net;
using System.Threading.Tasks;
using CrmSolid.Models;
using NUnit.Framework;

namespace CrmSolid.Tests;

[TestFixture]
public class TasksResourceTests
{
    [Test]
    public async Task ListAsync_ParsesAndAppliesFilters()
    {
        const string json = """
        {
          "items": [
            { "id": 3, "title": "Call back", "description": null, "contactId": 5, "contactName": "Acme", "dealId": null, "dealTitle": null, "priority": "High", "status": "Open", "dueAt": "2024-03-05T09:00:00Z", "completedAt": null, "isOverdue": true, "createdAt": "2024-03-01T10:00:00Z", "updatedAt": null }
          ],
          "nextCursor": null,
          "hasMore": false
        }
        """;
        var mock = new MockHttpMessageHandler(HttpStatusCode.OK, json);
        var client = TestClientFactory.Create(mock);

        var page = await client.Tasks.ListAsync(status: "open", overdue: true);

        Assert.That(page.Items, Has.Count.EqualTo(1));
        Assert.That(page.Items[0].Priority, Is.EqualTo("High"));
        Assert.That(page.Items[0].IsOverdue, Is.True);

        var query = mock.Requests[0].RequestUri!.Query;
        Assert.That(query, Does.Contain("status=open"));
        Assert.That(query, Does.Contain("overdue=true"));
    }

    [Test]
    public async Task CreateAsync_SerializesRequestBody()
    {
        const string responseJson = """
        { "id": 11, "title": "Follow up", "priority": "Medium", "status": "Open", "isOverdue": false, "createdAt": "2024-03-01T10:00:00Z" }
        """;
        var mock = new MockHttpMessageHandler(HttpStatusCode.Created, responseJson);
        var client = TestClientFactory.Create(mock);

        var task = await client.Tasks.CreateAsync(new CreateTaskRequest { Title = "Follow up", Priority = "medium" });

        Assert.That(task.Id, Is.EqualTo(11));
        var body = mock.RequestBodies[0];
        Assert.That(body, Does.Contain("\"title\":\"Follow up\""));
        Assert.That(body, Does.Contain("\"priority\":\"medium\""));
    }

    [Test]
    public async Task CompleteAsync_PostsDoneToStatusPath()
    {
        const string responseJson = """
        { "id": 11, "title": "Follow up", "priority": "Medium", "status": "Done", "isOverdue": false, "completedAt": "2024-03-02T10:00:00Z", "createdAt": "2024-03-01T10:00:00Z" }
        """;
        var mock = new MockHttpMessageHandler(HttpStatusCode.OK, responseJson);
        var client = TestClientFactory.Create(mock);

        var task = await client.Tasks.CompleteAsync(11);

        Assert.That(task.Status, Is.EqualTo("Done"));
        Assert.That(mock.Requests[0].RequestUri!.AbsolutePath, Is.EqualTo("/v1/tasks/11/status"));
        Assert.That(mock.RequestBodies[0], Does.Contain("\"status\":\"done\""));
    }
}
