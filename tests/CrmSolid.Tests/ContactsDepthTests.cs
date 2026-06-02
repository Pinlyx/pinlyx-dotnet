using System.Net;
using System.Threading.Tasks;
using NUnit.Framework;

namespace CrmSolid.Tests;

/// <summary>Covers the CRM core-depth methods on ContactsResource (tags, score, assignment, activity).</summary>
[TestFixture]
public class ContactsDepthTests
{
    [Test]
    public async Task AddTagAsync_PostsBodyAndReturnsTags()
    {
        const string json = """
        { "items": [ { "id": 3, "name": "VIP", "color": "#2563EB" } ] }
        """;
        var mock = new MockHttpMessageHandler(HttpStatusCode.OK, json);
        var client = TestClientFactory.Create(mock);

        var tags = await client.Contacts.AddTagAsync(contactId: 101, tagName: "VIP");

        Assert.That(tags, Has.Count.EqualTo(1));
        Assert.That(tags[0].Name, Is.EqualTo("VIP"));
        Assert.That(mock.Requests[0].RequestUri!.AbsolutePath, Is.EqualTo("/v1/contacts/101/tags"));
        Assert.That(mock.RequestBodies[0], Does.Contain("\"tagName\":\"VIP\""));
    }

    [Test]
    public async Task RemoveTagAsync_SendsDeleteToCorrectPath()
    {
        const string json = """{ "items": [] }""";
        var mock = new MockHttpMessageHandler(HttpStatusCode.OK, json);
        var client = TestClientFactory.Create(mock);

        var tags = await client.Contacts.RemoveTagAsync(contactId: 101, tagId: 3);

        Assert.That(tags, Is.Empty);
        Assert.That(mock.Requests[0].Method, Is.EqualTo(System.Net.Http.HttpMethod.Delete));
        Assert.That(mock.Requests[0].RequestUri!.AbsolutePath, Is.EqualTo("/v1/contacts/101/tags/3"));
    }

    [Test]
    public async Task SetLeadScoreAsync_PutsScore()
    {
        const string json = """
        { "id": 101, "platform": "telegram", "stage": "Lead", "leadScore": 85, "leadScoreIsAi": false, "createdAt": "2024-03-01T10:00:00Z", "hasUnreadMessages": false }
        """;
        var mock = new MockHttpMessageHandler(HttpStatusCode.OK, json);
        var client = TestClientFactory.Create(mock);

        var contact = await client.Contacts.SetLeadScoreAsync(101, 85);

        Assert.That(contact.LeadScore, Is.EqualTo(85));
        Assert.That(contact.LeadScoreIsAi, Is.False);
        Assert.That(mock.Requests[0].Method, Is.EqualTo(System.Net.Http.HttpMethod.Put));
        Assert.That(mock.Requests[0].RequestUri!.AbsolutePath, Is.EqualTo("/v1/contacts/101/score"));
        Assert.That(mock.RequestBodies[0], Does.Contain("\"score\":85"));
    }

    [Test]
    public async Task ListActivitiesAsync_ParsesTimeline()
    {
        const string json = """
        { "items": [ { "id": 50, "type": "TagAdded", "body": "Added tag 'VIP'", "createdAt": "2024-03-01T10:00:00Z" } ] }
        """;
        var mock = new MockHttpMessageHandler(HttpStatusCode.OK, json);
        var client = TestClientFactory.Create(mock);

        var activities = await client.Contacts.ListActivitiesAsync(101);

        Assert.That(activities, Has.Count.EqualTo(1));
        Assert.That(activities[0].Type, Is.EqualTo("TagAdded"));
        Assert.That(mock.Requests[0].RequestUri!.AbsolutePath, Is.EqualTo("/v1/contacts/101/activities"));
    }
}
