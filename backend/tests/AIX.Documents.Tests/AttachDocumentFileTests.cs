using AIX.Documents.Domain;
using AIX.Documents.Events;
using AIX.SharedKernel.Primitives;
using FluentAssertions;

namespace AIX.Documents.Tests;

public class AttachDocumentFileTests
{
    private static readonly Guid ValidDocumentTypeId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid ValidDocumentTypeVersionId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid ValidTaxonomyNodeId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    private static readonly Guid ValidUserId = Guid.Parse("44444444-4444-4444-4444-444444444444");
    private static readonly Guid ValidFileId = Guid.Parse("55555555-5555-5555-5555-555555555555");
    private static readonly Guid AnotherFileId = Guid.Parse("66666666-6666-6666-6666-666666666666");

    [Fact]
    public void draft_document_can_attach_primary_file()
    {
        var document = CreateDocument();
        var fileId = new DocumentFileId(ValidFileId);

        var result = AttachPrimaryFile(document, fileId);

        result.IsSuccess.Should().BeTrue();
        document.Files.Should().ContainSingle()
            .Which.Should().BeEquivalentTo(new DocumentFile(
                fileId,
                DocumentFileRole.Primary,
                "invoice.pdf",
                "application/pdf",
                1024));
    }

    [Fact]
    public void draft_document_can_attach_supporting_file()
    {
        var document = CreateDocument();

        var result = AttachSupportingFile(document, new DocumentFileId(ValidFileId));

        result.IsSuccess.Should().BeTrue();
        document.Files.Should().ContainSingle()
            .Which.Role.Should().Be(DocumentFileRole.Supporting);
    }

    [Fact]
    public void draft_document_can_attach_multiple_supporting_files()
    {
        var document = CreateDocument();

        AttachSupportingFile(document, new DocumentFileId(ValidFileId)).IsSuccess.Should().BeTrue();
        var result = AttachSupportingFile(document, new DocumentFileId(AnotherFileId), "receipt.jpg", "image/jpeg");

        result.IsSuccess.Should().BeTrue();
        document.Files.Should().HaveCount(2);
        document.Files.Should().OnlyContain(f => f.Role == DocumentFileRole.Supporting);
    }

    [Fact]
    public void attach_file_requires_non_empty_file_id()
    {
        var document = CreateDocument();

        var result = document.AttachFile(
            new DocumentFileId(Guid.Empty),
            DocumentFileRole.Primary,
            "invoice.pdf",
            "application/pdf",
            1024,
            new FakeClock());

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(DocumentErrors.FileIdRequired);
    }

    [Fact]
    public void attach_file_requires_file_name()
    {
        var document = CreateDocument();

        var result = document.AttachFile(
            new DocumentFileId(ValidFileId),
            DocumentFileRole.Primary,
            "   ",
            "application/pdf",
            1024,
            new FakeClock());

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(DocumentErrors.FileNameRequired);
    }

    [Fact]
    public void attach_file_requires_content_type()
    {
        var document = CreateDocument();

        var result = document.AttachFile(
            new DocumentFileId(ValidFileId),
            DocumentFileRole.Primary,
            "invoice.pdf",
            "",
            1024,
            new FakeClock());

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(DocumentErrors.ContentTypeRequired);
    }

    [Fact]
    public void attach_file_requires_positive_size()
    {
        var document = CreateDocument();

        var result = document.AttachFile(
            new DocumentFileId(ValidFileId),
            DocumentFileRole.Primary,
            "invoice.pdf",
            "application/pdf",
            0,
            new FakeClock());

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(DocumentErrors.FileSizeInvalid);
    }

    [Fact]
    public void cannot_attach_duplicate_file_id()
    {
        var document = CreateDocument();
        var fileId = new DocumentFileId(ValidFileId);
        AttachPrimaryFile(document, fileId).IsSuccess.Should().BeTrue();

        var result = document.AttachFile(
            fileId,
            DocumentFileRole.Supporting,
            "duplicate.pdf",
            "application/pdf",
            512,
            new FakeClock());

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(DocumentErrors.FileAlreadyAttached);
    }

    [Fact]
    public void cannot_attach_second_primary_file()
    {
        var document = CreateDocument();
        AttachPrimaryFile(document, new DocumentFileId(ValidFileId)).IsSuccess.Should().BeTrue();

        var result = AttachPrimaryFile(document, new DocumentFileId(AnotherFileId), "second-primary.pdf");

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(DocumentErrors.PrimaryFileAlreadyExists);
    }

    [Fact]
    public void complete_document_cannot_attach_files()
    {
        var document = CreateCompleteDocument();

        var result = AttachPrimaryFile(document, new DocumentFileId(ValidFileId));

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(DocumentErrors.CannotModifyWhenComplete);
        document.Files.Should().BeEmpty();
    }

    [Fact]
    public void attach_file_emits_document_file_attached_event()
    {
        var document = CreateDocument();
        var clock = new FakeClock();
        var fileId = new DocumentFileId(ValidFileId);

        document.AttachFile(
            fileId,
            DocumentFileRole.Primary,
            "invoice.pdf",
            "application/pdf",
            1024,
            clock).IsSuccess.Should().BeTrue();

        document.DomainEvents.Should().Contain(e => e is DocumentFileAttached);
        var attached = document.DomainEvents.OfType<DocumentFileAttached>().Single();
        attached.DocumentId.Should().Be(document.Id);
        attached.FileId.Should().Be(fileId);
        attached.Role.Should().Be(DocumentFileRole.Primary);
    }

    [Fact]
    public void attach_preserves_prior_domain_events()
    {
        var document = CreateDocument();

        AttachPrimaryFile(document, new DocumentFileId(ValidFileId)).IsSuccess.Should().BeTrue();

        document.DomainEvents.Should().HaveCount(2);
        document.DomainEvents[0].Should().BeOfType<DocumentCreated>();
        document.DomainEvents[1].Should().BeOfType<DocumentFileAttached>();
    }

    [Fact]
    public void complete_document_remains_immutable_after_failed_file_attach()
    {
        var document = CreateCompleteDocument();

        AttachPrimaryFile(document, new DocumentFileId(ValidFileId));

        document.State.Should().Be(DocumentState.Complete);
        document.Files.Should().BeEmpty();
    }

    private static Result AttachPrimaryFile(
        Document document,
        DocumentFileId fileId,
        string fileName = "invoice.pdf",
        string contentType = "application/pdf",
        long sizeInBytes = 1024) =>
        document.AttachFile(fileId, DocumentFileRole.Primary, fileName, contentType, sizeInBytes, new FakeClock());

    private static Result AttachSupportingFile(
        Document document,
        DocumentFileId fileId,
        string fileName = "receipt.pdf",
        string contentType = "application/pdf",
        long sizeInBytes = 512) =>
        document.AttachFile(fileId, DocumentFileRole.Supporting, fileName, contentType, sizeInBytes, new FakeClock());

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

    private static Document CreateCompleteDocument()
    {
        var document = CreateDocument();
        document.Complete(new FakeClock()).IsSuccess.Should().BeTrue();
        return document;
    }
}
