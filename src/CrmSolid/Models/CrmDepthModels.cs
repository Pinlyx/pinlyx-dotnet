using System;

namespace CrmSolid.Models;

/// <summary>A contact tag (label).</summary>
public sealed record Tag
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    /// <summary>Hex color, e.g. "#2563EB".</summary>
    public string Color { get; init; } = "#2563EB";
}

/// <summary>A tag from the dictionary endpoint, with how many contacts carry it.</summary>
public sealed record TagWithCount
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Color { get; init; } = "#2563EB";
    public int ContactCount { get; init; }
}

/// <summary>
/// One entry in a contact's activity timeline. Type is one of Note, StageChanged,
/// Assigned, Unassigned, ScoreChanged, TagAdded, TagRemoved, FieldUpdated, Created.
/// </summary>
public sealed record ContactActivity
{
    public int Id { get; init; }
    public string Type { get; init; } = string.Empty;
    public string? Body { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}
