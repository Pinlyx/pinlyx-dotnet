using System.Net;
using System.Threading.Tasks;
using NUnit.Framework;

namespace CrmSolid.Tests;

[TestFixture]
public class EmailResourceTests
{
    [Test]
    public async Task ListThreadsAsync_ParsesAndFilters()
    {
        const string json = """
        {
          "items": [
            { "id": 12, "subject": "Invoice question", "preview": "Hi, about...", "status": "open", "unreadCount": 1, "isStarred": false, "contactId": 5, "assignedToUserId": null, "aiLeadScore": 70, "lastMessageAt": "2024-03-01T10:00:00Z" }
          ],
          "nextCursor": null,
          "hasMore": false
        }
        """;
        var mock = new MockHttpMessageHandler(HttpStatusCode.OK, json);
        var client = TestClientFactory.Create(mock);

        var page = await client.Email.ListThreadsAsync(status: "open", unreadOnly: true);

        Assert.That(page.Items[0].Subject, Is.EqualTo("Invoice question"));
        Assert.That(page.Items[0].Status, Is.EqualTo("open"));
        var query = mock.Requests[0].RequestUri!.Query;
        Assert.That(query, Does.Contain("status=open"));
        Assert.That(query, Does.Contain("unreadOnly=true"));
    }

    [Test]
    public async Task GetThreadAsync_ParsesDetailWithMessages()
    {
        const string json = """
        {
          "id": 12, "subject": "Invoice question", "preview": "Hi", "status": "open", "unreadCount": 0, "isStarred": false, "contactId": 5, "assignedToUserId": null, "aiLeadScore": null, "lastMessageAt": "2024-03-01T10:00:00Z",
          "aiSummary": "Customer asks about invoice 42.",
          "messages": [
            { "id": 100, "direction": "inbound", "fromAddress": "jane@acme.com", "fromName": "Jane", "subject": "Invoice question", "body": "Where is invoice 42?", "isRead": true, "receivedAt": "2024-03-01T09:00:00Z" }
          ]
        }
        """;
        var mock = new MockHttpMessageHandler(HttpStatusCode.OK, json);
        var client = TestClientFactory.Create(mock);

        var thread = await client.Email.GetThreadAsync(12);

        Assert.That(thread.AiSummary, Is.EqualTo("Customer asks about invoice 42."));
        Assert.That(thread.Messages, Has.Count.EqualTo(1));
        Assert.That(thread.Messages[0].Direction, Is.EqualTo("inbound"));
        Assert.That(mock.Requests[0].RequestUri!.AbsolutePath, Is.EqualTo("/v1/email/threads/12"));
    }

    [Test]
    public async Task SetStatusAsync_PostsStatusToPath()
    {
        const string json = """
        { "id": 12, "subject": "Invoice question", "status": "closed", "unreadCount": 0, "isStarred": false, "lastMessageAt": "2024-03-01T10:00:00Z" }
        """;
        var mock = new MockHttpMessageHandler(HttpStatusCode.OK, json);
        var client = TestClientFactory.Create(mock);

        var thread = await client.Email.SetStatusAsync(12, "closed");

        Assert.That(thread.Status, Is.EqualTo("closed"));
        Assert.That(mock.Requests[0].RequestUri!.AbsolutePath, Is.EqualTo("/v1/email/threads/12/status"));
        Assert.That(mock.RequestBodies[0], Does.Contain("\"status\":\"closed\""));
    }

    [Test]
    public async Task AssignAsync_PostsUserIdToPath()
    {
        const string json = """
        { "id": 12, "subject": "Invoice question", "status": "open", "unreadCount": 0, "isStarred": false, "assignedToUserId": 8, "lastMessageAt": "2024-03-01T10:00:00Z" }
        """;
        var mock = new MockHttpMessageHandler(HttpStatusCode.OK, json);
        var client = TestClientFactory.Create(mock);

        var thread = await client.Email.AssignAsync(12, 8);

        Assert.That(thread.AssignedToUserId, Is.EqualTo(8));
        Assert.That(mock.Requests[0].RequestUri!.AbsolutePath, Is.EqualTo("/v1/email/threads/12/assignee"));
        Assert.That(mock.RequestBodies[0], Does.Contain("\"userId\":8"));
    }
}
