namespace AIX.Metadata.Contracts;

public sealed record LayoutSectionContract(
    Guid SectionId,
    string Title,
    int Order,
    IReadOnlyList<LayoutFieldPlacementContract> FieldPlacements,
    IReadOnlyList<LayoutGroupPlacementContract> GroupPlacements);
