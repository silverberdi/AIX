namespace AIX.Metadata.Domain;

public sealed record LayoutFieldPlacementSnapshot(FieldSchemaId FieldSchemaId, int Order);

public sealed record LayoutGroupPlacementSnapshot(
    KeywordGroupId KeywordGroupId,
    string InstanceKey,
    int Order);

public sealed record LayoutSectionSnapshot(
    LayoutSectionId SectionId,
    string Title,
    int Order,
    IReadOnlyList<LayoutFieldPlacementSnapshot> FieldPlacements,
    IReadOnlyList<LayoutGroupPlacementSnapshot> GroupPlacements);
