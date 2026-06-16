namespace AIX.Documents.Domain;

public sealed record DocumentCapturedMetadataGroupInstance
{
    public string GroupCode { get; }
    public string? InstanceKey { get; }
    public IReadOnlyDictionary<string, string?> Values { get; }

    public DocumentCapturedMetadataGroupInstance(
        string groupCode,
        string? instanceKey,
        IReadOnlyDictionary<string, string?> values)
    {
        GroupCode = groupCode;
        InstanceKey = instanceKey;
        Values = new Dictionary<string, string?>(values);
    }
}
