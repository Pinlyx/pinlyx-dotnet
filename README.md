# CrmSolid .NET SDK

[![NuGet](https://img.shields.io/nuget/v/CrmSolid.svg)](https://www.nuget.org/packages/CrmSolid/)
[![NuGet downloads](https://img.shields.io/nuget/dt/CrmSolid.svg)](https://www.nuget.org/packages/CrmSolid/)
[![CI](https://github.com/CRM-Solid/crmsolid-dotnet/actions/workflows/ci.yml/badge.svg)](https://github.com/CRM-Solid/crmsolid-dotnet/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![Targets](https://img.shields.io/badge/targets-netstandard2.0%20%7C%20net8.0%20%7C%20net9.0-512BD4.svg)](#)

The official **.NET SDK** for the [CRM Solid](https://crmsolid.com) omnichannel AI CRM
platform. It is strongly typed, scope-aware, and **safe-write by design**: it exposes the
full public API surface — CRM, messaging, sales, finance, automation and developer tooling —
while the operations the platform intentionally blocks (hard deletes, unscoped writes) stay
out of reach.

- **Typed clients** for the public REST API v1 (`/v1/*`) over a shared `HttpClient`
- **Bearer** auth (`csk_<env>_<key>`) for `/v1/*` + `/mcp`, and **HMAC** for legacy `/public/*`
- **Cursor pagination** as `IAsyncEnumerable<T>` (`StreamAllAsync`)
- **Automatic 429 retry** honoring `Retry-After` / `X-RateLimit-Reset`
- **Webhook signature verification** (`WebhookSignature.Verify`)
- **MCP** JSON-RPC client wrapper (`POST /mcp`)
- `IHttpClientFactory` / DI friendly · multi-target `netstandard2.0`, `net8.0`, `net9.0`

---

## Install

```sh
dotnet add package CrmSolid
```

Create an API key from your [Developer settings](https://app.crmsolid.com/settings/developers).
Pick only the scopes your integration needs — the create-key dialog groups them, and the
powerful ones (`finance:write`, `email:send`, `keys:manage`) are unchecked by default. The
token is shown **once**; store it securely. Format: `csk_<env>_<12-char-keyId><32-char-secret>`.

---

## Quickstart

```csharp
using CrmSolid;
using CrmSolid.Models;

var client = new CrmSolidClient("csk_live_xxxxxxxxxxxxYourSecretHere...");

// Who am I? (smoke test — returns identity + granted scopes)
var me = await client.Me.GetAsync();
Console.WriteLine($"{me.Email} — scopes: {string.Join(", ", me.ApiKey!.Scopes)}");

// List the first 50 Telegram contacts
var page = await client.Contacts.ListAsync(limit: 50, platform: Platform.Telegram);
foreach (var c in page.Items)
    Console.WriteLine($"#{c.Id} {c.Name} @{c.Username}");

// Stream every contact across all pages (cursor pagination, fetched on demand)
await foreach (var c in client.Contacts.StreamAllAsync(pageSize: 100))
    Console.WriteLine(c.Name);
```

---

## Authentication

### Bearer (default — `/v1/*` and `/mcp`)

```csharp
var client = new CrmSolidClient("csk_live_...");
// or, with options:
var client = new CrmSolidClient(new CrmSolidOptions
{
    ApiKey  = "csk_live_...",
    Timeout = TimeSpan.FromSeconds(30),
    MaxRetries = 3,                 // 429 auto-retry attempts
    AutoRetryOnRateLimit = true,
});
```

Each key carries a fixed set of scopes. A call that needs a scope your key lacks throws
`CrmSolidForbiddenException`. Use the constants in `CrmSolid.Models.Scopes` instead of magic
strings.

### Dependency injection

```csharp
// Program.cs
services.AddCrmSolid(opt => opt.ApiKey = builder.Configuration["CrmSolid:ApiKey"]);

public class MyService(CrmSolidClient crm)
{
    public Task<User> WhoAsync() => crm.Me.GetAsync();
}
```

`AddCrmSolid` wires the client through `IHttpClientFactory`, so handler lifetimes and
connection pooling are managed for you.

### HMAC (legacy `/public/*`)

```csharp
var client = new CrmSolidClient(new CrmSolidOptions
{
    HmacKeyId  = "abc123def456",
    HmacSecret = "your-secret",
});
```

The SDK signs each request `HMAC-SHA256(secret, METHOD\npath+query\nsha256-hex(body)\nunix-ts)`
and sets `X-API-Key` / `X-Timestamp` / `X-Signature`.

---

## Capability matrix

Every method is scope-gated. `R` = read, `W` = write (safe-write), `★` = powerful (off by default).

| Resource | Key methods | Scope | |
|---|---|---|:--:|
| `client.Me` | `GetAsync` | _none_ | R |
| `client.Contacts` | `ListAsync` · `GetAsync` · `CreateAsync` · `StreamAllAsync` | `contacts:read` | R |
| | `AddTagAsync` · `RemoveTagAsync` · `SetLeadScoreAsync` · `AssignAsync` · `AddActivityAsync` · `SetStageAsync` | `contacts:write` | W |
| `client.Conversations` | `ListRecentAsync` · `GetThreadAsync` · `StreamThreadAsync` | `contacts:read` | R |
| `client.Accounts` | `ListAsync` | `contacts:read` | R |
| `client.TelegramMessages` | `SendAsync` | `telegram:send` | W |
| | `GetAsync` | `telegram:read` | R |
| `client.TwitterMessages` | `SendAsync` | `twitter:send` | W |
| | `SearchAsync` | `contacts:read` | R |
| `client.Sequences` | `ListAsync` · `GetAsync` | `sequences:read` | R |
| | `PauseAsync` · `ResumeAsync` | `sequences:write` | W |
| `client.Deals` | `ListAsync` · `GetAsync` · `StreamAllAsync` | `deals:read` | R |
| | `CreateAsync` · `ChangeStageAsync` | `deals:write` | W |
| | `CloseAsync` (books revenue) | `deals:write` + `finance:write` | ★ |
| `client.Tasks` | `ListAsync` · `GetAsync` | `tasks:read` | R |
| | `CreateAsync` · `ChangeStatusAsync` · `CompleteAsync` | `tasks:write` | W |
| `client.Pipelines` | `ListAsync` · `GetAsync` | `pipelines:read` | R |
| `client.Finance` | `GetSummaryAsync` · `ListTransactionsAsync` · `ListInvoicesAsync` · `ListRevenueSourcesAsync` | `finance:read` | R |
| | `CreateTransactionAsync` · `MarkInvoicePaidAsync` | `finance:write` | ★ |
| `client.Email` | `ListThreadsAsync` · `GetThreadAsync` · `StreamThreadsAsync` | `email:read` | R |
| | `SetStatusAsync` · `AssignAsync` | `email:write` | W |
| | `ReplyAsync` (sends mail) | `email:send` | ★ |
| `client.Analytics` | `GetSummaryAsync` · `GetMessagingStatsAsync` · `ListTopContactsAsync` | `analytics:read` | R |
| `client.AiAgents` | `ListAsync` · `GetAsync` | `agents:read` | R |
| | `TestAsync` (dry-run, never sends) | `agents:run` | W |
| `client.Jobs` | `ListAsync` · `GetAsync` · `StreamAllAsync` | `jobs:read` | R |
| `client.Webhooks` | `ListAsync` · `GetAsync` · `ListDeliveriesAsync` · `ListEventTypesAsync` | `webhooks:read` | R |
| | `CreateAsync` · `UpdateAsync` · `RotateSecretAsync` · `SendTestAsync` · `DeleteAsync` | `webhooks:write` | W |
| `client.ApiKeys` | `ListAsync` · `CreateAsync` (attenuated) · `RevokeAsync` | `keys:manage` | ★ |
| `client.Mcp` | `ListToolsAsync` · `CallToolAsync` · `SendAsync` | _key-dependent_ | — |

Full OpenAPI spec: <https://app.crmsolid.com/openapi/public-v1.yaml>.

---

## Scopes

Constants live in `CrmSolid.Models.Scopes`. Default-granted scopes cover the standard
surface; the three **powerful** ones are off unless requested explicitly.

| Scope | Grants | Default |
|---|---|:--:|
| `contacts:read` / `contacts:write` | Contacts, tags, lead score, assignment, activity, stage, conversations, accounts | ✅ |
| `telegram:send` / `telegram:read` | Queue Telegram messages / read job status | ✅ |
| `twitter:send` | Send Twitter (X) DMs | ✅ |
| `sequences:read` / `sequences:write` | List sequences / pause-resume | ✅ |
| `analytics:read` | Dashboard KPIs, messaging stats, top contacts | ✅ |
| `deals:read` / `deals:write` | Pipeline read / create + move stages | ✅ |
| `tasks:read` / `tasks:write` | Tasks read / create + status | ✅ |
| `email:read` / `email:write` | Inbox read / status + assignment | ✅ |
| `finance:read` | Summary, transactions, invoices, revenue sources | ✅ |
| `pipelines:read` | Pipeline boards + stages | ✅ |
| `webhooks:read` / `webhooks:write` | Endpoints + deliveries / register + rotate | ✅ |
| `agents:read` / `agents:run` | List agents / run the test playground (no send) | ✅ |
| `jobs:read` | Outbound message-job monitor | ✅ |
| `finance:write` ★ | Create transactions, mark invoices paid | — |
| `email:send` ★ | Send a reply inside a thread | — |
| `keys:manage` ★ | Mint / revoke API keys (attenuated to your scopes) | — |

---

## Examples by resource

```csharp
// ── Twitter / X DMs ────────────────────────────────────────────────
var accounts = await client.Accounts.ListAsync(type: "twitter");
await client.TwitterMessages.SendAsync(new SendDmRequest
{
    XAccountId = accounts.Items[0].Id, ContactId = 42, Text = "Thanks for the follow!",
});

// ── Sequences (campaigns) ──────────────────────────────────────────
foreach (var s in await client.Sequences.ListAsync(status: "active"))
    Console.WriteLine($"{s.Name}: {s.ProcessedTargets}/{s.TotalTargets}");
await client.Sequences.PauseAsync(sequenceId: 3);   // idempotent

// ── Analytics ──────────────────────────────────────────────────────
var stats = await client.Analytics.GetMessagingStatsAsync(windowDays: 7);
Console.WriteLine($"7-day success rate: {stats.SuccessRate}%");

// ── Conversations ──────────────────────────────────────────────────
await foreach (var msg in client.Conversations.StreamThreadAsync(contactId: 42))
    Console.WriteLine($"[{msg.Direction}] {msg.Text}");

// ── Pipelines ──────────────────────────────────────────────────────
foreach (var board in await client.Pipelines.ListAsync())
    Console.WriteLine($"{board.Name}: {board.Stages.Count} stages, {board.ContactCount} contacts");
await client.Contacts.SetStageAsync(contactId: 42, stage: "negotiation");

// ── AI Agents (dry-run, never sends) ───────────────────────────────
var preview = await client.AiAgents.TestAsync(agentId: 1,
    new TestAgentRequest { IncomingText = "Do you offer annual billing?" });
Console.WriteLine(preview.Output);

// ── Jobs monitor ───────────────────────────────────────────────────
await foreach (var job in client.Jobs.StreamAllAsync(status: "failed"))
    Console.WriteLine($"#{job.Id} {job.Status}: {job.LastError}");

// ── Finance (write — needs finance:write) ──────────────────────────
await client.Finance.CreateTransactionAsync(new CreateTransactionRequest
{
    Type = "income", Amount = 1499.00m, Currency = "USD", Description = "Annual plan",
});
await client.Finance.MarkInvoicePaidAsync(invoiceId: 87);

// ── Email reply (needs email:send) ─────────────────────────────────
await client.Email.ReplyAsync(threadId: 12, new EmailReplyRequest
{
    BodyText = "Happy to help — here are the details…",   // To defaults to the last inbound sender
});

// ── Close a deal as won (needs deals:write + finance:write) ────────
await client.Deals.CloseAsync(dealId: 9);   // books an income transaction for the deal value
```

### Webhooks + signature verification

```csharp
// Register an endpoint — the signing secret is returned ONCE
var created = await client.Webhooks.CreateAsync(new CreateWebhookRequest
{
    Url = "https://example.com/crmsolid/hook",
    EventTypes = new[] { "contact.created", "deal.stage_changed" },  // or null for all
});
string secret = created.Secret;   // store this now

// In your receiver (ASP.NET Core Minimal API), verify the raw body before trusting it:
app.MapPost("/crmsolid/hook", async (HttpRequest req) =>
{
    using var reader = new StreamReader(req.Body);
    var raw = await reader.ReadToEndAsync();
    var sig = req.Headers["X-Webhook-Signature"].ToString();

    if (!CrmSolid.Webhooks.WebhookSignature.Verify(secret, raw, sig))
        return Results.Unauthorized();

    // ... process the verified payload ...
    return Results.Ok();
});
```

### Self-service API keys (attenuated)

```csharp
// A key with keys:manage can mint child keys — but only with scopes it already holds.
var ci = await client.ApiKeys.CreateAsync(new CreateApiKeyRequest
{
    Name = "CI pipeline", Scopes = "contacts:read deals:read", EnvLabel = "test",
});
Console.WriteLine(ci.Token);   // csk_test_… — shown once
await client.ApiKeys.RevokeAsync(ci.Id);
```

---

## Pagination & streaming

List endpoints are cursor-paginated and return `{ Items, NextCursor, HasMore }`. Pass
`NextCursor` as `after` to advance, or let `StreamAllAsync` walk every page for you:

```csharp
var first = await client.Deals.ListAsync(limit: 50);
var next  = first.HasMore ? await client.Deals.ListAsync(after: first.NextCursor) : null;

await foreach (var deal in client.Deals.StreamAllAsync(stage: "proposal"))
    Console.WriteLine(deal.Title);
```

---

## Error handling

Non-2xx responses raise typed exceptions, all derived from `CrmSolidApiException`:

| Status | Exception | Meaning |
|---|---|---|
| 400 | `CrmSolidValidationException` | Body / params failed validation |
| 401 | `CrmSolidAuthException` | Missing / invalid bearer |
| 403 | `CrmSolidForbiddenException` | Scope not granted, or IP not allowed |
| 404 | `CrmSolidNotFoundException` | Absent or not owned by caller |
| 429 | `CrmSolidRateLimitException` | Per-key limit hit (auto-retried first) |
| 5xx | `CrmSolidApiException` | Server-side — quote `RequestId` to support |

All expose `StatusCode`, the parsed `ApiError` envelope, and the `X-Request-Id` header.

```csharp
try
{
    await client.Deals.CloseAsync(9);
}
catch (CrmSolidForbiddenException ex)
{
    // e.g. key has deals:write but not finance:write
    Console.WriteLine(ex.Error?.Message);
}
```

---

## Rate limiting

The API limits per key and reports `X-RateLimit-Limit` / `-Remaining` / `-Reset`. On HTTP 429
the SDK **retries automatically** up to `MaxRetries` (default 3), honoring `Retry-After` then
`X-RateLimit-Reset`. When retries are exhausted, `CrmSolidRateLimitException` is thrown with
`RetryAfter`, `Limit`, `Remaining`, and `ResetAtUnix`. Disable with
`new CrmSolidOptions { AutoRetryOnRateLimit = false }`.

---

## Safety & boundaries

The SDK mirrors the platform's intentional limits, so an integration cannot do damage:

- **No hard deletes.** Removals are soft (archive / revoke) or absent.
- **Idempotent safe-writes.** Tag/untag, pause/resume, mark-paid and close are safe to retry.
- **Powerful scopes are opt-in.** `finance:write`, `email:send`, `keys:manage` are never
  default; `deals:close` additionally requires `finance:write` because it books revenue.
- **Attenuated key minting.** A minted key can only carry scopes the parent holds, never `*`.
- **AI agent runs are dry-runs.** `AiAgents.TestAsync` returns the would-be reply but never
  sends it.
- **Email is reply-only** within an existing thread — there is no bulk/compose blast.

---

## Versioning

Follows [Semantic Versioning](https://semver.org). Pre-1.0 releases may break between minors —
pin the patch. First stable target: `1.0.0`. See [CHANGELOG.md](CHANGELOG.md).

## Contributing

```sh
git clone https://github.com/CRM-Solid/crmsolid-dotnet.git
cd crmsolid-dotnet
dotnet test
```

## License

[MIT](LICENSE) © 2026 CRM Solid
