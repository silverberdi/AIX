using AIX.Documents.Contracts;

namespace AIX.Documents.Domain;

public sealed class DocumentCapturedMetadata
{
    public IReadOnlyDictionary<string, string?> StandaloneValues { get; }
    public IReadOnlyList<DocumentCapturedMetadataGroupInstance> GroupInstances { get; }

    private DocumentCapturedMetadata(
        IReadOnlyDictionary<string, string?> standaloneValues,
        IReadOnlyList<DocumentCapturedMetadataGroupInstance> groupInstances)
    {
        StandaloneValues = standaloneValues;
        GroupInstances = groupInstances;
    }

    public static DocumentCapturedMetadata From(CapturedMetadataPayload payload)
    {
        ArgumentNullException.ThrowIfNull(payload);

        var standaloneValues = new Dictionary<string, string?>(payload.StandaloneValues);
        var groupInstances = payload.GroupInstances
            .Select(group => new DocumentCapturedMetadataGroupInstance(
                group.GroupCode,
                group.InstanceKey,
                group.Values))
            .ToList();

        return new DocumentCapturedMetadata(standaloneValues, groupInstances);
    }
}
