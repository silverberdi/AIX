using AIX.Metadata.Domain;
using AIX.SharedKernel.Primitives;

namespace AIX.Metadata.Events;

public sealed record KeywordRegistered(
    Guid EventId,
    DateTimeOffset OccurredOn,
    CorrelationId CorrelationId,
    KeywordRegistryId KeywordRegistryId,
    KeywordId KeywordId,
    string Code,
    string Name,
    KeywordDataType DataType,
    int? MaxLength) : DomainEvent(EventId, OccurredOn, CorrelationId);
