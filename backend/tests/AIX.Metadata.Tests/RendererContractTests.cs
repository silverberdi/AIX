using AIX.Metadata.Contracts;
using AIX.Metadata.Domain;
using AIX.SharedKernel.Primitives;
using FluentAssertions;

namespace AIX.Metadata.Tests;

public class RendererContractTests
{
    [Fact]
    public void generates_default_layout_for_field_only_composition()
    {
        var documentType = CreateDocumentType();
        var registry = CreateRegistryWithKeywords();
        var clock = new FakeClock();

        var result = documentType.CreateVersion(
            clock,
            registry,
            [new VersionSchemaFieldDefinition(KeywordIdFor(registry, "vendor_name"), FieldCatalogType.Text)]);

        result.IsSuccess.Should().BeTrue();
        var layout = result.Value!.LayoutSchema;
        layout.Sections.Should().ContainSingle();
        layout.Sections[0].Title.Should().Be("General");
        layout.Sections[0].Placements.Should().ContainSingle()
            .Which.Should().BeOfType<LayoutFieldPlacement>();
    }

    [Fact]
    public void generates_default_layout_including_keyword_groups()
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
        var sections = result.Value!.LayoutSchema.Sections;
        sections.Should().HaveCount(2);
        sections[0].Title.Should().Be("General");
        sections[0].Placements.Should().ContainSingle().Which.Should().BeOfType<LayoutFieldPlacement>();
        sections[1].Title.Should().Be("Vendor Details");
        sections[1].Placements.Should().HaveCount(2);
        sections[1].Placements.Should().AllBeOfType<LayoutGroupPlacement>();
    }

    [Fact]
    public void preserves_explicit_layout_when_provided()
    {
        var documentType = CreateDocumentType();
        var registry = CreateRegistryWithKeywords();
        var clock = new FakeClock();

        var version = documentType.CreateVersion(
            clock,
            registry,
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
            ]).Value!;

        var contract = DocumentSchemaProjector.ToContract(documentType, version.Id).Value!;
        contract.Layout.Sections.Should().ContainSingle();
        contract.Layout.Sections[0].Title.Should().Be("Capture");
    }

    [Fact]
    public void exports_document_schema_contract_for_version()
    {
        var documentType = CreateDocumentType();
        var registry = CreateRegistryWithKeywords();
        var group = CreateVendorDetailsGroup(registry);
        var clock = new FakeClock();

        var version = documentType.CreateVersion(
            clock,
            registry,
            [group],
            fieldDefinitions:
            [
                new VersionSchemaFieldDefinition(KeywordIdFor(registry, "amount"), FieldCatalogType.Number)
            ],
            groupDefinitions:
            [
                new VersionSchemaGroupAssignmentDefinition(group.Id, Order: 1, InstanceKey: "line-1")
            ]).Value!;

        var contract = DocumentSchemaProjector.ToContract(documentType, version.Id).Value!;

        contract.DocumentTypeId.Should().Be(documentType.Id.Value);
        contract.DocumentTypeCode.Should().Be("INV");
        contract.DocumentTypeName.Should().Be("Invoice");
        contract.DocumentTypeVersionId.Should().Be(version.Id.Value);
        contract.VersionNumber.Should().Be(1);
        contract.Fields.Should().ContainSingle();
        contract.Fields[0].KeywordCode.Should().Be("amount");
        contract.Fields[0].CatalogType.Should().Be(RendererFieldCatalogType.Number);
        contract.Fields[0].DatasetId.Should().BeNull();
        contract.Groups.Should().ContainSingle();
        contract.Groups[0].GroupCode.Should().Be("vendor_details");
        contract.Layout.Sections.Should().HaveCount(2);
    }

    [Fact]
    public void contract_includes_version_identity_for_renderer_binding()
    {
        var documentType = CreateDocumentType();
        var clock = new FakeClock();

        documentType.CreateVersion(clock).IsSuccess.Should().BeTrue();
        var version = documentType.CreateVersion(clock).Value!;

        var contract = DocumentSchemaProjector.ToContract(documentType, version.Id).Value!;

        contract.SchemaBindingKey.Should().Be("INV/v2");
        contract.VersionNumber.Should().Be(2);
        contract.DocumentTypeVersionId.Should().Be(version.Id.Value);
    }

    [Fact]
    public void contract_export_is_deterministic_for_same_domain_state()
    {
        var documentType = CreateDocumentType();
        var registry = CreateRegistryWithKeywords();
        var clock = new FakeClock();

        var version = documentType.CreateVersion(
            clock,
            registry,
            [new VersionSchemaFieldDefinition(KeywordIdFor(registry, "vendor_name"), FieldCatalogType.Text)])
            .Value!;

        var first = DocumentSchemaProjector.ToContract(documentType, version.Id).Value!;
        var second = DocumentSchemaProjector.ToContract(documentType, version.Id).Value!;

        first.Should().BeEquivalentTo(second);
    }

    [Fact]
    public void export_fails_when_version_not_found()
    {
        var documentType = CreateDocumentType();

        var result = DocumentSchemaProjector.ToContract(documentType, DocumentTypeVersionId.New());

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(DocumentSchemaProjectorErrors.VersionNotFound);
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
