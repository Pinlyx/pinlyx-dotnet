using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CrmSolid.Models;
using NUnit.Framework;

namespace CrmSolid.Tests;

[TestFixture]
public class ContactsResourceTests
{
    [Test]
    public async Task ListAsync_ParsesPageAndCursor()
    {
        const string json = """
        {
          "items": [
            { "id": 101, "platform": "telegram", "name": "Alice", "username": "alice", "phone": null, "notes": null, "stage": "Lead", "createdAt": "2024-03-01T10:00:00Z", "lastMessageAt": null, "hasUnreadMessages": false },
            { "id": 100, "platform": "twitter", "name": "Bob", "username": "bobx", "phone": null, "notes": null, "stage": "Customer", "createdAt": "2024-03-02T10:00:00Z", "lastMessageAt": "2024-03-04T10:00:00Z", "hasUnreadMessages": true }
          ],
          "nextCursor": 100,
          "hasMore": true
        }
        """;
        var mock = new MockHttpMessageHandler(HttpStatusCode.OK, json);
        var client = TestClientFactory.Create(mock);

        var page = await client.Contacts.ListAsync(limit: 50, platform: Platform.Telegram);

        Assert.That(page.Items, Has.Count.EqualTo(2));
        Assert.That(page.HasMore, Is.True);
        Assert.That(page.NextCursor, Is.EqualTo(100));

        var first = page.Items[0];
        Assert.That(first.Id, Is.EqualTo(101));
        Assert.That(first.Platform, Is.EqualTo(Platform.Telegram));
        Assert.That(first.HasUnreadMessages, Is.False);
        Assert.That(page.Items[1].Platform, Is.EqualTo(Platform.Twitter));

        // Verify query string
        var query = mock.Requests[0].RequestUri!.Query;
        Assert.That(query, Does.Contain("limit=50"));
        Assert.That(query, Does.Contain("platform=telegram"));
    }

    [Test]
    public async Task GetAsync_FetchesById()
    {
        const string json = """
        { "id": 101, "platform": "telegram", "name": "Alice", "username": "alice", "phone": null, "notes": null, "stage": "Lead", "createdAt": "2024-03-01T10:00:00Z", "lastMessageAt": null, "hasUnreadMessages": false }
        """;
        var mock = new MockHttpMessageHandler(HttpStatusCode.OK, json);
        var client = TestClientFactory.Create(mock);

        var contact = await client.Contacts.GetAsync(101);

        Assert.That(contact.Id, Is.EqualTo(101));
        Assert.That(mock.Requests[0].RequestUri!.AbsolutePath, Is.EqualTo("/v1/contacts/101"));
    }

    [Test]
    public async Task CreateAsync_SerializesRequestBodyAsCamelCase()
    {
        const string responseJson = """
        { "id": 999, "platform": "telegram", "name": "New", "username": "newone", "phone": null, "notes": "hi", "stage": "Lead", "createdAt": "2024-03-01T10:00:00Z", "lastMessageAt": null, "hasUnreadMessages": false }
        """;
        var mock = new MockHttpMessageHandler(HttpStatusCode.Created, responseJson);
        var client = TestClientFactory.Create(mock);

        var contact = await client.Contacts.CreateAsync(new CreateContactRequest
        {
            Platform = Platform.Telegram,
            Name = "New",
            Username = "newone",
            Notes = "hi"
        });

        Assert.That(contact.Id, Is.EqualTo(999));
        Assert.That(mock.RequestBodies, Has.Count.EqualTo(1));

        var body = mock.RequestBodies[0];
        Assert.That(body, Does.Contain("\"platform\":\"telegram\""));
        Assert.That(body, Does.Contain("\"name\":\"New\""));
        Assert.That(body, Does.Contain("\"username\":\"newone\""));
        Assert.That(body, Does.Contain("\"notes\":\"hi\""));
    }

    [Test]
    public async Task StreamAllAsync_WalksMultiplePages()
    {
        var page1 = """{"items":[{"id":3,"platform":"telegram","name":"a","username":null,"phone":null,"notes":null,"stage":"Lead","createdAt":"2024-03-01T10:00:00Z","lastMessageAt":null,"hasUnreadMessages":false},{"id":2,"platform":"telegram","name":"b","username":null,"phone":null,"notes":null,"stage":"Lead","createdAt":"2024-03-01T10:00:00Z","lastMessageAt":null,"hasUnreadMessages":false}],"nextCursor":2,"hasMore":true}""";
        var page2 = """{"items":[{"id":1,"platform":"telegram","name":"c","username":null,"phone":null,"notes":null,"stage":"Lead","createdAt":"2024-03-01T10:00:00Z","lastMessageAt":null,"hasUnreadMessages":false}],"nextCursor":null,"hasMore":false}""";

        var responses = new Queue<string>(new[] { page1, page2 });
        var mock = new MockHttpMessageHandler(_ =>
            Task.FromResult(MockHttpMessageHandler.BuildResponse(HttpStatusCode.OK, responses.Dequeue())));
        var client = TestClientFactory.Create(mock);

        var ids = new List<int>();
        await foreach (var c in client.Contacts.StreamAllAsync(pageSize: 2))
            ids.Add(c.Id);

        Assert.That(ids, Is.EqualTo(new[] { 3, 2, 1 }));
        Assert.That(mock.Requests, Has.Count.EqualTo(2));

        var second = mock.Requests[1].RequestUri!.Query;
        Assert.That(second, Does.Contain("after=2"));
    }
}
