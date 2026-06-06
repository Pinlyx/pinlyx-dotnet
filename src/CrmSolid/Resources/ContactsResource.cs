using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using CrmSolid.Http;
using CrmSolid.Models;

namespace CrmSolid.Resources;

/// <summary>CRM contacts CRUD. Backed by <c>/v1/contacts</c>.</summary>
public sealed class ContactsResource
{
    private readonly CrmSolidHttpClient _http;

    internal ContactsResource(CrmSolidHttpClient http)
    {
        _http = http;
    }

    /// <summary>
    /// Returns a cursor-paginated page of contacts. Pass <see cref="ContactList.NextCursor"/>
    /// as <paramref name="after"/> on the next call to advance pagination.
    /// </summary>
    /// <param name="after">Return contacts with an id strictly less than this value.</param>
    /// <param name="limit">Page size, clamped server-side to 1–100. Default: 25.</param>
    /// <param name="platform">Filter by platform.</param>
    /// <param name="query">Case-insensitive substring search across name, username, and phone.</param>
    /// <param name="cancellationToken">Token to cancel the request.</param>
    public Task<ContactList> ListAsync(
        int? after = null,
        int limit = 25,
        Platform? platform = null,
        string? query = null,
        CancellationToken cancellationToken = default)
    {
        var qs = new QueryStringBuilder();
        if (after.HasValue)
            qs.Add("after", after.Value.ToString(CultureInfo.InvariantCulture));
        if (limit != 25)
            qs.Add("limit", limit.ToString(CultureInfo.InvariantCulture));
        if (platform.HasValue)
            qs.Add("platform", platform.Value.ToString().ToLowerInvariant());
        if (!string.IsNullOrEmpty(query))
            qs.Add("q", query!);

        return _http.GetAsync<ContactList>("v1/contacts" + qs, cancellationToken);
    }

    /// <summary>Returns a single contact owned by the authenticated user.</summary>
    public Task<Contact> GetAsync(int id, CancellationToken cancellationToken = default)
        => _http.GetAsync<Contact>(
            "v1/contacts/" + id.ToString(CultureInfo.InvariantCulture),
            cancellationToken);

    /// <summary>Creates a new contact. At least one of name, username, or phone is required.</summary>
    public Task<Contact> CreateAsync(
        CreateContactRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request is null) throw new ArgumentNullException(nameof(request));
        return _http.PostJsonAsync<CreateContactRequest, Contact>(
            "v1/contacts",
            request,
            cancellationToken);
    }

    /// <summary>
    /// Streams every contact across all pages using cursor pagination. Pages are
    /// fetched on demand; cancellation propagates immediately.
    /// </summary>
    /// <param name="pageSize">Per-page size (1–100). Default: 100.</param>
    /// <param name="platform">Optional platform filter applied to every page.</param>
    /// <param name="query">Optional substring search applied to every page.</param>
    /// <param name="cancellationToken">Token to cancel the iteration.</param>
    public async IAsyncEnumerable<Contact> StreamAllAsync(
        int pageSize = 100,
        Platform? platform = null,
        string? query = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        int? cursor = null;
        while (true)
        {
            var page = await ListAsync(cursor, pageSize, platform, query, cancellationToken)
                .ConfigureAwait(false);

            foreach (var contact in page.Items)
                yield return contact;

            if (!page.HasMore || page.NextCursor is null)
                yield break;

            cursor = page.NextCursor;
        }
    }

    // ===================== CRM core depth: tags / lead score / assignment / activity =====================

    internal sealed record AddTagBody(int? TagId, string? TagName);
    internal sealed record ScoreBody(int Score);
    internal sealed record AssignBody(int? UserId);
    internal sealed record ActivityBody(string Body);
    internal sealed record StageBody(string Stage);

    /// <summary>Lists the user's tag dictionary with per-tag contact counts (<c>GET /v1/tags</c>).</summary>
    public async Task<IReadOnlyList<TagWithCount>> ListTagDictionaryAsync(CancellationToken cancellationToken = default)
    {
        var res = await _http.GetAsync<ItemsResponse<TagWithCount>>("v1/tags", cancellationToken).ConfigureAwait(false);
        return res.Items;
    }

    /// <summary>Lists the tags attached to a contact.</summary>
    public async Task<IReadOnlyList<Tag>> GetTagsAsync(int contactId, CancellationToken cancellationToken = default)
    {
        var res = await _http.GetAsync<ItemsResponse<Tag>>(
            "v1/contacts/" + contactId.ToString(CultureInfo.InvariantCulture) + "/tags", cancellationToken).ConfigureAwait(false);
        return res.Items;
    }

    /// <summary>
    /// Attaches a tag to a contact. Provide <paramref name="tagId"/>, or <paramref name="tagName"/>
    /// (created if it doesn't exist yet). Idempotent. Returns the contact's full tag list.
    /// </summary>
    public async Task<IReadOnlyList<Tag>> AddTagAsync(int contactId, int? tagId = null, string? tagName = null, CancellationToken cancellationToken = default)
    {
        if (tagId is null && string.IsNullOrEmpty(tagName))
            throw new ArgumentException("either tagId or tagName is required");
        var res = await _http.PostJsonAsync<AddTagBody, ItemsResponse<Tag>>(
            "v1/contacts/" + contactId.ToString(CultureInfo.InvariantCulture) + "/tags",
            new AddTagBody(tagId, tagName), cancellationToken).ConfigureAwait(false);
        return res.Items;
    }

    /// <summary>Detaches a tag from a contact. Returns the contact's remaining tag list.</summary>
    public async Task<IReadOnlyList<Tag>> RemoveTagAsync(int contactId, int tagId, CancellationToken cancellationToken = default)
    {
        var res = await _http.DeleteAsync<ItemsResponse<Tag>>(
            "v1/contacts/" + contactId.ToString(CultureInfo.InvariantCulture) + "/tags/" + tagId.ToString(CultureInfo.InvariantCulture),
            cancellationToken).ConfigureAwait(false);
        return res.Items;
    }

    /// <summary>Manually sets a contact's lead score (0-100). Marks it a manual (non-AI) override.</summary>
    public Task<Contact> SetLeadScoreAsync(int contactId, int score, CancellationToken cancellationToken = default)
    {
        if (score < 0 || score > 100) throw new ArgumentOutOfRangeException(nameof(score), "score must be between 0 and 100");
        return _http.PutJsonAsync<ScoreBody, Contact>(
            "v1/contacts/" + contactId.ToString(CultureInfo.InvariantCulture) + "/score",
            new ScoreBody(score), cancellationToken);
    }

    /// <summary>Assigns a contact to a team member, or unassigns when <paramref name="userId"/> is null.</summary>
    public Task<Contact> AssignAsync(int contactId, int? userId, CancellationToken cancellationToken = default)
        => _http.PutJsonAsync<AssignBody, Contact>(
            "v1/contacts/" + contactId.ToString(CultureInfo.InvariantCulture) + "/assignee",
            new AssignBody(userId), cancellationToken);

    /// <summary>
    /// Moves a contact along its sales pipeline. Accepts
    /// <c>novalue|lead|conversation|proposal|negotiation|won|lost</c>. Logs a StageChanged activity.
    /// </summary>
    public Task<Contact> SetStageAsync(int contactId, string stage, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(stage)) throw new ArgumentException("stage is required", nameof(stage));
        return _http.PostJsonAsync<StageBody, Contact>(
            "v1/contacts/" + contactId.ToString(CultureInfo.InvariantCulture) + "/stage",
            new StageBody(stage), cancellationToken);
    }

    /// <summary>Returns a contact's activity timeline (newest first).</summary>
    public async Task<IReadOnlyList<ContactActivity>> ListActivitiesAsync(int contactId, int limit = 50, CancellationToken cancellationToken = default)
    {
        var qs = new QueryStringBuilder();
        if (limit != 50) qs.Add("limit", limit.ToString(CultureInfo.InvariantCulture));
        var res = await _http.GetAsync<ItemsResponse<ContactActivity>>(
            "v1/contacts/" + contactId.ToString(CultureInfo.InvariantCulture) + "/activities" + qs, cancellationToken).ConfigureAwait(false);
        return res.Items;
    }

    /// <summary>Adds a free-text note to a contact's activity timeline.</summary>
    public Task<ContactActivity> AddActivityAsync(int contactId, string body, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(body)) throw new ArgumentException("body is required", nameof(body));
        return _http.PostJsonAsync<ActivityBody, ContactActivity>(
            "v1/contacts/" + contactId.ToString(CultureInfo.InvariantCulture) + "/activities",
            new ActivityBody(body), cancellationToken);
    }
}
