namespace AIX.Documents.Contracts;

public sealed class CapturedMetadataPayload
{
    public IReadOnlyDictionary<string, string?> StandaloneValues { get; }
    public IReadOnlyList<CapturedMetadataGroupInstance> GroupInstances { get; }

    public CapturedMetadataPayload(
        IReadOnlyDictionary<string, string?>? standaloneValues = null,
        IReadOnlyList<CapturedMetadataGroupInstance>? groupInstances = null)
    {
        StandaloneValues = standaloneValues is null
            ? new Dictionary<string, string?>()
            : new Dictionary<string, string?>(standaloneValues);
        GroupInstances = groupInstances is null
            ? []
            : [.. groupInstances];
    }
}
