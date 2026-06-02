using System;
using System.Collections.Generic;

namespace CrmSolid.Models;

/// <summary>
/// Generic wrapper for endpoints that return a non-paginated <c>{ "items": [...] }</c>
/// envelope (tags, activities). Use the typed resource methods rather than this directly.
/// </summary>
public sealed record ItemsResponse<T>
{
    public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
}
