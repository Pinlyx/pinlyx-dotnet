using System;
using System.Collections.Generic;

namespace CrmSolid.Models;

/// <summary>
/// Request body for <c>POST /v1/finance/transactions</c> (requires <c>finance:write</c>).
/// Defaults to a completed entry dated now.
/// </summary>
public sealed class CreateTransactionRequest
{
    /// <summary>"income" or "expense" (required).</summary>
    public string Type { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Currency { get; set; }
    /// <summary>"completed" (default), "pending", "refunded" or "failed".</summary>
    public string? Status { get; set; }
    public DateTimeOffset? OccurredAt { get; set; }
    public string? Description { get; set; }
    public int? CategoryId { get; set; }
    public int? ContactId { get; set; }
    public string? ProductName { get; set; }
    public decimal? Fee { get; set; }
}

/// <summary>
/// Read-only finance overview. Totals are realized (completed) only and grouped per
/// currency — there is no FX conversion across currencies.
/// </summary>
public sealed record FinanceSummary
{
    public string Range { get; init; } = string.Empty;
    public DateTimeOffset From { get; init; }
    public DateTimeOffset To { get; init; }
    public string? Currency { get; init; }
    public IReadOnlyList<CurrencyTotal> Totals { get; init; } = Array.Empty<CurrencyTotal>();
    public IReadOnlyList<ExpenseCategoryTotal> TopExpenseCategories { get; init; } = Array.Empty<ExpenseCategoryTotal>();
    public IReadOnlyList<OutstandingTotal> Outstanding { get; init; } = Array.Empty<OutstandingTotal>();
}

public sealed record CurrencyTotal
{
    public string Currency { get; init; } = string.Empty;
    public decimal Income { get; init; }
    public decimal Expense { get; init; }
    public decimal Net { get; init; }
}

public sealed record ExpenseCategoryTotal
{
    public int? CategoryId { get; init; }
    public string CategoryName { get; init; } = string.Empty;
    public decimal Total { get; init; }
}

public sealed record OutstandingTotal
{
    public string Currency { get; init; } = string.Empty;
    public decimal IncomePending { get; init; }
    public decimal ExpensePending { get; init; }
}

/// <summary>A single finance ledger entry. Type is "income"/"expense"; status is "completed"/"pending"/"refunded"/"failed".</summary>
public sealed record Transaction
{
    public int Id { get; init; }
    public string Type { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "USD";
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset OccurredAt { get; init; }
    public string? Description { get; init; }
    public int? CategoryId { get; init; }
    public int? ContactId { get; init; }
    public string? ProductName { get; init; }
    public decimal? Fee { get; init; }
    /// <summary>Amount minus fee when a processor fee is present.</summary>
    public decimal? Net { get; init; }
    public string? SourceLabel { get; init; }
    public int? InvoiceId { get; init; }
}

public sealed record TransactionList
{
    public IReadOnlyList<Transaction> Items { get; init; } = Array.Empty<Transaction>();
    public int? NextCursor { get; init; }
    public bool HasMore { get; init; }
}

/// <summary>A customer invoice. Status is "draft"/"sent"/"paid"/"overdue"/"cancelled".</summary>
public sealed record Invoice
{
    public int Id { get; init; }
    public string InvoiceNumber { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string Currency { get; init; } = "USD";
    public decimal Total { get; init; }
    public int? ContactId { get; init; }
    public DateTimeOffset IssueDate { get; init; }
    public DateTimeOffset? DueDate { get; init; }
    public DateTimeOffset? PaidAt { get; init; }
}

public sealed record InvoiceList
{
    public IReadOnlyList<Invoice> Items { get; init; } = Array.Empty<Invoice>();
    public int? NextCursor { get; init; }
    public bool HasMore { get; init; }
    /// <summary>Issued-but-unpaid (sent/overdue) totals per currency.</summary>
    public IReadOnlyList<InvoiceOutstanding> Outstanding { get; init; } = Array.Empty<InvoiceOutstanding>();
}

public sealed record InvoiceOutstanding
{
    public string Currency { get; init; } = string.Empty;
    public int Count { get; init; }
    public decimal Total { get; init; }
}

/// <summary>A configured external revenue source. Secrets are never returned by the API.</summary>
public sealed record RevenueSource
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    /// <summary>"push" or "pull".</summary>
    public string Mode { get; init; } = string.Empty;
    public string DefaultCurrency { get; init; } = "USD";
    public bool IsActive { get; init; }
    public string? FeedUrl { get; init; }
    public int? PullIntervalMinutes { get; init; }
    /// <summary>"never" / "ok" / "error".</summary>
    public string LastSyncStatus { get; init; } = string.Empty;
    public DateTimeOffset? LastSyncAt { get; init; }
    public string? LastSyncError { get; init; }
    public long TotalIngested { get; init; }
    public DateTimeOffset? LastEventAt { get; init; }
    public DateTimeOffset? NextRunAt { get; init; }
}

/// <summary>Response for <c>GET /v1/finance/revenue-sources</c>.</summary>
public sealed record RevenueSourceList
{
    public IReadOnlyList<RevenueSource> Items { get; init; } = Array.Empty<RevenueSource>();
    public int Count { get; init; }
    public long TotalIngested { get; init; }
}
