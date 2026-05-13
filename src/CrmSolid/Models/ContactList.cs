using System;
using System.Collections.Generic;

namespace CrmSolid.Models;

/// <summary>
/// Cursor-paginated list of contacts. Pass <see cref="NextCursor"/> as the <c>after</c>
/// parameter on the next request to continue pagination.
/// </summary>
public sealed record ContactList
{
    public IReadOnlyList<Contact> Items { get; init; } = Array.Empty<Contact>();

    /// <summary>Cursor for the next page. Null when <see cref="HasMore"/> is false.</summary>
    public int? NextCursor { get; init; }

    /// <summary>Whether additional pages exist beyond this response.</summary>
    public bool HasMore { get; init; }
}
