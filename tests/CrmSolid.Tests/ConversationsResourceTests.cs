using System.Net;
using System.Threading.Tasks;
using NUnit.Framework;

namespace CrmSolid.Tests;

[TestFixture]
public class ConversationsResourceTests
{
    [Test]
    public async Task ListRecentAsync_AppliesUnreadFilterAndParsesItems()
    {
        const string json = """
        { "items": [ { "contactId": 5, "platform": "telegram", "name": "Bob", "username": "bob", "lastMessageAt": "2024-03-01T10:00:00Z", "hasUnread": true, "preview": "hi", "stage": "Lead" } ] }
        """;
        var mock = new MockHttpMessageHandler(HttpStatusCode.OK, json);
        var client = TestClientFactory.Create(mock);

        var items = await client.Conversations.ListRecentAsync(unreadOnly: true);

        Assert.That(items, Has.Count.EqualTo(1));
        Assert.That(items[0].HasUnread, Is.True);
        Assert.That(mock.Requests[0].RequestUri!.Query, Does.Contain("unreadOnly=true"));
    }

    [Test]
    public async Task GetThreadAsync_ParsesMessagesAndCursor()
    {
        const string json = """
        { "items": [ { "id": 9, "direction": "incoming", "text": "hey", "at": "2024-03-01T10:00:00Z", "accountId": 2, "xAccountId": null, "mediaType": null, "mediaUrl": null } ], "nextBefore": 9 }
        """;
        var mock = new MockHttpMessageHandler(HttpStatusCode.OK, json);
        var client = TestClientFactory.Create(mock);

        var thread = await client.Conversations.GetThreadAsync(5, before: 20);

        Assert.That(thread.Items, Has.Count.EqualTo(1));
        Assert.That(thread.Items[0].Direction, Is.EqualTo("incoming"));
        Assert.That(thread.NextBefore, Is.EqualTo(9));
        Assert.That(mock.Requests[0].RequestUri!.AbsolutePath, Is.EqualTo("/v1/conversations/5"));
        Assert.That(mock.Requests[0].RequestUri!.Query, Does.Contain("before=20"));
    }
}
