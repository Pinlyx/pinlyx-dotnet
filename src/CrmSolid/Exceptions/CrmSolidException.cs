using System;
using System.Net;
using CrmSolid.Models;

namespace CrmSolid.Exceptions;

/// <summary>Base class for all Pinlyx SDK exceptions.</summary>
public class CrmSolidException : Exception
{
    public CrmSolidException(string message) : base(message) { }
    public CrmSolidException(string message, Exception inner) : base(message, inner) { }
}

/// <summary>Raised when the API returns a non-success HTTP status code.</summary>
public class CrmSolidApiException : CrmSolidException
{
    /// <summary>The HTTP status code returned by the API.</summary>
    public HttpStatusCode StatusCode { get; }

    /// <summary>The parsed error envelope, if the response body was a valid <see cref="ApiError"/>.</summary>
    public ApiError? Error { get; }

    /// <summary>The <c>X-Request-Id</c> header value, if present. Useful for support.</summary>
    public string? RequestId { get; }

    public CrmSolidApiException(HttpStatusCode statusCode, ApiError? error, string? requestId, string message)
        : base(message)
    {
        StatusCode = statusCode;
        Error = error;
        RequestId = requestId;
    }
}

/// <summary>Raised on HTTP 401 — missing or invalid bearer token.</summary>
public sealed class CrmSolidAuthException : CrmSolidApiException
{
    public CrmSolidAuthException(ApiError? error, string? requestId, string message)
        : base(HttpStatusCode.Unauthorized, error, requestId, message) { }
}

/// <summary>Raised on HTTP 403 — the key lacks the scope required for this operation, or IP not allowed.</summary>
public sealed class CrmSolidForbiddenException : CrmSolidApiException
{
    public CrmSolidForbiddenException(ApiError? error, string? requestId, string message)
        : base(HttpStatusCode.Forbidden, error, requestId, message) { }
}

/// <summary>Raised on HTTP 404 — resource does not exist or is not owned by the caller.</summary>
public sealed class CrmSolidNotFoundException : CrmSolidApiException
{
    public CrmSolidNotFoundException(ApiError? error, string? requestId, string message)
        : base(HttpStatusCode.NotFound, error, requestId, message) { }
}

/// <summary>Raised on HTTP 400 — request body or parameters failed validation.</summary>
public sealed class CrmSolidValidationException : CrmSolidApiException
{
    public CrmSolidValidationException(ApiError? error, string? requestId, string message)
        : base(HttpStatusCode.BadRequest, error, requestId, message) { }
}

/// <summary>
/// Raised on HTTP 429 — per-key rate limit exceeded. The handler retries automatically
/// up to <see cref="CrmSolidOptions.MaxRetries"/> times respecting <see cref="RetryAfter"/>;
/// if retries are exhausted (or disabled) this exception is thrown.
/// </summary>
public sealed class CrmSolidRateLimitException : CrmSolidApiException
{
    /// <summary>Duration to wait before retrying, parsed from the <c>Retry-After</c> header.</summary>
    public TimeSpan? RetryAfter { get; }

    /// <summary>Max requests allowed per minute for this key (from <c>X-RateLimit-Limit</c>).</summary>
    public int? Limit { get; }

    /// <summary>Requests remaining in the current window (from <c>X-RateLimit-Remaining</c>).</summary>
    public int? Remaining { get; }

    /// <summary>Unix timestamp (seconds) when the rate limit window resets (from <c>X-RateLimit-Reset</c>).</summary>
    public long? ResetAtUnix { get; }

    public CrmSolidRateLimitException(
        ApiError? error,
        string? requestId,
        string message,
        TimeSpan? retryAfter,
        int? limit,
        int? remaining,
        long? resetAtUnix)
        : base((HttpStatusCode)429, error, requestId, message)
    {
        RetryAfter = retryAfter;
        Limit = limit;
        Remaining = remaining;
        ResetAtUnix = resetAtUnix;
    }
}
