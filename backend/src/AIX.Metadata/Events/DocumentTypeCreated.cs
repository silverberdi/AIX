using AIX.Metadata.Domain;
using AIX.SharedKernel.Primitives;

namespace AIX.Metadata.Events;

public sealed record DocumentTypeCreated(
    Guid EventId,
    DateTimeOffset OccurredOn,
    CorrelationId CorrelationId,
    DocumentTypeId DocumentTypeId,
    string Name,
    string Code) : DomainEvent(EventId, OccurredOn, CorrelationId);
