using AIX.Documents.Domain;
using AIX.Documents.Events;
using FluentAssertions;

namespace AIX.Documents.Tests;

public class CreateDocumentTests
{
    private static readonly Guid ValidDocumentTypeId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid ValidDocumentTypeVersionId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid ValidTaxonomyNodeId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    private static readonly Guid ValidUserId = Guid.Parse("44444444-4444-4444-4444-444444444444");

    [Fact]
    public void creates_document_successfully()
    {
        var clock = new FakeClock();
        var documentTypeId = new DocumentTypeId(ValidDocumentTypeId);
        var documentTypeVersionId = new DocumentTypeVersionId(ValidDocumentTypeVersionId);
        var taxonomyNodeId = new TaxonomyNodeId(ValidTaxonomyNodeId);
        var createdBy = new UserId(ValidUserId);

        var result = Document.Create(
            documentTypeId,
            documentTypeVersionId,
            taxonomyNodeId,
            createdBy,
            clock);

        result.IsSuccess.Should().BeTrue();
        var document = result.Value!;
        document.Id.Value.Should().NotBe(Guid.Empty);
        document.DocumentTypeId.Should().Be(documentTypeId);
        document.DocumentTypeVersionId.Should().Be(documentTypeVersionId);
        document.TaxonomyNodeId.Should().Be(taxonomyNodeId);
        document.CreatedBy.Should().Be(createdBy);
        document.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<DocumentCreated>();
    }

    [Fact]
    public void document_initial_state_is_draft()
    {
        var document = CreateDocument();

        document.State.Should().Be(DocumentState.Draft);
    }

    [Fact]
    public void document_has_created_at_from_clock()
    {
        var expectedCreatedAt = new DateTimeOffset(2026, 1, 15, 8, 30, 0, TimeSpan.Zero);
        var clock = new FakeClock { UtcNow = expectedCreatedAt };

        var document = CreateDocument(clock);

        document.CreatedAt.Should().Be(expectedCreatedAt);
    }

    [Fact]
    public void document_requires_document_type()
    {
        var clock = new FakeClock();
        var documentTypeId = new DocumentTypeId(Guid.Empty);

        var result = Document.Create(
            documentTypeId,
            new DocumentTypeVersionId(ValidDocumentTypeVersionId),
            new TaxonomyNodeId(ValidTaxonomyNodeId),
            new UserId(ValidUserId),
            clock);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(DocumentErrors.DocumentTypeRequired);
    }

    [Fact]
    public void document_requires_document_type_version()
    {
        var clock = new FakeClock();
        var documentTypeVersionId = new DocumentTypeVersionId(Guid.Empty);

        var result = Document.Create(
            new DocumentTypeId(ValidDocumentTypeId),
            documentTypeVersionId,
            new TaxonomyNodeId(ValidTaxonomyNodeId),
            new UserId(ValidUserId),
            clock);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(DocumentErrors.DocumentTypeVersionRequired);
    }

    [Fact]
    public void document_type_cannot_change()
    {
        var document = CreateDocument();

        typeof(Document).GetProperty(nameof(Document.DocumentTypeId))!
            .CanWrite.Should().BeFalse();
        document.DocumentTypeId.Should().Be(new DocumentTypeId(ValidDocumentTypeId));
    }

    [Fact]
    public void document_type_version_cannot_change()
    {
        var document = CreateDocument();

        typeof(Document).GetProperty(nameof(Document.DocumentTypeVersionId))!
            .CanWrite.Should().BeFalse();
        document.DocumentTypeVersionId.Should().Be(new DocumentTypeVersionId(ValidDocumentTypeVersionId));
    }

    private static Document CreateDocument(FakeClock? clock = null)
    {
        clock ??= new FakeClock();

        var result = Document.Create(
            new DocumentTypeId(ValidDocumentTypeId),
            new DocumentTypeVersionId(ValidDocumentTypeVersionId),
            new TaxonomyNodeId(ValidTaxonomyNodeId),
            new UserId(ValidUserId),
            clock);

        result.IsSuccess.Should().BeTrue();
        return result.Value!;
    }
}
