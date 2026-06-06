using System.Net;
using System.Threading.Tasks;
using CrmSolid.Models;
using NUnit.Framework;

namespace CrmSolid.Tests;

[TestFixture]
public class TwitterMessagesResourceTests
{
    [Test]
    public async Task SendAsync_PostsBodyAndParsesResult()
    {
        const string json = """
        { "status": "sent", "xDmId": 10, "messageId": "m-1", "text": "hi there", "sentAt": "2024-03-01T10:00:00Z" }
        """;
        var mock = new MockHttpMessageHandler(HttpStatusCode.OK, json);
        var client = TestClientFactory.Create(mock);

        var result = await client.TwitterMessages.SendAsync(new SendDmRequest { XAccountId = 3, ContactId = 5, Text = "hi there" });

        Assert.That(result.Status, Is.EqualTo("sent"));
        Assert.That(result.XDmId, Is.EqualTo(10));
        Assert.That(mock.Requests[0].RequestUri!.AbsolutePath, Is.EqualTo("/v1/twitter/messages"));
        var body = mock.RequestBodies[0];
        Assert.That(body, Does.Contain("\"xAccountId\":3"));
        Assert.That(body, Does.Contain("\"contactId\":5"));
    }

    [Test]
    public async Task SearchAsync_AppliesQueryAndParsesItems()
    {
        const string json = """
        { "items": [ { "id": 1, "xAccountId": 3, "contactId": 5, "contactName": "Bob", "contactUsername": "bob", "text": "hey", "at": "2024-03-01T10:00:00Z", "senderId": "s1", "recipientId": "r1" } ] }
        """;
        var mock = new MockHttpMessageHandler(HttpStatusCode.OK, json);
        var client = TestClientFactory.Create(mock);

        var items = await client.TwitterMessages.SearchAsync("hey", contactId: 5);

        Assert.That(items, Has.Count.EqualTo(1));
        Assert.That(items[0].Text, Is.EqualTo("hey"));
        var query = mock.Requests[0].RequestUri!.Query;
        Assert.That(query, Does.Contain("q=hey"));
        Assert.That(query, Does.Contain("contactId=5"));
    }
}
