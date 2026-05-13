# CrmSolid .NET SDK

[![NuGet](https://img.shields.io/nuget/v/CrmSolid.svg)](https://www.nuget.org/packages/CrmSolid/)
[![NuGet downloads](https://img.shields.io/nuget/dt/CrmSolid.svg)](https://www.nuget.org/packages/CrmSolid/)
[![CI](https://github.com/CRM-Solid/crmsolid-dotnet/actions/workflows/ci.yml/badge.svg)](https://github.com/CRM-Solid/crmsolid-dotnet/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

Official **.NET SDK** for the [CRM Solid](https://crmsolid.com) omnichannel AI CRM platform.

- ✅ Strongly-typed clients for the public REST API v1 (`/v1/*`)
- ✅ Bearer token auth (`csk_<env>_<key>`) and HMAC signing for legacy `/public/*`
- ✅ `IAsyncEnumerable` cursor pagination
- ✅ Automatic 429 retry honoring `Retry-After` / `X-RateLimit-Reset`
- ✅ MCP JSON-RPC client wrapper (`POST /mcp`)
- ✅ `IHttpClientFactory` / DI friendly
- ✅ Multi-target: `netstandard2.0`, `net8.0`, `net9.0`

---

## Install

```sh
dotnet add package CrmSolid
```

## Quickstart

```csharp
using CrmSolid;
using CrmSolid.Models;

var client = new CrmSolidClient("csk_live_xxxxxxxxxxxxYourSecretHere...");

// Smoke test — who am I?
var me = await client.Me.GetAsync();
Console.WriteLine($"Workspace: {me.Email}  scopes: {string.Join(", ", me.ApiKey!.Scopes)}");

// List the first 50 contacts
var page = await client.Contacts.ListAsync(limit: 50, platform: Platform.Telegram);
foreach (var c in page.Items)
    Console.WriteLine($"#{c.Id} {c.Name} @{c.Username}");

// Stream every contact across all pages (cursor pagination, on-demand)
await foreach (var c in client.Contacts.StreamAllAsync(pageSize: 100))
    Console.WriteLine(c.Name);

// Send a Telegram message (async — returns the queued job)
var job = await client.TelegramMessages.SendAsync(new SendMessageRequest
{
    AccountId = 7,
    Username = "john_doe",
    Text = "Hello from the SDK!",
});
Console.WriteLine($"Queued job {job.Id} ({job.Status}).");
```

Get an API key from your [CRM Solid dashboard](https://app.crmsolid.com/settings/developers). Token format: `csk_<env>_<12-char-keyId><32-char-secret>`.

---

## Authentication

The SDK supports two credential schemes:

### Bearer (default — for `/v1/*` and `/mcp`)

```csharp
var client = new CrmSolidClient("csk_live_...");
// or
var client = new CrmSolidClient(new CrmSolidOptions { ApiKey = "csk_live_..." });
```

Each key is issued with a fixed set of scopes (`contacts:read`, `contacts:write`, `telegram:send`, `telegram:read`). Operations that need a scope your key doesn't have throw `CrmSolidForbiddenException`.

### HMAC (for legacy `/public/*` endpoints)

```csharp
var client = new CrmSolidClient(new CrmSolidOptions
{
    HmacKeyId  = "abc123def456",
    HmacSecret = "your-secret",
});
```

The SDK signs every request as `HMAC-SHA256(secret, METHOD\npath+query\nsha256-hex(body)\nunix-ts)` and sets the `X-API-Key`, `X-Timestamp`, `X-Signature` headers.

---

## Dependency injection

```csharp
// Program.cs
services.AddCrmSolid(opt =>
{
    opt.ApiKey = builder.Configuration["CrmSolid:ApiKey"];
});

// then inject CrmSolidClient anywhere
public class MyService(CrmSolidClient crm)
{
    public Task<User> WhoAsync() => crm.Me.GetAsync();
}
```

`AddCrmSolid` uses `IHttpClientFactory` under the hood, so connection pooling and handler lifetimes are managed for you.

---

## API reference

| Resource | Endpoints | Required scope |
|---|---|---|
| `client.Me.GetAsync()` | `GET /v1/me` | _none_ |
| `client.Contacts.ListAsync(...)` | `GET /v1/contacts` | `contacts:read` |
| `client.Contacts.StreamAllAsync(...)` | `GET /v1/contacts` (paged) | `contacts:read` |
| `client.Contacts.GetAsync(id)` | `GET /v1/contacts/{id}` | `contacts:read` |
| `client.Contacts.CreateAsync(req)` | `POST /v1/contacts` | `contacts:write` |
| `client.TelegramMessages.SendAsync(req)` | `POST /v1/telegram/messages` | `telegram:send` |
| `client.TelegramMessages.GetAsync(id)` | `GET /v1/telegram/messages/{id}` | `telegram:read` |
| `client.Mcp.SendAsync(method, params)` | `POST /mcp` | _key-dependent_ |
| `client.Mcp.ListToolsAsync()` | `POST /mcp` (`tools/list`) | _key-dependent_ |
| `client.Mcp.CallToolAsync(name, args)` | `POST /mcp` (`tools/call`) | _key-dependent_ |

The full OpenAPI specification lives at <https://app.crmsolid.com/openapi/public-v1.yaml>.

---

## Error handling

Non-2xx responses raise typed exceptions, all derived from `CrmSolidApiException`:

| Status | Exception | Notes |
|---|---|---|
| 400 | `CrmSolidValidationException` | Body or params failed validation |
| 401 | `CrmSolidAuthException` | Missing / invalid bearer |
| 403 | `CrmSolidForbiddenException` | Scope not granted or IP not allowed |
| 404 | `CrmSolidNotFoundException` | Not owned by caller or absent |
| 429 | `CrmSolidRateLimitException` | Per-key limit hit (see below) |
| 5xx | `CrmSolidApiException` | Server-side; consult `RequestId` |

All exceptions expose `StatusCode`, the parsed `ApiError` envelope, and the `X-Request-Id` header for support.

```csharp
try
{
    await client.Contacts.GetAsync(999_999);
}
catch (CrmSolidNotFoundException ex)
{
    Console.WriteLine($"missing — {ex.Error?.Message}");
}
```

---

## Rate limiting

The API rate-limits per-key and communicates state via `X-RateLimit-Limit`, `X-RateLimit-Remaining`, `X-RateLimit-Reset`. On HTTP 429 the SDK **retries automatically** up to `CrmSolidOptions.MaxRetries` (default `3`), honoring `Retry-After` first and `X-RateLimit-Reset` as a fallback. When retries are exhausted, `CrmSolidRateLimitException` is thrown with `RetryAfter`, `Limit`, `Remaining`, and `ResetAtUnix` populated.

Disable auto-retry:

```csharp
new CrmSolidOptions { ApiKey = "...", AutoRetryOnRateLimit = false };
```

---

## Examples

See the [`examples/`](examples/) folder:

- [`BasicQuickstart`](examples/BasicQuickstart) — auth check + list contacts
- [`SendTelegramMessage`](examples/SendTelegramMessage) — enqueue + poll until terminal
- [`BulkContactImport`](examples/BulkContactImport) — full-workspace contact stream

Run any of them with:

```sh
cd examples/BasicQuickstart
CRMSOLID_API_KEY=csk_live_... dotnet run
```

---

## Versioning

This SDK follows [Semantic Versioning](https://semver.org). Pre-1.0 releases may contain breaking changes between minors; pin the patch.

The first stable release is targeted for `1.0.0`. Until then, expect occasional churn in error types and resource shapes.

---

## Contributing

Issues and PRs welcome. To build locally:

```sh
git clone https://github.com/CRM-Solid/crmsolid-dotnet.git
cd crmsolid-dotnet
dotnet test
```

---

## License

[MIT](LICENSE) © 2026 CRM Solid
