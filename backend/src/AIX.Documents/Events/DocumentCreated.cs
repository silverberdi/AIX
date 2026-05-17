using AIX.Documents.Domain;
using AIX.SharedKernel.Primitives;

namespace AIX.Documents.Events;

public sealed record DocumentCreated(
    Guid EventId,
    DateTimeOffset OccurredOn,
    CorrelationId CorrelationId,
    DocumentId DocumentId,
    DocumentTypeId DocumentTypeId,
    DocumentTypeVersionId DocumentTypeVersionId,
    TaxonomyNodeId TaxonomyNodeId,
    UserId CreatedBy) : DomainEvent(EventId, OccurredOn, CorrelationId);
