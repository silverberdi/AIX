using AIX.Documents.Events;
using AIX.SharedKernel.Primitives;

namespace AIX.Documents.Domain;

public sealed class Document : Entity<DocumentId>
{
    private readonly DocumentTypeId _documentTypeId;
    private readonly DocumentTypeVersionId _documentTypeVersionId;
    private readonly List<DomainEvent> _domainEvents = [];

    public DocumentTypeId DocumentTypeId => _documentTypeId;
    public DocumentTypeVersionId DocumentTypeVersionId => _documentTypeVersionId;
    public TaxonomyNodeId TaxonomyNodeId { get; private init; }
    public UserId CreatedBy { get; private init; }
    public DateTimeOffset CreatedAt { get; private init; }
    public DocumentState State { get; private init; }

    public IReadOnlyList<DomainEvent> DomainEvents => _domainEvents;

    private Document(
        DocumentId id,
        DocumentTypeId documentTypeId,
        DocumentTypeVersionId documentTypeVersionId,
        TaxonomyNodeId taxonomyNodeId,
        UserId createdBy,
        DateTimeOffset createdAt)
    {
        Id = id;
        _documentTypeId = documentTypeId;
        _documentTypeVersionId = documentTypeVersionId;
        TaxonomyNodeId = taxonomyNodeId;
        CreatedBy = createdBy;
        CreatedAt = createdAt;
        State = DocumentState.Draft;
    }

    public static Result<Document> Create(
        DocumentTypeId documentTypeId,
        DocumentTypeVersionId documentTypeVersionId,
        TaxonomyNodeId taxonomyNodeId,
        UserId createdBy,
        IClock clock)
    {
        ArgumentNullException.ThrowIfNull(clock);

        if (documentTypeId.Value == Guid.Empty)
        {
            return Result<Document>.Failure(DocumentErrors.DocumentTypeRequired);
        }

        if (documentTypeVersionId.Value == Guid.Empty)
        {
            return Result<Document>.Failure(DocumentErrors.DocumentTypeVersionRequired);
        }

        var createdAt = clock.UtcNow;
        var documentId = DocumentId.New();
        var document = new Document(
            documentId,
            documentTypeId,
            documentTypeVersionId,
            taxonomyNodeId,
            createdBy,
            createdAt);

        document._domainEvents.Add(new DocumentCreated(
            Guid.NewGuid(),
            createdAt,
            CorrelationId.New(),
            documentId,
            documentTypeId,
            documentTypeVersionId,
            taxonomyNodeId,
            createdBy));

        return Result<Document>.Success(document);
    }
}
