using AIX.Metadata.Events;
using AIX.SharedKernel.Primitives;

namespace AIX.Metadata.Domain;

public sealed class DocumentType : Entity<DocumentTypeId>
{
    private readonly List<DocumentTypeVersion> _versions = [];
    private readonly List<DomainEvent> _domainEvents = [];

    public string Name { get; private init; } = string.Empty;
    public string Code { get; private init; } = string.Empty;
    public DocumentTypeState State { get; private set; }
    public DateTimeOffset CreatedAt { get; private init; }
    public IReadOnlyList<DocumentTypeVersion> Versions => _versions;
    public DocumentTypeVersion? LatestVersion =>
        _versions.Count == 0
            ? null
            : _versions.MaxBy(version => version.VersionNumber);
    public IReadOnlyList<DomainEvent> DomainEvents => _domainEvents;

    private DocumentType(
        DocumentTypeId id,
        string name,
        string code,
        DateTimeOffset createdAt)
    {
        Id = id;
        Name = name;
        Code = code;
        CreatedAt = createdAt;
        State = DocumentTypeState.Active;
    }

    public static Result<DocumentType> Create(string name, string code, IClock clock)
    {
        ArgumentNullException.ThrowIfNull(clock);

        if (string.IsNullOrWhiteSpace(name))
        {
            return Result<DocumentType>.Failure(DocumentTypeErrors.NameRequired);
        }

        if (string.IsNullOrWhiteSpace(code))
        {
            return Result<DocumentType>.Failure(DocumentTypeErrors.CodeRequired);
        }

        var createdAt = clock.UtcNow;
        var documentTypeId = DocumentTypeId.New();
        var documentType = new DocumentType(documentTypeId, name, code, createdAt);

        documentType._domainEvents.Add(new DocumentTypeCreated(
            Guid.NewGuid(),
            createdAt,
            CorrelationId.New(),
            documentTypeId,
            name,
            code));

        return Result<DocumentType>.Success(documentType);
    }

    public Result<DocumentTypeVersion> CreateVersion(IClock clock) =>
        CreateVersion(NextVersionNumber(), clock, VersionSchemaComposition.Empty);

    public Result<DocumentTypeVersion> CreateVersion(IClock clock, VersionSchemaComposition composition) =>
        CreateVersion(NextVersionNumber(), clock, composition);

    public Result<DocumentTypeVersion> CreateVersion(
        IClock clock,
        KeywordRegistry registry,
        IReadOnlyList<VersionSchemaFieldDefinition> fieldDefinitions) =>
        CreateVersion(NextVersionNumber(), clock, registry, fieldDefinitions);

    public Result<DocumentTypeVersion> CreateVersion(
        IClock clock,
        KeywordRegistry registry,
        IReadOnlyList<KeywordGroup> keywordGroups,
        IReadOnlyList<VersionSchemaFieldDefinition> fieldDefinitions,
        IReadOnlyList<VersionSchemaGroupAssignmentDefinition> groupDefinitions) =>
        CreateVersion(NextVersionNumber(), clock, registry, keywordGroups, fieldDefinitions, groupDefinitions);

    public Result<DocumentTypeVersion> CreateVersion(
        IClock clock,
        KeywordRegistry registry,
        IReadOnlyList<VersionSchemaFieldDefinition> fieldDefinitions,
        IReadOnlyList<LayoutSectionDefinition> layoutSections) =>
        CreateVersion(NextVersionNumber(), clock, registry, fieldDefinitions, layoutSections);

    public Result<DocumentTypeVersion> CreateVersion(
        IClock clock,
        KeywordRegistry registry,
        IReadOnlyList<KeywordGroup> keywordGroups,
        IReadOnlyList<VersionSchemaFieldDefinition> fieldDefinitions,
        IReadOnlyList<VersionSchemaGroupAssignmentDefinition> groupDefinitions,
        IReadOnlyList<LayoutSectionDefinition> layoutSections) =>
        CreateVersion(
            NextVersionNumber(),
            clock,
            registry,
            keywordGroups,
            fieldDefinitions,
            groupDefinitions,
            layoutSections);

    public Result<DocumentTypeVersion> CreateVersion(int versionNumber, IClock clock) =>
        CreateVersion(versionNumber, clock, VersionSchemaComposition.Empty);

    public Result<DocumentTypeVersion> CreateVersion(
        int versionNumber,
        IClock clock,
        VersionSchemaComposition composition)
    {
        ArgumentNullException.ThrowIfNull(clock);
        ArgumentNullException.ThrowIfNull(composition);

        if (versionNumber <= 0)
        {
            return Result<DocumentTypeVersion>.Failure(DocumentTypeErrors.VersionNumberInvalid);
        }

        if (_versions.Any(version => version.VersionNumber == versionNumber))
        {
            return Result<DocumentTypeVersion>.Failure(DocumentTypeErrors.DuplicateVersionNumber);
        }

        return CreateVersion(versionNumber, clock, composition, layoutSections: null);
    }

    public Result<DocumentTypeVersion> CreateVersion(
        int versionNumber,
        IClock clock,
        KeywordRegistry registry,
        IReadOnlyList<VersionSchemaFieldDefinition> fieldDefinitions)
    {
        ArgumentNullException.ThrowIfNull(registry);
        ArgumentNullException.ThrowIfNull(fieldDefinitions);

        var compositionResult = VersionSchemaComposition.FromDefinitions(registry, fieldDefinitions);
        if (!compositionResult.IsSuccess)
        {
            return Result<DocumentTypeVersion>.Failure(compositionResult.Error.Value);
        }

        return CreateVersion(versionNumber, clock, compositionResult.Value!, layoutSections: null);
    }

    public Result<DocumentTypeVersion> CreateVersion(
        int versionNumber,
        IClock clock,
        KeywordRegistry registry,
        IReadOnlyList<VersionSchemaFieldDefinition> fieldDefinitions,
        IReadOnlyList<LayoutSectionDefinition> layoutSections)
    {
        ArgumentNullException.ThrowIfNull(registry);
        ArgumentNullException.ThrowIfNull(fieldDefinitions);
        ArgumentNullException.ThrowIfNull(layoutSections);

        var compositionResult = VersionSchemaComposition.FromDefinitions(registry, fieldDefinitions);
        if (!compositionResult.IsSuccess)
        {
            return Result<DocumentTypeVersion>.Failure(compositionResult.Error.Value);
        }

        return CreateVersion(versionNumber, clock, compositionResult.Value!, layoutSections);
    }

    public Result<DocumentTypeVersion> CreateVersion(
        int versionNumber,
        IClock clock,
        KeywordRegistry registry,
        IReadOnlyList<KeywordGroup> keywordGroups,
        IReadOnlyList<VersionSchemaFieldDefinition> fieldDefinitions,
        IReadOnlyList<VersionSchemaGroupAssignmentDefinition> groupDefinitions)
    {
        ArgumentNullException.ThrowIfNull(registry);
        ArgumentNullException.ThrowIfNull(keywordGroups);
        ArgumentNullException.ThrowIfNull(fieldDefinitions);
        ArgumentNullException.ThrowIfNull(groupDefinitions);

        var compositionResult = VersionSchemaComposition.FromDefinitions(
            registry,
            keywordGroups,
            fieldDefinitions,
            groupDefinitions);
        if (!compositionResult.IsSuccess)
        {
            return Result<DocumentTypeVersion>.Failure(compositionResult.Error.Value);
        }

        return CreateVersion(versionNumber, clock, compositionResult.Value!, layoutSections: null);
    }

    public Result<DocumentTypeVersion> CreateVersion(
        int versionNumber,
        IClock clock,
        KeywordRegistry registry,
        IReadOnlyList<KeywordGroup> keywordGroups,
        IReadOnlyList<VersionSchemaFieldDefinition> fieldDefinitions,
        IReadOnlyList<VersionSchemaGroupAssignmentDefinition> groupDefinitions,
        IReadOnlyList<LayoutSectionDefinition> layoutSections)
    {
        ArgumentNullException.ThrowIfNull(registry);
        ArgumentNullException.ThrowIfNull(keywordGroups);
        ArgumentNullException.ThrowIfNull(fieldDefinitions);
        ArgumentNullException.ThrowIfNull(groupDefinitions);
        ArgumentNullException.ThrowIfNull(layoutSections);

        var compositionResult = VersionSchemaComposition.FromDefinitions(
            registry,
            keywordGroups,
            fieldDefinitions,
            groupDefinitions);
        if (!compositionResult.IsSuccess)
        {
            return Result<DocumentTypeVersion>.Failure(compositionResult.Error.Value);
        }

        return CreateVersion(versionNumber, clock, compositionResult.Value!, layoutSections);
    }

    public Result<DocumentTypeVersion> CreateVersion(
        int versionNumber,
        IClock clock,
        VersionSchemaComposition composition,
        IReadOnlyList<LayoutSectionDefinition>? layoutSections)
    {
        ArgumentNullException.ThrowIfNull(clock);
        ArgumentNullException.ThrowIfNull(composition);

        if (versionNumber <= 0)
        {
            return Result<DocumentTypeVersion>.Failure(DocumentTypeErrors.VersionNumberInvalid);
        }

        if (_versions.Any(version => version.VersionNumber == versionNumber))
        {
            return Result<DocumentTypeVersion>.Failure(DocumentTypeErrors.DuplicateVersionNumber);
        }

        var layoutResult = layoutSections is null
            ? LayoutSchema.CreateDefault(composition)
            : LayoutSchema.FromDefinitions(composition, layoutSections);
        if (!layoutResult.IsSuccess)
        {
            return Result<DocumentTypeVersion>.Failure(layoutResult.Error.Value);
        }

        return AddVersion(versionNumber, clock, composition, layoutResult.Value!);
    }

    public Result Activate(IClock clock)
    {
        ArgumentNullException.ThrowIfNull(clock);

        if (State == DocumentTypeState.Active)
        {
            return Result.Failure(DocumentTypeErrors.AlreadyActive);
        }

        State = DocumentTypeState.Active;
        return Result.Success();
    }

    public Result Deactivate(IClock clock)
    {
        ArgumentNullException.ThrowIfNull(clock);

        if (State == DocumentTypeState.Inactive)
        {
            return Result.Failure(DocumentTypeErrors.AlreadyInactive);
        }

        State = DocumentTypeState.Inactive;
        return Result.Success();
    }

    public SchemaValidationResult ValidateMetadataAgainstVersion(
        DocumentTypeVersionId versionId,
        KeywordRegistry registry,
        VersionMetadataPayload payload)
    {
        ArgumentNullException.ThrowIfNull(registry);
        ArgumentNullException.ThrowIfNull(payload);

        var version = _versions.FirstOrDefault(existing => existing.Id == versionId);
        if (version is null)
        {
            return SchemaValidationResult.Invalid([SchemaValidationErrors.VersionNotFound]);
        }

        return VersionSchemaValidator.Validate(registry, version, payload);
    }

    private int NextVersionNumber() =>
        _versions.Count == 0 ? 1 : _versions.Max(version => version.VersionNumber) + 1;

    private Result<DocumentTypeVersion> AddVersion(
        int versionNumber,
        IClock clock,
        VersionSchemaComposition composition,
        LayoutSchema layoutSchema)
    {
        var createdAt = clock.UtcNow;
        var versionId = DocumentTypeVersionId.New();
        var version = new DocumentTypeVersion(versionId, versionNumber, createdAt, composition, layoutSchema);
        _versions.Add(version);

        _domainEvents.Add(new DocumentTypeVersionCreated(
            Guid.NewGuid(),
            createdAt,
            CorrelationId.New(),
            Id,
            versionId,
            versionNumber,
            composition.FieldSnapshots,
            composition.GroupSnapshots,
            layoutSchema.SectionSnapshots));

        return Result<DocumentTypeVersion>.Success(version);
    }
}
