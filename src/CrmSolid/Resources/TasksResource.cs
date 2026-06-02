using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using CrmSolid.Http;
using CrmSolid.Models;

namespace CrmSolid.Resources;

/// <summary>CRM tasks / reminders. Backed by <c>/v1/tasks</c>.</summary>
public sealed class TasksResource
{
    private readonly CrmSolidHttpClient _http;

    internal TasksResource(CrmSolidHttpClient http) { _http = http; }

    internal sealed record StatusBody(string Status);

    /// <summary>Cursor-paginated tasks.</summary>
    public Task<TaskList> ListAsync(
        int? after = null,
        int limit = 25,
        string? status = null,
        string? priority = null,
        bool? overdue = null,
        int? contactId = null,
        int? dealId = null,
        CancellationToken cancellationToken = default)
    {
        var qs = new QueryStringBuilder();
        if (after.HasValue) qs.Add("after", after.Value.ToString(CultureInfo.InvariantCulture));
        if (limit != 25) qs.Add("limit", limit.ToString(CultureInfo.InvariantCulture));
        if (!string.IsNullOrEmpty(status)) qs.Add("status", status!);
        if (!string.IsNullOrEmpty(priority)) qs.Add("priority", priority!);
        if (overdue.HasValue) qs.Add("overdue", overdue.Value ? "true" : "false");
        if (contactId.HasValue) qs.Add("contactId", contactId.Value.ToString(CultureInfo.InvariantCulture));
        if (dealId.HasValue) qs.Add("dealId", dealId.Value.ToString(CultureInfo.InvariantCulture));
        return _http.GetAsync<TaskList>("v1/tasks" + qs, cancellationToken);
    }

    public Task<CrmTask> GetAsync(int id, CancellationToken cancellationToken = default)
        => _http.GetAsync<CrmTask>("v1/tasks/" + id.ToString(CultureInfo.InvariantCulture), cancellationToken);

    public Task<CrmTask> CreateAsync(CreateTaskRequest request, CancellationToken cancellationToken = default)
    {
        if (request is null) throw new ArgumentNullException(nameof(request));
        return _http.PostJsonAsync<CreateTaskRequest, CrmTask>("v1/tasks", request, cancellationToken);
    }

    /// <summary>Sets a task's status (open/inprogress/done). Done stamps the completion time.</summary>
    public Task<CrmTask> ChangeStatusAsync(int id, string status, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(status)) throw new ArgumentException("status is required", nameof(status));
        return _http.PostJsonAsync<StatusBody, CrmTask>(
            "v1/tasks/" + id.ToString(CultureInfo.InvariantCulture) + "/status",
            new StatusBody(status),
            cancellationToken);
    }

    /// <summary>Convenience: mark a task done.</summary>
    public Task<CrmTask> CompleteAsync(int id, CancellationToken cancellationToken = default)
        => ChangeStatusAsync(id, "done", cancellationToken);

    /// <summary>Streams every task across all pages using cursor pagination.</summary>
    public async IAsyncEnumerable<CrmTask> StreamAllAsync(
        int pageSize = 100,
        string? status = null,
        string? priority = null,
        bool? overdue = null,
        int? contactId = null,
        int? dealId = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        int? cursor = null;
        while (true)
        {
            var page = await ListAsync(cursor, pageSize, status, priority, overdue, contactId, dealId, cancellationToken).ConfigureAwait(false);
            foreach (var task in page.Items) yield return task;
            if (!page.HasMore || page.NextCursor is null) yield break;
            cursor = page.NextCursor;
        }
    }
}
