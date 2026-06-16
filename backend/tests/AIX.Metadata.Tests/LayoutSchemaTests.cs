using AIX.Metadata.Domain;
using AIX.Metadata.Events;
using AIX.SharedKernel.Primitives;
using FluentAssertions;

namespace AIX.Metadata.Tests;

public class LayoutSchemaTests
{
    [Fact]
    public void create_version_with_layout_sections_successfully()
    {
        var registry = CreateRegistryWithKeywords();

        var result = BuildVersionWithExplicitLayout(
            registry,
            fieldDefinitions:
            [
                new VersionSchemaFieldDefinition(KeywordIdFor(registry, "vendor_name"), FieldCatalogType.Text),
                new VersionSchemaFieldDefinition(KeywordIdFor(registry, "amount"), FieldCatalogType.Number, OrderHint: 2)
            ],
            layoutSections:
            [
                new LayoutSectionDefinition(
                    "Main",
                    Order: 1,
                    Placements:
                    [
                        new LayoutFieldPlacementDefinition(KeywordIdFor(registry, "vendor_name"), Order: 2),
                        new LayoutFieldPlacementDefinition(KeywordIdFor(registry, "amount"), Order: 1)
                    ]),
                new LayoutSectionDefinition(
                    "Summary",
                    Order: 2,
                    Placements: [])
            ]);

        result.IsSuccess.Should().BeTrue();
        var layout = result.Value!.LayoutSchema;
        layout.Sections.Should().HaveCount(2);
        layout.Sections[0].Title.Should().Be("Main");
        layout.Sections[1].Title.Should().Be("Summary");
        layout.Sections[0].Placements.Should().HaveCount(2);
        layout.Sections[0].Placements[0].Should().BeOfType<LayoutFieldPlacement>()
            .Which.FieldSchemaId.Should().Be(
                result.Value.SchemaComposition.Fields.Single(field => field.KeywordId == KeywordIdFor(registry, "amount")).Id);
        layout.Sections[0].Placements[1].Should().BeOfType<LayoutFieldPlacement>()
            .Which.FieldSchemaId.Should().Be(
                result.Value.SchemaComposition.Fields.Single(field => field.KeywordId == KeywordIdFor(registry, "vendor_name")).Id);
    }

    [Fact]
    public void rejects_layout_reference_to_unknown_field()
    {
        var registry = CreateRegistryWithKeywords();
        var unknownKeywordId = KeywordId.New();

        var result = BuildVersionWithExplicitLayout(
            registry,
            fieldDefinitions:
            [
                new VersionSchemaFieldDefinition(KeywordIdFor(registry, "vendor_name"), FieldCatalogType.Text)
            ],
            layoutSections:
            [
                new LayoutSectionDefinition(
                    "Main",
                    Order: 1,
                    Placements: [new LayoutFieldPlacementDefinition(unknownKeywordId, Order: 1)])
            ]);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(LayoutSchemaErrors.UnknownFieldReference);
    }

    [Fact]
    public void rejects_layout_reference_to_unknown_group_instance()
    {
        var registry = CreateRegistryWithKeywords();
        var group = CreateVendorDetailsGroup(registry);

        var result = BuildVersionWithExplicitLayout(
            registry,
            keywordGroups: [group],
            fieldDefinitions: [],
            groupDefinitions:
            [
                new VersionSchemaGroupAssignmentDefinition(group.Id, Order: 1, InstanceKey: "line-1")
            ],
            layoutSections:
            [
                new LayoutSectionDefinition(
                    "Main",
                    Order: 1,
                    Placements:
                    [
                        new LayoutGroupPlacementDefinition(group.Id, Order: 1, InstanceKey: "missing-instance")
                    ])
            ]);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(LayoutSchemaErrors.UnknownGroupReference);
    }

    [Fact]
    public void rejects_duplicate_field_placement_in_layout()
    {
        var registry = CreateRegistryWithKeywords();
        var keywordId = KeywordIdFor(registry, "vendor_name");
        var composition = VersionSchemaComposition.FromDefinitions(
            registry,
            [new VersionSchemaFieldDefinition(keywordId, FieldCatalogType.Text)]).Value!;

        var result = LayoutSchema.FromDefinitions(
            composition,
            [
                new LayoutSectionDefinition(
                    "Main",
                    Order: 1,
                    Placements:
                    [
                        new LayoutFieldPlacementDefinition(keywordId, Order: 1),
                        new LayoutFieldPlacementDefinition(keywordId, Order: 2)
                    ])
            ]);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(LayoutSchemaErrors.DuplicateFieldPlacement);
    }

    [Fact]
    public void layout_immutable_on_published_version()
    {
        var registry = CreateRegistryWithKeywords();
        var firstClock = new FakeClock { UtcNow = new DateTimeOffset(2026, 5, 1, 10, 0, 0, TimeSpan.Zero) };
        var documentType = CreateDocumentType();

        var first = BuildVersionWithExplicitLayout(
            registry,
            clock: firstClock,
            documentType: documentType,
            fieldDefinitions:
            [
                new VersionSchemaFieldDefinition(KeywordIdFor(registry, "vendor_name"), FieldCatalogType.Text)
            ],
            layoutSections:
            [
                new LayoutSectionDefinition(
                    "First Layout",
                    Order: 1,
                    Placements: [])
            ]).Value!;

        documentType.CreateVersion(
            new FakeClock(),
            registry,
            fieldDefinitions:
            [
                new VersionSchemaFieldDefinition(KeywordIdFor(registry, "amount"), FieldCatalogType.Number)
            ],
            layoutSections:
            [
                new LayoutSectionDefinition(
                    "Second Layout",
                    Order: 1,
                    Placements:
                    [
                        new LayoutFieldPlacementDefinition(KeywordIdFor(registry, "amount"), Order: 1)
                    ])
            ]).IsSuccess.Should().BeTrue();

        first.LayoutSchema.Sections.Should().ContainSingle().Which.Title.Should().Be("First Layout");
        first.SchemaComposition.Fields.Should().ContainSingle();
        documentType.LatestVersion!.LayoutSchema.Sections[0].Title.Should().Be("Second Layout");
    }

    [Fact]
    public void default_layout_covers_all_composition_entries_when_not_explicit()
    {
        var documentType = CreateDocumentType();
        var registry = CreateRegistryWithKeywords();
        var group = CreateVendorDetailsGroup(registry);
        var clock = new FakeClock();

        var result = documentType.CreateVersion(
            clock,
            registry,
            [group],
            fieldDefinitions:
            [
                new VersionSchemaFieldDefinition(KeywordIdFor(registry, "amount"), FieldCatalogType.Number, OrderHint: 5)
            ],
            groupDefinitions:
            [
                new VersionSchemaGroupAssignmentDefinition(group.Id, Order: 2, InstanceKey: "line-1"),
                new VersionSchemaGroupAssignmentDefinition(group.Id, Order: 3, InstanceKey: "line-2")
            ]);

        result.IsSuccess.Should().BeTrue();
        var version = result.Value!;
        version.LayoutSchema.Sections.Should().HaveCount(2);
        version.LayoutSchema.Sections[0].Title.Should().Be(DefaultLayoutGenerator.GeneralSectionTitle);
        version.LayoutSchema.Sections[0].Placements.Should().ContainSingle()
            .Which.Should().BeOfType<LayoutFieldPlacement>();
        var groupSection = version.LayoutSchema.Sections[1];
        groupSection.Title.Should().Be("Vendor Details");
        groupSection.Placements.Should().HaveCount(2);
        groupSection.Placements[0].Should().BeOfType<LayoutGroupPlacement>()
            .Which.InstanceKey.Should().Be("line-1");
        groupSection.Placements[1].Should().BeOfType<LayoutGroupPlacement>()
            .Which.InstanceKey.Should().Be("line-2");
    }

    [Fact]
    public void section_order_is_stable()
    {
        var composition = VersionSchemaComposition.Empty;

        var result = LayoutSchema.FromDefinitions(
            composition,
            [
                new LayoutSectionDefinition("Zebra", Order: 30, Placements: []),
                new LayoutSectionDefinition("Alpha", Order: 10, Placements: []),
                new LayoutSectionDefinition("Middle", Order: 20, Placements: [])
            ]);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Sections.Select(section => section.Title)
            .Should().Equal("Alpha", "Middle", "Zebra");
        result.Value.Sections.Select(section => section.Order)
            .Should().Equal(10, 20, 30);
    }

    [Fact]
    public void version_created_event_includes_layout_section_snapshot()
    {
        var documentType = CreateDocumentType();
        var registry = CreateRegistryWithKeywords();
        var clock = new FakeClock { UtcNow = new DateTimeOffset(2026, 5, 23, 14, 0, 0, TimeSpan.Zero) };

        BuildVersionWithExplicitLayout(
            registry,
            clock: clock,
            documentType: documentType,
            fieldDefinitions:
            [
                new VersionSchemaFieldDefinition(KeywordIdFor(registry, "vendor_name"), FieldCatalogType.Text)
            ],
            layoutSections:
            [
                new LayoutSectionDefinition(
                    "Capture",
                    Order: 1,
                    Placements: [])
            ]).IsSuccess.Should().BeTrue();

        var created = documentType.DomainEvents
            .OfType<DocumentTypeVersionCreated>()
            .Should()
            .ContainSingle()
            .Subject;

        created.LayoutSnapshots.Should().ContainSingle();
        created.LayoutSnapshots[0].Title.Should().Be("Capture");
        created.LayoutSnapshots[0].Order.Should().Be(1);
    }

    [Fact]
    public void slice_005_through_012_behavior_remains_valid()
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
        documentType.LatestVersion!.LayoutSchema.Sections.Should().NotBeNull();
    }

    private static Result<DocumentTypeVersion> BuildVersionWithExplicitLayout(
        KeywordRegistry registry,
        IReadOnlyList<VersionSchemaFieldDefinition>? fieldDefinitions = null,
        IReadOnlyList<KeywordGroup>? keywordGroups = null,
        IReadOnlyList<VersionSchemaGroupAssignmentDefinition>? groupDefinitions = null,
        IReadOnlyList<LayoutSectionDefinition> layoutSections = null!,
        FakeClock? clock = null,
        DocumentType? documentType = null)
    {
        fieldDefinitions ??= [];
        keywordGroups ??= [];
        groupDefinitions ??= [];
        clock ??= new FakeClock();
        documentType ??= CreateDocumentType();

        if (keywordGroups.Count == 0 && groupDefinitions.Count == 0)
        {
            return documentType.CreateVersion(clock, registry, fieldDefinitions, layoutSections);
        }

        return documentType.CreateVersion(
            clock,
            registry,
            keywordGroups,
            fieldDefinitions,
            groupDefinitions,
            layoutSections);
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
}
