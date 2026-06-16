using AIX.Documents.Contracts;
using AIX.Documents.Events;
using AIX.SharedKernel.Primitives;

namespace AIX.Documents.Domain;

public sealed class Document : Entity<DocumentId>
{
    private readonly DocumentTypeId _documentTypeId;
    private readonly DocumentTypeVersionId _documentTypeVersionId;
    private readonly List<DocumentFile> _files = [];
    private readonly List<DomainEvent> _domainEvents = [];
    private DocumentCapturedMetadata? _capturedMetadata;

    public DocumentTypeId DocumentTypeId => _documentTypeId;
    public DocumentTypeVersionId DocumentTypeVersionId => _documentTypeVersionId;
    public TaxonomyNodeId TaxonomyNodeId { get; private init; }
    public UserId CreatedBy { get; private init; }
    public DateTimeOffset CreatedAt { get; private init; }
    public DocumentState State { get; private set; }
    public IReadOnlyList<DocumentFile> Files => _files;
    public DocumentCapturedMetadata? CapturedMetadata => _capturedMetadata;

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

    public Result Complete(IClock clock)
    {
        ArgumentNullException.ThrowIfNull(clock);

        if (State == DocumentState.Complete)
        {
            return Result.Failure(DocumentErrors.AlreadyComplete);
        }

        State = DocumentState.Complete;

        _domainEvents.Add(new DocumentCompleted(
            Guid.NewGuid(),
            clock.UtcNow,
            CorrelationId.New(),
            Id));

        return Result.Success();
    }

    public Result AttachFile(
        DocumentFileId fileId,
        DocumentFileRole role,
        string fileName,
        string contentType,
        long sizeInBytes,
        IClock clock)
    {
        ArgumentNullException.ThrowIfNull(clock);

        if (State == DocumentState.Complete)
        {
            return Result.Failure(DocumentErrors.CannotModifyWhenComplete);
        }

        if (fileId.Value == Guid.Empty)
        {
            return Result.Failure(DocumentErrors.FileIdRequired);
        }

        if (string.IsNullOrWhiteSpace(fileName))
        {
            return Result.Failure(DocumentErrors.FileNameRequired);
        }

        if (string.IsNullOrWhiteSpace(contentType))
        {
            return Result.Failure(DocumentErrors.ContentTypeRequired);
        }

        if (sizeInBytes <= 0)
        {
            return Result.Failure(DocumentErrors.FileSizeInvalid);
        }

        if (_files.Any(file => file.Id == fileId))
        {
            return Result.Failure(DocumentErrors.FileAlreadyAttached);
        }

        if (role == DocumentFileRole.Primary && _files.Any(file => file.Role == DocumentFileRole.Primary))
        {
            return Result.Failure(DocumentErrors.PrimaryFileAlreadyExists);
        }

        var file = new DocumentFile(fileId, role, fileName, contentType, sizeInBytes);
        _files.Add(file);

        _domainEvents.Add(new DocumentFileAttached(
            Guid.NewGuid(),
            clock.UtcNow,
            CorrelationId.New(),
            Id,
            fileId,
            role,
            fileName,
            contentType,
            sizeInBytes));

        return Result.Success();
    }

    public Result SetCapturedMetadata(CapturedMetadataPayload payload, IClock clock)
    {
        ArgumentNullException.ThrowIfNull(payload);
        ArgumentNullException.ThrowIfNull(clock);

        if (State == DocumentState.Complete)
        {
            return Result.Failure(DocumentErrors.CannotModifyWhenComplete);
        }

        var capturedMetadata = DocumentCapturedMetadata.From(payload);
        _capturedMetadata = capturedMetadata;

        _domainEvents.Add(new DocumentMetadataCaptured(
            Guid.NewGuid(),
            clock.UtcNow,
            CorrelationId.New(),
            Id,
            capturedMetadata.StandaloneValues,
            capturedMetadata.GroupInstances));

        return Result.Success();
    }
}
