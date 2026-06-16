namespace AIX.Metadata.Domain;

public abstract record LayoutPlacement(int Order);

public sealed record LayoutFieldPlacement(FieldSchemaId FieldSchemaId, int Order)
    : LayoutPlacement(Order);

public sealed record LayoutGroupPlacement(
    KeywordGroupId KeywordGroupId,
    string InstanceKey,
    int Order)
    : LayoutPlacement(Order);
