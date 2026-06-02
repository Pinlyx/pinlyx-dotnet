using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using CrmSolid.Http;
using CrmSolid.Models;

namespace CrmSolid.Resources;

/// <summary>Read-only finance + revenue. Backed by <c>/v1/finance/*</c>. No write operations exist.</summary>
public sealed class FinanceResource
{
    private readonly CrmSolidHttpClient _http;

    internal FinanceResource(CrmSolidHttpClient http) { _http = http; }

    /// <summary>Per-currency realized totals + outstanding + top expense categories.</summary>
    public Task<FinanceSummary> GetSummaryAsync(string range = "30d", string? currency = null, CancellationToken cancellationToken = default)
    {
        var qs = new QueryStringBuilder();
        if (range != "30d") qs.Add("range", range);
        if (!string.IsNullOrEmpty(currency)) qs.Add("currency", currency!);
        return _http.GetAsync<FinanceSummary>("v1/finance/summary" + qs, cancellationToken);
    }

    /// <summary>Cursor-paginated ledger entries.</summary>
    public Task<TransactionList> ListTransactionsAsync(
        int? after = null,
        int limit = 25,
        string? type = null,
        string? status = null,
        int? contactId = null,
        string? currency = null,
        CancellationToken cancellationToken = default)
    {
        var qs = new QueryStringBuilder();
        if (after.HasValue) qs.Add("after", after.Value.ToString(CultureInfo.InvariantCulture));
        if (limit != 25) qs.Add("limit", limit.ToString(CultureInfo.InvariantCulture));
        if (!string.IsNullOrEmpty(type)) qs.Add("type", type!);
        if (!string.IsNullOrEmpty(status)) qs.Add("status", status!);
        if (contactId.HasValue) qs.Add("contactId", contactId.Value.ToString(CultureInfo.InvariantCulture));
        if (!string.IsNullOrEmpty(currency)) qs.Add("currency", currency!);
        return _http.GetAsync<TransactionList>("v1/finance/transactions" + qs, cancellationToken);
    }

    /// <summary>Cursor-paginated invoices with an outstanding (sent/overdue) summary per currency.</summary>
    public Task<InvoiceList> ListInvoicesAsync(
        int? after = null,
        int limit = 25,
        string? status = null,
        CancellationToken cancellationToken = default)
    {
        var qs = new QueryStringBuilder();
        if (after.HasValue) qs.Add("after", after.Value.ToString(CultureInfo.InvariantCulture));
        if (limit != 25) qs.Add("limit", limit.ToString(CultureInfo.InvariantCulture));
        if (!string.IsNullOrEmpty(status)) qs.Add("status", status!);
        return _http.GetAsync<InvoiceList>("v1/finance/invoices" + qs, cancellationToken);
    }

    /// <summary>Lists configured external revenue sources (secrets are never returned).</summary>
    public Task<RevenueSourceList> ListRevenueSourcesAsync(CancellationToken cancellationToken = default)
        => _http.GetAsync<RevenueSourceList>("v1/finance/revenue-sources", cancellationToken);

    /// <summary>Streams every transaction across all pages using cursor pagination.</summary>
    public async IAsyncEnumerable<Transaction> StreamTransactionsAsync(
        int pageSize = 100,
        string? type = null,
        string? status = null,
        int? contactId = null,
        string? currency = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        int? cursor = null;
        while (true)
        {
            var page = await ListTransactionsAsync(cursor, pageSize, type, status, contactId, currency, cancellationToken).ConfigureAwait(false);
            foreach (var tx in page.Items) yield return tx;
            if (!page.HasMore || page.NextCursor is null) yield break;
            cursor = page.NextCursor;
        }
    }
}
