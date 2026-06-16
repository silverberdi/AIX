namespace AIX.Metadata.Domain;

public sealed record LayoutSection(
    LayoutSectionId Id,
    string Title,
    int Order,
    IReadOnlyList<LayoutPlacement> Placements);
