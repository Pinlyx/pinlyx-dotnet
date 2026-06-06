using System.Net;
using System.Threading.Tasks;
using NUnit.Framework;

namespace CrmSolid.Tests;

[TestFixture]
public class ContactsStageTests
{
    [Test]
    public async Task SetStageAsync_PostsStageToCorrectPath()
    {
        const string json = """
        { "id": 5, "platform": "telegram", "name": "Bob", "stage": "Won", "leadScoreIsAi": false, "createdAt": "2024-03-01T10:00:00Z", "hasUnreadMessages": false }
        """;
        var mock = new MockHttpMessageHandler(HttpStatusCode.OK, json);
        var client = TestClientFactory.Create(mock);

        var contact = await client.Contacts.SetStageAsync(5, "won");

        Assert.That(contact.Stage, Is.EqualTo("Won"));
        Assert.That(mock.Requests[0].RequestUri!.AbsolutePath, Is.EqualTo("/v1/contacts/5/stage"));
        Assert.That(mock.RequestBodies[0], Does.Contain("\"stage\":\"won\""));
    }
}
