namespace AIX.Metadata.Domain;

public sealed class VersionMetadataPayload
{
    public IReadOnlyDictionary<string, string?> StandaloneValues { get; }
    public IReadOnlyList<MetadataGroupInstancePayload> GroupInstances { get; }

    public VersionMetadataPayload(
        IReadOnlyDictionary<string, string?>? standaloneValues = null,
        IReadOnlyList<MetadataGroupInstancePayload>? groupInstances = null)
    {
        StandaloneValues = standaloneValues ?? new Dictionary<string, string?>();
        GroupInstances = groupInstances ?? [];
    }
}

public sealed record MetadataGroupInstancePayload(
    string GroupCode,
    string? InstanceKey,
    IReadOnlyDictionary<string, string?> Values);
