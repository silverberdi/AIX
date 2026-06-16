namespace AIX.Documents.Contracts;

public sealed record CapturedMetadataGroupInstance
{
    public string GroupCode { get; }
    public string? InstanceKey { get; }
    public IReadOnlyDictionary<string, string?> Values { get; }

    public CapturedMetadataGroupInstance(
        string groupCode,
        string? instanceKey,
        IReadOnlyDictionary<string, string?> values)
    {
        GroupCode = groupCode;
        InstanceKey = instanceKey;
        Values = new Dictionary<string, string?>(values);
    }
}
