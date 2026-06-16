using AIX.Documents.Domain;
using AIX.SharedKernel.Primitives;

namespace AIX.Documents.Events;

public sealed record DocumentMetadataCaptured(
    Guid EventId,
    DateTimeOffset OccurredOn,
    CorrelationId CorrelationId,
    DocumentId DocumentId,
    IReadOnlyDictionary<string, string?> StandaloneValues,
    IReadOnlyList<DocumentCapturedMetadataGroupInstance> GroupInstances) : DomainEvent(EventId, OccurredOn, CorrelationId);
