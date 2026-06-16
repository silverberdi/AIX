namespace AIX.Metadata.Contracts;

public sealed record LayoutGroupPlacementContract(
    Guid KeywordGroupId,
    string InstanceKey,
    int Order);
