using AIX.Documents.Domain;
using AIX.SharedKernel.Primitives;

namespace AIX.Documents.Events;

public sealed record DocumentFileAttached(
    Guid EventId,
    DateTimeOffset OccurredOn,
    CorrelationId CorrelationId,
    DocumentId DocumentId,
    DocumentFileId FileId,
    DocumentFileRole Role,
    string FileName,
    string ContentType,
    long SizeInBytes) : DomainEvent(EventId, OccurredOn, CorrelationId);
