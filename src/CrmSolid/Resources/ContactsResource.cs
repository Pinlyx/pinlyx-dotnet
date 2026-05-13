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
}
