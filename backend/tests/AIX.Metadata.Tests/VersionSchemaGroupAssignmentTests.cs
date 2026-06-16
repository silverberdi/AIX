using AIX.Metadata.Domain;
using AIX.Metadata.Events;
using FluentAssertions;

namespace AIX.Metadata.Tests;

public class VersionSchemaGroupAssignmentTests
{
    [Fact]
    public void assign_keyword_group_to_version_successfully()
    {
        var documentType = CreateDocumentType();
        var registry = CreateRegistryWithKeywords();
        var group = CreateVendorDetailsGroup(registry);
        var clock = new FakeClock();

        var result = documentType.CreateVersion(
            clock,
            registry,
            [group],
            fieldDefinitions: [],
            groupDefinitions:
            [
                new VersionSchemaGroupAssignmentDefinition(group.Id, Order: 1)
            ]);

        result.IsSuccess.Should().BeTrue();
        var assignments = result.Value!.SchemaComposition.GroupAssignments;
        assignments.Should().ContainSingle();
        assignments[0].KeywordGroupId.Should().Be(group.Id);
        assignments[0].GroupCode.Should().Be("vendor_details");
        assignments[0].Order.Should().Be(1);
    }

    [Fact]
    public void rejects_unregistered_keyword_group_id()
    {
        var documentType = CreateDocumentType();
        var registry = CreateRegistryWithKeywords();
        var unknownGroupId = KeywordGroupId.New();

        var result = documentType.CreateVersion(
            new FakeClock(),
            registry,
            keywordGroups: [],
            fieldDefinitions: [],
            groupDefinitions:
            [
                new VersionSchemaGroupAssignmentDefinition(unknownGroupId, Order: 1)
            ]);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(VersionSchemaCompositionErrors.KeywordGroupNotFound);
        documentType.Versions.Should().BeEmpty();
    }

    [Fact]
    public void rejects_duplicate_group_placement_when_not_repeatable()
    {
        var documentType = CreateDocumentType();
        var registry = CreateRegistryWithKeywords();
        var group = CreateRequiredHeaderGroup(registry);

        var result = documentType.CreateVersion(
            new FakeClock(),
            registry,
            [group],
            fieldDefinitions: [],
            groupDefinitions:
            [
                new VersionSchemaGroupAssignmentDefinition(group.Id, Order: 1, InstanceKey: "a"),
                new VersionSchemaGroupAssignmentDefinition(group.Id, Order: 2, InstanceKey: "b")
            ]);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(VersionSchemaCompositionErrors.DuplicateGroupPlacement);
    }

    [Fact]
    public void repeatable_group_allows_multiple_instances_per_rules()
    {
        var documentType = CreateDocumentType();
        var registry = CreateRegistryWithKeywords();
        var group = CreateVendorDetailsGroup(registry);

        var success = documentType.CreateVersion(
            new FakeClock(),
            registry,
            [group],
            fieldDefinitions: [],
            groupDefinitions:
            [
                new VersionSchemaGroupAssignmentDefinition(group.Id, Order: 1, InstanceKey: "line-1"),
                new VersionSchemaGroupAssignmentDefinition(group.Id, Order: 2, InstanceKey: "line-2")
            ]);

        success.IsSuccess.Should().BeTrue();
        success.Value!.SchemaComposition.GroupAssignments.Should().HaveCount(2);
        success.Value.SchemaComposition.GroupAssignments.Select(a => a.InstanceKey)
            .Should().BeEquivalentTo("line-1", "line-2");

        var duplicateInstance = documentType.CreateVersion(
            new FakeClock(),
            registry,
            [group],
            fieldDefinitions: [],
            groupDefinitions:
            [
                new VersionSchemaGroupAssignmentDefinition(group.Id, Order: 1, InstanceKey: "line-1"),
                new VersionSchemaGroupAssignmentDefinition(group.Id, Order: 2, InstanceKey: "line-1")
            ]);

        duplicateInstance.IsSuccess.Should().BeFalse();
        duplicateInstance.Error.Should().Be(VersionSchemaCompositionErrors.DuplicateGroupInstanceKey);
    }

    [Fact]
    public void required_group_surfaces_required_in_version_schema()
    {
        var documentType = CreateDocumentType();
        var registry = CreateRegistryWithKeywords();
        var group = CreateRequiredHeaderGroup(registry);

        var result = documentType.CreateVersion(
            new FakeClock(),
            registry,
            [group],
            fieldDefinitions: [],
            groupDefinitions:
            [
                new VersionSchemaGroupAssignmentDefinition(group.Id, Order: 1)
            ]);

        result.IsSuccess.Should().BeTrue();
        result.Value!.SchemaComposition.GroupAssignments.Should().ContainSingle()
            .Which.IsRequired.Should().BeTrue();
    }

    [Fact]
    public void rejects_keyword_duplicated_in_group_and_standalone_field()
    {
        var documentType = CreateDocumentType();
        var registry = CreateRegistryWithKeywords();
        var group = CreateRequiredHeaderGroup(registry);
        var sharedKeywordId = KeywordIdFor(registry, "vendor_name");

        var result = documentType.CreateVersion(
            new FakeClock(),
            registry,
            [group],
            fieldDefinitions:
            [
                new VersionSchemaFieldDefinition(sharedKeywordId, FieldCatalogType.Text)
            ],
            groupDefinitions:
            [
                new VersionSchemaGroupAssignmentDefinition(group.Id, Order: 1)
            ]);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(VersionSchemaCompositionErrors.KeywordInGroupAndStandaloneField);
        documentType.Versions.Should().BeEmpty();
    }

    [Fact]
    public void prior_version_group_assignments_immutable()
    {
        var documentType = CreateDocumentType();
        var registry = CreateRegistryWithKeywords();
        var firstGroup = CreateVendorDetailsGroup(registry);
        var secondGroup = CreateRequiredHeaderGroup(registry);
        var firstClock = new FakeClock { UtcNow = new DateTimeOffset(2026, 5, 1, 10, 0, 0, TimeSpan.Zero) };

        var first = documentType.CreateVersion(
            firstClock,
            registry,
            [firstGroup],
            fieldDefinitions: [],
            groupDefinitions:
            [
                new VersionSchemaGroupAssignmentDefinition(firstGroup.Id, Order: 1)
            ]).Value!;

        documentType.CreateVersion(
            new FakeClock(),
            registry,
            [secondGroup],
            fieldDefinitions: [],
            groupDefinitions:
            [
                new VersionSchemaGroupAssignmentDefinition(secondGroup.Id, Order: 1)
            ]).IsSuccess.Should().BeTrue();

        first.SchemaComposition.GroupAssignments.Should().ContainSingle();
        first.SchemaComposition.GroupAssignments[0].KeywordGroupId.Should().Be(firstGroup.Id);
        documentType.Versions[0].SchemaComposition.GroupAssignments[0].GroupCode.Should().Be("vendor_details");
        documentType.LatestVersion!.SchemaComposition.GroupAssignments[0].GroupCode.Should().Be("header");
    }

    [Fact]
    public void version_created_event_includes_group_assignment_snapshot()
    {
        var documentType = CreateDocumentType();
        var registry = CreateRegistryWithKeywords();
        var group = CreateRequiredHeaderGroup(registry);
        var clock = new FakeClock { UtcNow = new DateTimeOffset(2026, 5, 23, 12, 0, 0, TimeSpan.Zero) };

        documentType.CreateVersion(
            clock,
            registry,
            [group],
            fieldDefinitions: [],
            groupDefinitions:
            [
                new VersionSchemaGroupAssignmentDefinition(group.Id, Order: 1, InstanceKey: "hdr")
            ]).IsSuccess.Should().BeTrue();

        var created = documentType.DomainEvents
            .OfType<DocumentTypeVersionCreated>()
            .Should()
            .ContainSingle()
            .Subject;

        created.GroupSnapshots.Should().ContainSingle();
        var snapshot = created.GroupSnapshots[0];
        snapshot.KeywordGroupId.Should().Be(group.Id);
        snapshot.GroupCode.Should().Be("header");
        snapshot.Order.Should().Be(1);
        snapshot.InstanceKey.Should().Be("hdr");
        snapshot.IsRequired.Should().BeTrue();
        snapshot.IsRepeatable.Should().BeFalse();
        snapshot.KeywordIds.Should().BeEquivalentTo(group.KeywordIds);
    }

    [Fact]
    public void slice_005_through_011_behavior_remains_valid()
    {
        var clock = new FakeClock();
        var documentType = DocumentType.Create("Invoice", "INV", clock).Value!;
        var registry = CreateRegistryWithKeywords();

        documentType.CreateVersion(
            clock,
            registry,
            [new VersionSchemaFieldDefinition(KeywordIdFor(registry, "amount"), FieldCatalogType.Number)])
            .IsSuccess.Should().BeTrue();

        documentType.CreateVersion(clock).IsSuccess.Should().BeTrue();
    }

    private static DocumentType CreateDocumentType()
    {
        var result = DocumentType.Create("Invoice", "INV", new FakeClock());
        result.IsSuccess.Should().BeTrue();
        return result.Value!;
    }

    private static KeywordRegistry CreateRegistryWithKeywords()
    {
        var clock = new FakeClock();
        var registry = KeywordRegistry.Create(clock).Value!;
        RegisterKeyword(registry, "vendor_name", "Vendor Name", KeywordDataType.Text, 100, clock);
        RegisterKeyword(registry, "vendor_tax_id", "Vendor Tax Id", KeywordDataType.Text, 50, clock);
        RegisterKeyword(registry, "amount", "Amount", KeywordDataType.Number, null, clock);
        return registry;
    }

    private static KeywordId KeywordIdFor(KeywordRegistry registry, string code) =>
        registry.Keywords.Single(keyword => keyword.Code == code).Id;

    private static void RegisterKeyword(
        KeywordRegistry registry,
        string code,
        string name,
        KeywordDataType dataType,
        int? maxLength,
        FakeClock clock)
    {
        registry.Register(code, name, dataType, maxLength, clock).IsSuccess.Should().BeTrue();
    }

    private static KeywordGroup CreateVendorDetailsGroup(KeywordRegistry registry) =>
        KeywordGroup.Create(
            "vendor_details",
            "Vendor Details",
            isRepeatable: true,
            isRequired: false,
            [KeywordIdFor(registry, "vendor_name"), KeywordIdFor(registry, "vendor_tax_id")],
            registry,
            new FakeClock()).Value!;

    private static KeywordGroup CreateRequiredHeaderGroup(KeywordRegistry registry) =>
        KeywordGroup.Create(
            "header",
            "Header",
            isRepeatable: false,
            isRequired: true,
            [KeywordIdFor(registry, "vendor_name")],
            registry,
            new FakeClock()).Value!;
}
