namespace AIX.Metadata.Domain;

public sealed record VersionSchemaGroupAssignment(
    KeywordGroupId KeywordGroupId,
    string GroupCode,
    int Order,
    string InstanceKey,
    bool IsRepeatable,
    bool IsRequired,
    IReadOnlyList<KeywordId> KeywordIds);
