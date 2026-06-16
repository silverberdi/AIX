using AIX.Documents.Domain;
using AIX.Documents.Events;
using FluentAssertions;

namespace AIX.Documents.Tests;

public class CompleteDocumentTests
{
    private static readonly Guid ValidDocumentTypeId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid ValidDocumentTypeVersionId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid ValidTaxonomyNodeId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    private static readonly Guid ValidUserId = Guid.Parse("44444444-4444-4444-4444-444444444444");

    [Fact]
    public void draft_can_transition_to_complete()
    {
        var document = CreateDocument();
        var clock = new FakeClock();

        var result = document.Complete(clock);

        result.IsSuccess.Should().BeTrue();
        document.State.Should().Be(DocumentState.Complete);
    }

    [Fact]
    public void completing_already_complete_document_fails()
    {
        var document = CreateCompleteDocument();

        var result = document.Complete(new FakeClock());

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(DocumentErrors.AlreadyComplete);
    }

    [Fact]
    public void complete_emits_document_completed_event()
    {
        var document = CreateDocument();
        var clock = new FakeClock();

        document.Complete(clock);

        document.DomainEvents.Should().Contain(e => e is DocumentCompleted);
    }

    [Fact]
    public void complete_preserves_prior_domain_events()
    {
        var document = CreateDocument();

        document.Complete(new FakeClock());

        document.DomainEvents.Should().HaveCount(2);
        document.DomainEvents[0].Should().BeOfType<DocumentCreated>();
        document.DomainEvents[1].Should().BeOfType<DocumentCompleted>();
    }

    [Fact]
    public void complete_document_remains_complete_after_failed_transition()
    {
        var document = CreateCompleteDocument();

        document.Complete(new FakeClock());

        document.State.Should().Be(DocumentState.Complete);
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

    private static Document CreateCompleteDocument()
    {
        var document = CreateDocument();
        document.Complete(new FakeClock()).IsSuccess.Should().BeTrue();
        return document;
    }
}
