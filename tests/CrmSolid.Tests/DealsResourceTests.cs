using System.Net;
using System.Threading.Tasks;
using CrmSolid.Models;
using NUnit.Framework;

namespace CrmSolid.Tests;

[TestFixture]
public class DealsResourceTests
{
    [Test]
    public async Task ListAsync_ParsesPageAndAppliesFilters()
    {
        const string json = """
        {
          "items": [
            { "id": 7, "title": "Big deal", "contactId": 5, "contactName": "Acme", "value": 1000.0, "currency": "USD", "stage": "Negotiation", "probability": 60, "expectedCloseAt": null, "closedAt": null, "notes": null, "openTaskCount": 2, "createdAt": "2024-03-01T10:00:00Z", "updatedAt": null }
          ],
          "nextCursor": 7,
          "hasMore": false
        }
        """;
        var mock = new MockHttpMessageHandler(HttpStatusCode.OK, json);
        var client = TestClientFactory.Create(mock);

        var page = await client.Deals.ListAsync(stage: "negotiation", contactId: 5);

        Assert.That(page.Items, Has.Count.EqualTo(1));
        Assert.That(page.Items[0].Stage, Is.EqualTo("Negotiation"));
        Assert.That(page.Items[0].Value, Is.EqualTo(1000.0m));
        Assert.That(page.Items[0].OpenTaskCount, Is.EqualTo(2));

        var query = mock.Requests[0].RequestUri!.Query;
        Assert.That(query, Does.Contain("stage=negotiation"));
        Assert.That(query, Does.Contain("contactId=5"));
    }

    [Test]
    public async Task CreateAsync_SerializesRequestBody()
    {
        const string responseJson = """
        { "id": 42, "title": "New deal", "value": 500.0, "currency": "USD", "stage": "Lead", "probability": 0, "openTaskCount": 0, "createdAt": "2024-03-01T10:00:00Z" }
        """;
        var mock = new MockHttpMessageHandler(HttpStatusCode.Created, responseJson);
        var client = TestClientFactory.Create(mock);

        var deal = await client.Deals.CreateAsync(new CreateDealRequest { Title = "New deal", Value = 500m, Stage = "lead" });

        Assert.That(deal.Id, Is.EqualTo(42));
        var body = mock.RequestBodies[0];
        Assert.That(body, Does.Contain("\"title\":\"New deal\""));
        Assert.That(body, Does.Contain("\"stage\":\"lead\""));
    }

    [Test]
    public async Task ChangeStageAsync_PostsStageToCorrectPath()
    {
        const string responseJson = """
        { "id": 7, "title": "Big deal", "value": 1000.0, "currency": "USD", "stage": "Qualified", "probability": 60, "openTaskCount": 0, "createdAt": "2024-03-01T10:00:00Z" }
        """;
        var mock = new MockHttpMessageHandler(HttpStatusCode.OK, responseJson);
        var client = TestClientFactory.Create(mock);

        var deal = await client.Deals.ChangeStageAsync(7, "qualified");

        Assert.That(deal.Stage, Is.EqualTo("Qualified"));
        Assert.That(mock.Requests[0].RequestUri!.AbsolutePath, Is.EqualTo("/v1/deals/7/stage"));
        Assert.That(mock.RequestBodies[0], Does.Contain("\"stage\":\"qualified\""));
    }
}
