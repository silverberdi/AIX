using AIX.Metadata.Domain;
using AIX.SharedKernel.Primitives;

namespace AIX.Metadata.Events;

public sealed record DocumentTypeVersionCreated(
    Guid EventId,
    DateTimeOffset OccurredOn,
    CorrelationId CorrelationId,
    DocumentTypeId DocumentTypeId,
    DocumentTypeVersionId DocumentTypeVersionId,
    int VersionNumber,
    IReadOnlyList<VersionSchemaFieldSnapshot> FieldSnapshots,
    IReadOnlyList<VersionSchemaGroupSnapshot> GroupSnapshots,
    IReadOnlyList<LayoutSectionSnapshot> LayoutSnapshots) : DomainEvent(EventId, OccurredOn, CorrelationId);
