using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using CrmSolid.Http;
using CrmSolid.Models;

namespace CrmSolid.Resources;

/// <summary>
/// AI Agents. Backed by <c>/v1/ai-agents</c>. List/inspect with <c>agents:read</c>; run the
/// test playground with <c>agents:run</c>. <see cref="TestAsync"/> is a dry-run — it returns
/// the reply the agent WOULD send but never delivers it.
/// </summary>
public sealed class AiAgentsResource
{
    private readonly CrmSolidHttpClient _http;

    internal AiAgentsResource(CrmSolidHttpClient http) { _http = http; }

    /// <summary>Cursor-paginated agents (newest first).</summary>
    public Task<AiAgentList> ListAsync(int? after = null, int limit = 25, CancellationToken cancellationToken = default)
    {
        var qs = new QueryStringBuilder();
        if (after.HasValue) qs.Add("after", after.Value.ToString(CultureInfo.InvariantCulture));
        if (limit != 25) qs.Add("limit", limit.ToString(CultureInfo.InvariantCulture));
        return _http.GetAsync<AiAgentList>("v1/ai-agents" + qs, cancellationToken);
    }

    /// <summary>Returns a single agent.</summary>
    public Task<AiAgent> GetAsync(int id, CancellationToken cancellationToken = default)
        => _http.GetAsync<AiAgent>("v1/ai-agents/" + id.ToString(CultureInfo.InvariantCulture), cancellationToken);

    /// <summary>
    /// Test-runs the agent against a sample inbound message and returns the generated reply
    /// WITHOUT sending it. Requires <c>agents:run</c>.
    /// </summary>
    public Task<TestAgentResult> TestAsync(int id, TestAgentRequest request, CancellationToken cancellationToken = default)
    {
        if (request is null) throw new ArgumentNullException(nameof(request));
        return _http.PostJsonAsync<TestAgentRequest, TestAgentResult>(
            "v1/ai-agents/" + id.ToString(CultureInfo.InvariantCulture) + "/test", request, cancellationToken);
    }

    /// <summary>Streams every agent across pages using cursor pagination.</summary>
    public async IAsyncEnumerable<AiAgent> StreamAllAsync(
        int pageSize = 100,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        int? cursor = null;
        while (true)
        {
            var page = await ListAsync(cursor, pageSize, cancellationToken).ConfigureAwait(false);
            foreach (var agent in page.Items) yield return agent;
            if (!page.HasMore || page.NextCursor is null) yield break;
            cursor = page.NextCursor;
        }
    }
}
