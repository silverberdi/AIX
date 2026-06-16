using AIX.Documents.Domain;
using AIX.SharedKernel.Primitives;

namespace AIX.Documents.Events;

public sealed record DocumentCompleted(
    Guid EventId,
    DateTimeOffset OccurredOn,
    CorrelationId CorrelationId,
    DocumentId DocumentId) : DomainEvent(EventId, OccurredOn, CorrelationId);
