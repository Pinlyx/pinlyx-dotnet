using System.Net;
using System.Threading.Tasks;
using NUnit.Framework;

namespace CrmSolid.Tests;

[TestFixture]
public class JobsResourceTests
{
    [Test]
    public async Task ListAsync_AppliesFiltersAndParsesPage()
    {
        const string json = """
        { "items": [ { "id": 9, "accountId": 2, "status": "sent", "targetId": 12345, "targetUsername": "bob", "text": "hi", "runAt": "2024-03-01T10:00:00Z", "createdAt": "2024-03-01T09:59:00Z", "updatedAt": "2024-03-01T10:00:05Z", "lastError": null } ], "nextCursor": 9, "hasMore": false }
        """;
        var mock = new MockHttpMessageHandler(HttpStatusCode.OK, json);
        var client = TestClientFactory.Create(mock);

        var page = await client.Jobs.ListAsync(status: "sent", accountId: 2);

        Assert.That(page.Items, Has.Count.EqualTo(1));
        Assert.That(page.Items[0].Status, Is.EqualTo("sent"));
        Assert.That(page.Items[0].TargetId, Is.EqualTo(12345));
        var query = mock.Requests[0].RequestUri!.Query;
        Assert.That(query, Does.Contain("status=sent"));
        Assert.That(query, Does.Contain("accountId=2"));
    }
}
