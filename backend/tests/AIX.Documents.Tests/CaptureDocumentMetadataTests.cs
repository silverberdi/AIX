using AIX.Documents.Contracts;
using AIX.Documents.Domain;
using AIX.Documents.Events;
using AIX.SharedKernel.Primitives;
using FluentAssertions;

namespace AIX.Documents.Tests;

public class CaptureDocumentMetadataTests
{
    private static readonly Guid ValidDocumentTypeId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid ValidDocumentTypeVersionId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid ValidTaxonomyNodeId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    private static readonly Guid ValidUserId = Guid.Parse("44444444-4444-4444-4444-444444444444");
    private static readonly Guid ValidFileId = Guid.Parse("55555555-5555-5555-5555-555555555555");

    [Fact]
    public void new_document_metadata_is_null_until_set()
    {
        var document = CreateDocument();

        document.CapturedMetadata.Should().BeNull();
    }

    [Fact]
    public void draft_accepts_first_metadata_capture()
    {
        var document = CreateDocument();
        var payload = CreatePayload(
            standaloneValues: new Dictionary<string, string?> { ["vendor_name"] = "Acme Corp" });

        var result = document.SetCapturedMetadata(payload, new FakeClock());

        result.IsSuccess.Should().BeTrue();
        document.CapturedMetadata.Should().NotBeNull();
        document.CapturedMetadata!.StandaloneValues.Should().ContainKey("vendor_name")
            .WhoseValue.Should().Be("Acme Corp");
        document.CapturedMetadata.GroupInstances.Should().BeEmpty();
    }

    [Fact]
    public void draft_replaces_existing_metadata()
    {
        var document = CreateDocument();
        var firstPayload = CreatePayload(
            standaloneValues: new Dictionary<string, string?> { ["vendor_name"] = "Acme Corp" });
        var secondPayload = CreatePayload(
            standaloneValues: new Dictionary<string, string?> { ["vendor_name"] = "Beta Inc" },
            groupInstances:
            [
                new CapturedMetadataGroupInstance(
                    "line_items",
                    "1",
                    new Dictionary<string, string?> { ["description"] = "Widget" })
            ]);

        document.SetCapturedMetadata(firstPayload, new FakeClock()).IsSuccess.Should().BeTrue();

        var result = document.SetCapturedMetadata(secondPayload, new FakeClock());

        result.IsSuccess.Should().BeTrue();
        document.CapturedMetadata!.StandaloneValues.Should().ContainKey("vendor_name")
            .WhoseValue.Should().Be("Beta Inc");
        document.CapturedMetadata.GroupInstances.Should().ContainSingle();
        document.CapturedMetadata.GroupInstances[0].GroupCode.Should().Be("line_items");
    }

    [Fact]
    public void complete_document_rejects_metadata_update()
    {
        var document = CreateDocument();
        var originalPayload = CreatePayload(
            standaloneValues: new Dictionary<string, string?> { ["vendor_name"] = "Acme Corp" });
        document.SetCapturedMetadata(originalPayload, new FakeClock()).IsSuccess.Should().BeTrue();

        var documentTypeId = document.DocumentTypeId;
        var documentTypeVersionId = document.DocumentTypeVersionId;
        document.Complete(new FakeClock()).IsSuccess.Should().BeTrue();

        var updatePayload = CreatePayload(
            standaloneValues: new Dictionary<string, string?> { ["vendor_name"] = "Changed Corp" });

        var result = document.SetCapturedMetadata(updatePayload, new FakeClock());

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(DocumentErrors.CannotModifyWhenComplete);
        document.CapturedMetadata!.StandaloneValues.Should().ContainKey("vendor_name")
            .WhoseValue.Should().Be("Acme Corp");
        document.DocumentTypeId.Should().Be(documentTypeId);
        document.DocumentTypeVersionId.Should().Be(documentTypeVersionId);
    }

    [Fact]
    public void metadata_capture_emits_domain_event()
    {
        var document = CreateDocument();
        var clock = new FakeClock();
        var payload = CreatePayload(
            standaloneValues: new Dictionary<string, string?> { ["vendor_name"] = "Acme Corp" });

        document.SetCapturedMetadata(payload, clock).IsSuccess.Should().BeTrue();

        document.DomainEvents.Should().Contain(e => e is DocumentMetadataCaptured);
        var captured = document.DomainEvents.OfType<DocumentMetadataCaptured>().Single();
        captured.DocumentId.Should().Be(document.Id);
        captured.StandaloneValues.Should().ContainKey("vendor_name").WhoseValue.Should().Be("Acme Corp");
        captured.GroupInstances.Should().BeEmpty();
        captured.OccurredOn.Should().Be(clock.UtcNow);
    }

    [Fact]
    public void metadata_update_preserves_type_version_binding()
    {
        var document = CreateDocument();
        var documentTypeId = document.DocumentTypeId;
        var documentTypeVersionId = document.DocumentTypeVersionId;
        var payload = CreatePayload(
            standaloneValues: new Dictionary<string, string?> { ["vendor_name"] = "Acme Corp" });

        document.SetCapturedMetadata(payload, new FakeClock()).IsSuccess.Should().BeTrue();

        document.DocumentTypeId.Should().Be(documentTypeId);
        document.DocumentTypeVersionId.Should().Be(documentTypeVersionId);
    }

    [Fact]
    public void complete_document_file_rules_still_enforced()
    {
        var document = CreateDocument();
        var metadataPayload = CreatePayload(
            standaloneValues: new Dictionary<string, string?> { ["vendor_name"] = "Acme Corp" });
        document.SetCapturedMetadata(metadataPayload, new FakeClock()).IsSuccess.Should().BeTrue();
        document.Complete(new FakeClock()).IsSuccess.Should().BeTrue();

        var result = document.AttachFile(
            new DocumentFileId(ValidFileId),
            DocumentFileRole.Primary,
            "invoice.pdf",
            "application/pdf",
            1024,
            new FakeClock());

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(DocumentErrors.CannotModifyWhenComplete);
        document.Files.Should().BeEmpty();
    }

    private static CapturedMetadataPayload CreatePayload(
        IReadOnlyDictionary<string, string?>? standaloneValues = null,
        IReadOnlyList<CapturedMetadataGroupInstance>? groupInstances = null) =>
        new(standaloneValues, groupInstances);

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
