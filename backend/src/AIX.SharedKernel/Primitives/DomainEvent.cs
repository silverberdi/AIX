namespace AIX.SharedKernel.Primitives;

public abstract record DomainEvent(
    Guid EventId,
    DateTimeOffset OccurredOn,
    CorrelationId CorrelationId,
    CausationId? CausationId = null);
