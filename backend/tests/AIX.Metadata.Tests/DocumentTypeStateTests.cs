using AIX.Metadata.Domain;
using FluentAssertions;

namespace AIX.Metadata.Tests;

public class DocumentTypeStateTests
{
    [Fact]
    public void deactivate_transitions_active_to_inactive()
    {
        var documentType = CreateDocumentType();
        var clock = new FakeClock();

        var result = documentType.Deactivate(clock);

        result.IsSuccess.Should().BeTrue();
        documentType.State.Should().Be(DocumentTypeState.Inactive);
    }

    [Fact]
    public void deactivate_fails_when_already_inactive()
    {
        var documentType = CreateDocumentType();
        documentType.Deactivate(new FakeClock()).IsSuccess.Should().BeTrue();

        var result = documentType.Deactivate(new FakeClock());

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(DocumentTypeErrors.AlreadyInactive);
    }

    [Fact]
    public void activate_transitions_inactive_to_active()
    {
        var documentType = CreateDocumentType();
        documentType.Deactivate(new FakeClock()).IsSuccess.Should().BeTrue();
        var clock = new FakeClock();

        var result = documentType.Activate(clock);

        result.IsSuccess.Should().BeTrue();
        documentType.State.Should().Be(DocumentTypeState.Active);
    }

    [Fact]
    public void activate_fails_when_already_active()
    {
        var documentType = CreateDocumentType();

        var result = documentType.Activate(new FakeClock());

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(DocumentTypeErrors.AlreadyActive);
    }

    private static DocumentType CreateDocumentType()
    {
        var result = DocumentType.Create("Invoice", "INV", new FakeClock());
        result.IsSuccess.Should().BeTrue();
        return result.Value!;
    }
}
