using AIX.Metadata.Domain;
using AIX.Metadata.Events;
using FluentAssertions;

namespace AIX.Metadata.Tests;

public class DocumentTypeVersioningTests
{
    [Fact]
    public void create_version_assigns_first_version_number()
    {
        var documentType = CreateDocumentType();
        var clock = new FakeClock();

        var result = documentType.CreateVersion(clock);

        result.IsSuccess.Should().BeTrue();
        result.Value!.VersionNumber.Should().Be(1);
        result.Value!.Id.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void create_version_auto_increments_version_number()
    {
        var documentType = CreateDocumentType();
        documentType.CreateVersion(new FakeClock()).IsSuccess.Should().BeTrue();

        var result = documentType.CreateVersion(new FakeClock());

        result.IsSuccess.Should().BeTrue();
        result.Value!.VersionNumber.Should().Be(2);
    }

    [Fact]
    public void previous_versions_remain_unchanged_when_new_version_created()
    {
        var documentType = CreateDocumentType();
        var firstClock = new FakeClock { UtcNow = new DateTimeOffset(2026, 5, 1, 10, 0, 0, TimeSpan.Zero) };
        var first = documentType.CreateVersion(firstClock).Value!;
        var secondClock = new FakeClock { UtcNow = new DateTimeOffset(2026, 5, 2, 10, 0, 0, TimeSpan.Zero) };

        documentType.CreateVersion(secondClock).IsSuccess.Should().BeTrue();

        documentType.Versions.Should().HaveCount(2);
        documentType.Versions[0].Should().Be(first);
        documentType.Versions[0].VersionNumber.Should().Be(1);
        documentType.Versions[0].CreatedAt.Should().Be(firstClock.UtcNow);
    }

    [Fact]
    public void latest_version_is_highest_version_number()
    {
        var documentType = CreateDocumentType();
        documentType.CreateVersion(new FakeClock()).IsSuccess.Should().BeTrue();
        var latest = documentType.CreateVersion(new FakeClock()).Value!;

        documentType.LatestVersion.Should().Be(latest);
        documentType.LatestVersion!.VersionNumber.Should().Be(2);
    }

    [Fact]
    public void create_version_with_explicit_number_succeeds()
    {
        var documentType = CreateDocumentType();

        var result = documentType.CreateVersion(3, new FakeClock());

        result.IsSuccess.Should().BeTrue();
        result.Value!.VersionNumber.Should().Be(3);
    }

    [Fact]
    public void duplicate_version_number_is_rejected()
    {
        var documentType = CreateDocumentType();
        documentType.CreateVersion(1, new FakeClock()).IsSuccess.Should().BeTrue();

        var result = documentType.CreateVersion(1, new FakeClock());

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(DocumentTypeErrors.DuplicateVersionNumber);
    }

    [Fact]
    public void invalid_version_number_is_rejected()
    {
        var documentType = CreateDocumentType();

        var result = documentType.CreateVersion(0, new FakeClock());

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(DocumentTypeErrors.VersionNumberInvalid);
    }

    [Fact]
    public void version_created_event_contains_identity_and_version_number()
    {
        var documentType = CreateDocumentType();
        var clock = new FakeClock { UtcNow = new DateTimeOffset(2026, 5, 17, 15, 0, 0, TimeSpan.Zero) };

        var version = documentType.CreateVersion(clock).Value!;
        var created = documentType.DomainEvents
            .OfType<DocumentTypeVersionCreated>()
            .Should()
            .ContainSingle()
            .Subject;

        created.DocumentTypeId.Should().Be(documentType.Id);
        created.DocumentTypeVersionId.Should().Be(version.Id);
        created.VersionNumber.Should().Be(1);
        created.OccurredOn.Should().Be(clock.UtcNow);
    }

    [Fact]
    public void slice_005_create_and_state_behavior_remain_valid()
    {
        var clock = new FakeClock();
        var createResult = DocumentType.Create("Invoice", "INV", clock);

        createResult.IsSuccess.Should().BeTrue();
        var documentType = createResult.Value!;
        documentType.State.Should().Be(DocumentTypeState.Active);
        documentType.Deactivate(clock).IsSuccess.Should().BeTrue();
        documentType.Activate(clock).IsSuccess.Should().BeTrue();
    }

    private static DocumentType CreateDocumentType()
    {
        var result = DocumentType.Create("Invoice", "INV", new FakeClock());
        result.IsSuccess.Should().BeTrue();
        return result.Value!;
    }
}
