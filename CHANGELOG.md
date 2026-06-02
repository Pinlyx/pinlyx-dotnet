# Changelog

All notable changes to the CrmSolid .NET SDK will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

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

[Unreleased]: https://github.com/CRM-Solid/crmsolid-dotnet/compare/v0.2.0-alpha.1...HEAD
[0.2.0-alpha.1]: https://github.com/CRM-Solid/crmsolid-dotnet/compare/v0.1.0-alpha.1...v0.2.0-alpha.1
[0.1.0-alpha.1]: https://github.com/CRM-Solid/crmsolid-dotnet/releases/tag/v0.1.0-alpha.1
