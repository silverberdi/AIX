namespace AIX.Metadata.Contracts;

public sealed record GroupAssignmentContract(
    Guid KeywordGroupId,
    string GroupCode,
    int Order,
    string InstanceKey,
    bool IsRepeatable,
    bool IsRequired,
    IReadOnlyList<Guid> KeywordIds);
