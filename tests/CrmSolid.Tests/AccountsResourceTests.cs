using System.Net;
using System.Threading.Tasks;
using NUnit.Framework;

namespace CrmSolid.Tests;

[TestFixture]
public class AccountsResourceTests
{
    [Test]
    public async Task ListAsync_ParsesItemsAndCounts()
    {
        const string json = """
        { "items": [ { "id": 1, "type": "telegram", "name": "Acct", "phone": "+1555", "status": "active", "lastUsedAt": "2024-03-01T10:00:00Z", "createdAt": "2024-02-01T10:00:00Z" } ], "telegram": 1, "twitter": 0 }
        """;
        var mock = new MockHttpMessageHandler(HttpStatusCode.OK, json);
        var client = TestClientFactory.Create(mock);

        var list = await client.Accounts.ListAsync(type: "telegram");

        Assert.That(list.Items, Has.Count.EqualTo(1));
        Assert.That(list.Telegram, Is.EqualTo(1));
        Assert.That(list.Items[0].Type, Is.EqualTo("telegram"));
        Assert.That(mock.Requests[0].RequestUri!.Query, Does.Contain("type=telegram"));
    }
}
