using AIX.Metadata.Domain;
using AIX.Metadata.Events;
using FluentAssertions;

namespace AIX.Metadata.Tests;

public class CreateDocumentTypeTests
{
    [Fact]
    public void creates_document_type_successfully()
    {
        var clock = new FakeClock();

        var result = DocumentType.Create("Invoice", "INV", clock);

        result.IsSuccess.Should().BeTrue();
        var documentType = result.Value!;
        documentType.Id.Value.Should().NotBe(Guid.Empty);
        documentType.Name.Should().Be("Invoice");
        documentType.Code.Should().Be("INV");
        documentType.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<DocumentTypeCreated>();
    }

    [Fact]
    public void document_type_initial_state_is_active()
    {
        var documentType = CreateDocumentType();

        documentType.State.Should().Be(DocumentTypeState.Active);
    }

    [Fact]
    public void document_type_has_created_at_from_clock()
    {
        var expectedCreatedAt = new DateTimeOffset(2026, 3, 1, 9, 0, 0, TimeSpan.Zero);
        var clock = new FakeClock { UtcNow = expectedCreatedAt };

        var documentType = CreateDocumentType(clock);

        documentType.CreatedAt.Should().Be(expectedCreatedAt);
    }

    [Fact]
    public void document_type_requires_name()
    {
        var clock = new FakeClock();

        var result = DocumentType.Create("   ", "INV", clock);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(DocumentTypeErrors.NameRequired);
    }

    [Fact]
    public void document_type_requires_code()
    {
        var clock = new FakeClock();

        var result = DocumentType.Create("Invoice", "", clock);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(DocumentTypeErrors.CodeRequired);
    }

    [Fact]
    public void document_type_created_event_contains_identity_and_metadata()
    {
        var clock = new FakeClock { UtcNow = new DateTimeOffset(2026, 5, 17, 14, 0, 0, TimeSpan.Zero) };

        var result = DocumentType.Create("Invoice", "INV", clock);
        var documentType = result.Value!;
        var created = documentType.DomainEvents.Single().Should().BeOfType<DocumentTypeCreated>().Subject;

        created.DocumentTypeId.Should().Be(documentType.Id);
        created.Name.Should().Be("Invoice");
        created.Code.Should().Be("INV");
        created.OccurredOn.Should().Be(clock.UtcNow);
    }

    private static DocumentType CreateDocumentType(FakeClock? clock = null)
    {
        clock ??= new FakeClock();

        var result = DocumentType.Create("Invoice", "INV", clock);
        result.IsSuccess.Should().BeTrue();
        return result.Value!;
    }
}
