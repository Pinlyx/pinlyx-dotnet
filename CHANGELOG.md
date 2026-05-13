# Changelog

All notable changes to the CrmSolid .NET SDK will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

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

[Unreleased]: https://github.com/CRM-Solid/crmsolid-dotnet/compare/v0.1.0-alpha.1...HEAD
[0.1.0-alpha.1]: https://github.com/CRM-Solid/crmsolid-dotnet/releases/tag/v0.1.0-alpha.1
