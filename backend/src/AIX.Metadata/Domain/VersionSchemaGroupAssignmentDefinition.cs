namespace AIX.Metadata.Domain;

public sealed record VersionSchemaGroupAssignmentDefinition(
    KeywordGroupId KeywordGroupId,
    int Order,
    string? InstanceKey = null);
