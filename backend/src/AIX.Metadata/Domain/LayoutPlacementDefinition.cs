namespace AIX.Metadata.Domain;

public abstract record LayoutPlacementDefinition(int Order);

public sealed record LayoutFieldPlacementDefinition(KeywordId KeywordId, int Order)
    : LayoutPlacementDefinition(Order);

public sealed record LayoutGroupPlacementDefinition(
    KeywordGroupId KeywordGroupId,
    int Order,
    string? InstanceKey = null)
    : LayoutPlacementDefinition(Order);
