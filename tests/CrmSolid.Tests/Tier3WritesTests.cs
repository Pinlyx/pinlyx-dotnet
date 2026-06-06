using System.Net;
using System.Threading.Tasks;
using CrmSolid.Models;
using NUnit.Framework;

namespace CrmSolid.Tests;

[TestFixture]
public class Tier3WritesTests
{
    [Test]
    public async Task Finance_CreateTransactionAsync_PostsBodyAndParses()
    {
        const string json = """
        { "id": 100, "type": "income", "amount": 250.0, "currency": "USD", "status": "completed", "occurredAt": "2024-03-01T10:00:00Z", "description": "Consulting" }
        """;
        var mock = new MockHttpMessageHandler(HttpStatusCode.OK, json);
        var client = TestClientFactory.Create(mock);

        var tx = await client.Finance.CreateTransactionAsync(new CreateTransactionRequest { Type = "income", Amount = 250m, Currency = "USD" });

        Assert.That(tx.Id, Is.EqualTo(100));
        Assert.That(tx.Type, Is.EqualTo("income"));
        Assert.That(mock.Requests[0].RequestUri!.AbsolutePath, Is.EqualTo("/v1/finance/transactions"));
        Assert.That(mock.RequestBodies[0], Does.Contain("\"type\":\"income\""));
    }

    [Test]
    public async Task Finance_MarkInvoicePaidAsync_PostsToPayPath()
    {
        const string json = """
        { "id": 5, "invoiceNumber": "INV-005", "status": "paid", "currency": "USD", "total": 999.0, "issueDate": "2024-02-01T00:00:00Z", "paidAt": "2024-03-01T10:00:00Z" }
        """;
        var mock = new MockHttpMessageHandler(HttpStatusCode.OK, json);
        var client = TestClientFactory.Create(mock);

        var inv = await client.Finance.MarkInvoicePaidAsync(5);

        Assert.That(inv.Status, Is.EqualTo("paid"));
        Assert.That(mock.Requests[0].RequestUri!.AbsolutePath, Is.EqualTo("/v1/finance/invoices/5/pay"));
    }

    [Test]
    public async Task Email_ReplyAsync_PostsBodyAndParsesResult()
    {
        const string json = """{ "messageId": 77, "threadId": 12, "messageIdHeader": "<out-abc@example.com>" }""";
        var mock = new MockHttpMessageHandler(HttpStatusCode.OK, json);
        var client = TestClientFactory.Create(mock);

        var result = await client.Email.ReplyAsync(12, new EmailReplyRequest { BodyText = "Thanks!" });

        Assert.That(result.ThreadId, Is.EqualTo(12));
        Assert.That(mock.Requests[0].RequestUri!.AbsolutePath, Is.EqualTo("/v1/email/threads/12/reply"));
        Assert.That(mock.RequestBodies[0], Does.Contain("Thanks!"));
    }

    [Test]
    public async Task Deals_CloseAsync_PostsToClosePath()
    {
        const string json = """
        { "id": 3, "title": "Big deal", "value": 5000.0, "currency": "USD", "stage": "Won", "probability": 100, "openTaskCount": 0, "createdAt": "2024-03-01T10:00:00Z" }
        """;
        var mock = new MockHttpMessageHandler(HttpStatusCode.OK, json);
        var client = TestClientFactory.Create(mock);

        var deal = await client.Deals.CloseAsync(3);

        Assert.That(deal.Stage, Is.EqualTo("Won"));
        Assert.That(mock.Requests[0].RequestUri!.AbsolutePath, Is.EqualTo("/v1/deals/3/close"));
    }

    [Test]
    public async Task ApiKeys_CreateAsync_ReturnsTokenOnce()
    {
        const string json = """
        { "id": 9, "keyId": "abc123def456", "token": "csk_live_abc123def456SECRET", "prefix": "csk_live_abc123de", "envLabel": "live", "scopes": "contacts:read deals:read", "rateLimitPerMinute": 60, "expiresAt": null }
        """;
        var mock = new MockHttpMessageHandler(HttpStatusCode.OK, json);
        var client = TestClientFactory.Create(mock);

        var created = await client.ApiKeys.CreateAsync(new CreateApiKeyRequest { Name = "ci", Scopes = "contacts:read deals:read" });

        Assert.That(created.Token, Does.StartWith("csk_live_"));
        Assert.That(created.Scopes, Does.Contain("contacts:read"));
        Assert.That(mock.Requests[0].RequestUri!.AbsolutePath, Is.EqualTo("/v1/api-keys"));
    }
}
