using System.Net;
using System.Threading.Tasks;
using NUnit.Framework;

namespace CrmSolid.Tests;

[TestFixture]
public class FinanceResourceTests
{
    [Test]
    public async Task GetSummaryAsync_ParsesTotals()
    {
        const string json = """
        {
          "range": "30d",
          "from": "2024-02-01T00:00:00Z",
          "to": "2024-03-02T00:00:00Z",
          "currency": null,
          "totals": [ { "currency": "USD", "income": 5000.0, "expense": 1200.0, "net": 3800.0 } ],
          "topExpenseCategories": [ { "categoryId": 4, "categoryName": "Ads", "total": 800.0 } ],
          "outstanding": [ { "currency": "USD", "incomePending": 300.0, "expensePending": 0.0 } ]
        }
        """;
        var mock = new MockHttpMessageHandler(HttpStatusCode.OK, json);
        var client = TestClientFactory.Create(mock);

        var summary = await client.Finance.GetSummaryAsync(range: "30d");

        Assert.That(summary.Totals, Has.Count.EqualTo(1));
        Assert.That(summary.Totals[0].Net, Is.EqualTo(3800.0m));
        Assert.That(summary.TopExpenseCategories[0].CategoryName, Is.EqualTo("Ads"));
        Assert.That(summary.Outstanding[0].IncomePending, Is.EqualTo(300.0m));
    }

    [Test]
    public async Task ListTransactionsAsync_ParsesAndFilters()
    {
        const string json = """
        {
          "items": [
            { "id": 9, "type": "income", "amount": 250.0, "currency": "USD", "status": "completed", "occurredAt": "2024-03-01T10:00:00Z", "description": "Sale", "categoryId": null, "contactId": 5, "productName": "Plan", "fee": 10.0, "net": 240.0, "sourceLabel": "TakiPlus", "invoiceId": null }
          ],
          "nextCursor": 9,
          "hasMore": false
        }
        """;
        var mock = new MockHttpMessageHandler(HttpStatusCode.OK, json);
        var client = TestClientFactory.Create(mock);

        var page = await client.Finance.ListTransactionsAsync(type: "income", status: "completed");

        Assert.That(page.Items[0].Type, Is.EqualTo("income"));
        Assert.That(page.Items[0].Net, Is.EqualTo(240.0m));
        var query = mock.Requests[0].RequestUri!.Query;
        Assert.That(query, Does.Contain("type=income"));
        Assert.That(query, Does.Contain("status=completed"));
    }

    [Test]
    public async Task ListRevenueSourcesAsync_ParsesEnvelope()
    {
        const string json = """
        {
          "items": [
            { "id": 1, "name": "TakiPlus", "mode": "push", "defaultCurrency": "USD", "isActive": true, "feedUrl": null, "pullIntervalMinutes": null, "lastSyncStatus": "ok", "lastSyncAt": "2024-03-01T10:00:00Z", "lastSyncError": null, "totalIngested": 1234, "lastEventAt": "2024-03-01T09:00:00Z", "nextRunAt": null }
          ],
          "count": 1,
          "totalIngested": 1234
        }
        """;
        var mock = new MockHttpMessageHandler(HttpStatusCode.OK, json);
        var client = TestClientFactory.Create(mock);

        var result = await client.Finance.ListRevenueSourcesAsync();

        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result.TotalIngested, Is.EqualTo(1234));
        Assert.That(result.Items[0].Mode, Is.EqualTo("push"));
        Assert.That(mock.Requests[0].RequestUri!.AbsolutePath, Is.EqualTo("/v1/finance/revenue-sources"));
    }
}
