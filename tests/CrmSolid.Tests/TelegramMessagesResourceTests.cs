using System;
using System.Net;
using System.Threading.Tasks;
using CrmSolid.Models;
using NUnit.Framework;

namespace CrmSolid.Tests;

[TestFixture]
public class TelegramMessagesResourceTests
{
    [Test]
    public async Task SendAsync_PostsToCorrectPathAndReturnsJob()
    {
        const string json = """
        { "id": 555, "accountId": 7, "status": "queued", "runAt": "2024-03-01T10:00:00Z", "createdAt": "2024-03-01T10:00:00Z" }
        """;
        var mock = new MockHttpMessageHandler(HttpStatusCode.Accepted, json);
        var client = TestClientFactory.Create(mock);

        var job = await client.TelegramMessages.SendAsync(new SendMessageRequest
        {
            AccountId = 7,
            Username = "john_doe",
            Text = "Hello!",
        });

        Assert.That(job.Id, Is.EqualTo(555));
        Assert.That(job.AccountId, Is.EqualTo(7));
        Assert.That(job.Status, Is.EqualTo(JobStatus.Queued));

        Assert.That(mock.Requests[0].RequestUri!.AbsolutePath, Is.EqualTo("/v1/telegram/messages"));
        Assert.That(mock.RequestBodies[0], Does.Contain("\"accountId\":7"));
        Assert.That(mock.RequestBodies[0], Does.Contain("\"username\":\"john_doe\""));
        Assert.That(mock.RequestBodies[0], Does.Contain("\"text\":\"Hello!\""));
    }

    [Test]
    public async Task GetAsync_DeserializesJobDetail()
    {
        const string json = """
        { "id": 555, "accountId": 7, "status": "failed", "runAt": "2024-03-01T10:00:00Z", "createdAt": "2024-03-01T10:00:00Z", "updatedAt": "2024-03-01T10:05:00Z", "lastError": "FLOOD_WAIT_60" }
        """;
        var mock = new MockHttpMessageHandler(HttpStatusCode.OK, json);
        var client = TestClientFactory.Create(mock);

        var detail = await client.TelegramMessages.GetAsync(555);

        Assert.That(detail.Status, Is.EqualTo(JobStatus.Failed));
        Assert.That(detail.LastError, Is.EqualTo("FLOOD_WAIT_60"));
        Assert.That(detail.UpdatedAt, Is.Not.Null);
    }
}
