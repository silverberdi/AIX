namespace AIX.Metadata.Domain;

public sealed record LayoutSectionDefinition(
    string Title,
    int Order,
    IReadOnlyList<LayoutPlacementDefinition> Placements,
    LayoutSectionId? Id = null);
