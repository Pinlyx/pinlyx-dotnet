using System.Net;
using System.Threading.Tasks;
using NUnit.Framework;

namespace CrmSolid.Tests;

[TestFixture]
public class PipelinesResourceTests
{
    [Test]
    public async Task ListAsync_ParsesBoardsAndStages()
    {
        const string json = """
        { "items": [ { "id": 1, "name": "Sales", "description": "d", "icon": "BriefcaseIcon", "color": "#6366F1", "sortOrder": 0, "isDefault": true, "contactCount": 3, "stages": [ { "id": 10, "pipelineId": 1, "name": "Lead", "color": "#3B82F6", "description": null, "sortOrder": 1, "kind": "Open", "contactCount": 3 } ] } ] }
        """;
        var mock = new MockHttpMessageHandler(HttpStatusCode.OK, json);
        var client = TestClientFactory.Create(mock);

        var items = await client.Pipelines.ListAsync();

        Assert.That(items, Has.Count.EqualTo(1));
        Assert.That(items[0].IsDefault, Is.True);
        Assert.That(items[0].Stages, Has.Count.EqualTo(1));
        Assert.That(items[0].Stages[0].Kind, Is.EqualTo("Open"));
        Assert.That(mock.Requests[0].RequestUri!.AbsolutePath, Is.EqualTo("/v1/pipelines"));
    }
}
