using AIX.Metadata.Domain;
using AIX.SharedKernel.Primitives;

namespace AIX.Metadata.Events;

public sealed record KeywordGroupCreated(
    Guid EventId,
    DateTimeOffset OccurredOn,
    CorrelationId CorrelationId,
    KeywordGroupId KeywordGroupId,
    string Code,
    string Name,
    bool IsRepeatable,
    bool IsRequired,
    IReadOnlyList<KeywordId> KeywordIds) : DomainEvent(EventId, OccurredOn, CorrelationId);
