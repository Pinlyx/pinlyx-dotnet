using System.Text.Json;

namespace CrmSolid.Models;

/// <summary>Standard error envelope returned by all v1 error responses.</summary>
public sealed record ApiError
{
    /// <summary>
    /// Machine-readable error code. Known values: <c>bad_request</c>, <c>not_found</c>,
    /// <c>conflict</c>, <c>unauthorized</c>, <c>forbidden</c>, <c>rate_limited</c>,
    /// <c>internal_error</c>.
    /// </summary>
    public string Error { get; init; } = string.Empty;

    /// <summary>Human-readable explanation of the error.</summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>Optional structured context (field-level validation errors, etc.).</summary>
    public JsonElement? Details { get; init; }
}
