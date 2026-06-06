using System;
using System.Collections.Generic;

namespace CrmSolid.Models;

/// <summary>A column within a pipeline board. Kind is "Open"/"Won"/"Lost".</summary>
public sealed record PipelineStage
{
    public int Id { get; init; }
    public int PipelineId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Color { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int SortOrder { get; init; }
    public string Kind { get; init; } = string.Empty;
    public int ContactCount { get; init; }
}

/// <summary>A pipeline board with its stage columns and per-column contact counts.</summary>
public sealed record Pipeline
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? Icon { get; init; }
    public string Color { get; init; } = string.Empty;
    public int SortOrder { get; init; }
    public bool IsDefault { get; init; }
    public int ContactCount { get; init; }
    public IReadOnlyList<PipelineStage> Stages { get; init; } = Array.Empty<PipelineStage>();
}
