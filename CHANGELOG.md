# Changelog

All notable changes to the CrmSolid .NET SDK will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [0.3.0-alpha.1] — 2026-06-06

A large surface expansion bringing the SDK up to the platform's real capacity. Everything
remains scope-gated and safe-write — destructive operations are still intentionally absent.

### Added
- **Tier 1 — typed coverage of already-granted capabilities** (no new scopes):
  - `TwitterMessages` — send X DMs (`twitter:send`) and search stored DMs.
  - `Sequences` — list / inspect / pause / resume campaigns (`sequences:read`/`write`).
  - `Analytics` — dashboard summary, messaging stats, top contacts (`analytics:read`).
  - `Conversations` — recent conversations + paginated message threads (`contacts:read`).
  - `Accounts` — connected Telegram + Twitter accounts (`contacts:read`).
  - `Contacts.SetStageAsync` — move a contact along its pipeline (`contacts:write`).
- **Tier 2 — new read / safe-write scopes**:
  - `Pipelines` — read boards + stages (`pipelines:read`).
  - `Webhooks` — register / rotate / test / delete endpoints + read deliveries
    (`webhooks:read`/`write`), plus `CrmSolid.Webhooks.WebhookSignature` to verify the
    `X-Webhook-Signature` header (HMAC-SHA256, constant-time).
  - `AiAgents` — list, inspect, and run the test playground without sending
    (`agents:read`/`agents:run`).
  - `Jobs` — read the outbound message-job monitor (`jobs:read`).
- **Tier 3 — powerful scopes, off by default** (grant deliberately):
  - `Finance.CreateTransactionAsync` / `MarkInvoicePaidAsync` (`finance:write`).
  - `Email.ReplyAsync` — send a reply inside a thread (`email:send`).
  - `Deals.CloseAsync` — close a deal as won; books revenue, so it requires BOTH
    `deals:write` AND `finance:write`.
  - `ApiKeys` — self-service list / mint / revoke, attenuated to the parent key's scopes
    (`keys:manage`).
- 11 new scope constants in `Scopes` (`PipelinesRead`, `WebhooksRead`/`Write`,
  `AgentsRead`/`Run`, `JobsRead`, `FinanceWrite`, `EmailSend`, `KeysManage`).
- `PATCH` support in the internal HTTP client.

### Notes
- New default-granted scopes: pipelines/webhooks/agents/jobs. `finance:write`, `email:send`
  and `keys:manage` are NOT default — request them explicitly when creating a key.
- Minted keys are attenuated (a key can never grant a scope it doesn't hold, nor `*`).

## [0.2.0-alpha.1] — 2026-06-02

### Added
- `Deals`, `Tasks`, `Finance`, and `Email` resources on `CrmSolidClient`, covering the
  v1 REST endpoints for the sales pipeline, tasks/reminders, read-only finance (summary,
  transactions, invoices, revenue sources), and the email inbox (read + status/assignment).
- CRM core-depth methods on `Contacts`: `ListTagDictionaryAsync`, `GetTagsAsync`,
  `AddTagAsync`, `RemoveTagAsync`, `SetLeadScoreAsync`, `AssignAsync`,
  `ListActivitiesAsync`, `AddActivityAsync`.
- Enriched `Contact` with `Email`, `Company`, `LeadScore`, `LeadScoreIsAi`,
  `AssignedToUserId`, and `Tags`.
- Cursor iterators: `Deals.StreamAllAsync`, `Tasks.StreamAllAsync`,
  `Finance.StreamTransactionsAsync`, `Email.StreamThreadsAsync`.
- New scope constants in `Scopes`: `FinanceRead`, `DealsRead`/`DealsWrite`,
  `TasksRead`/`TasksWrite`, `EmailRead`/`EmailWrite`, plus the previously missing
  `TwitterSend`, `SequencesRead`/`SequencesWrite`, `AnalyticsRead`.
- `PUT` and `DELETE` support in the internal HTTP client.

### Notes
- Safe-write boundary mirrors the API: finance is read-only, deals cannot be moved to
  `won` (it books revenue — panel-only), and email cannot be sent from the SDK.

## [0.1.0-alpha.1] — 2026-05-13

### Added
- Initial release.
- `CrmSolidClient` facade with `Me`, `Contacts`, `TelegramMessages`, and `Mcp` resources.
- Bearer-token credentials (`csk_<env>_<key>` format) for `/v1/*` and `/mcp`.
- HMAC-SHA256 credentials for legacy `/public/*` endpoints.
- `IAsyncEnumerable<Contact>` cursor pagination via `Contacts.StreamAllAsync`.
- Automatic HTTP 429 retry honoring `Retry-After` and `X-RateLimit-Reset`.
- Typed exception hierarchy: `CrmSolidValidationException` (400),
  `CrmSolidAuthException` (401), `CrmSolidForbiddenException` (403),
  `CrmSolidNotFoundException` (404), `CrmSolidRateLimitException` (429),
  and `CrmSolidApiException` (other non-2xx).
- DI registration helper: `services.AddCrmSolid(opt => ...)` via `IHttpClientFactory`.
- Multi-target: `netstandard2.0`, `net8.0`, `net9.0`.
- 23 unit tests covering auth, HMAC signing, rate-limit handler, resource shapes,
  pagination, and error mapping.

[Unreleased]: https://github.com/CRM-Solid/crmsolid-dotnet/compare/v0.3.0-alpha.1...HEAD
[0.3.0-alpha.1]: https://github.com/CRM-Solid/crmsolid-dotnet/compare/v0.2.0-alpha.1...v0.3.0-alpha.1
[0.2.0-alpha.1]: https://github.com/CRM-Solid/crmsolid-dotnet/compare/v0.1.0-alpha.1...v0.2.0-alpha.1
[0.1.0-alpha.1]: https://github.com/CRM-Solid/crmsolid-dotnet/releases/tag/v0.1.0-alpha.1
