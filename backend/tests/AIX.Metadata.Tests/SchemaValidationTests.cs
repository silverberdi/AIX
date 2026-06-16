using AIX.Metadata.Domain;
using FluentAssertions;

namespace AIX.Metadata.Tests;

public class SchemaValidationTests
{
    [Fact]
    public void validates_complete_metadata_payload_successfully()
    {
        var documentType = CreateDocumentType();
        var registry = CreateRegistryWithKeywords();
        var clock = new FakeClock();
        var version = documentType.CreateVersion(
            clock,
            registry,
            fieldDefinitions:
            [
                new VersionSchemaFieldDefinition(
                    KeywordIdFor(registry, "vendor_name"),
                    FieldCatalogType.Text,
                    IsRequired: true),
                new VersionSchemaFieldDefinition(
                    KeywordIdFor(registry, "amount"),
                    FieldCatalogType.Number)
            ]).Value!;

        var payload = new VersionMetadataPayload(
            standaloneValues: new Dictionary<string, string?>
            {
                ["vendor_name"] = "Acme Corp",
                ["amount"] = "100.50"
            });

        var result = documentType.ValidateMetadataAgainstVersion(version.Id, registry, payload);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void fails_missing_required_field_from_schema()
    {
        var documentType = CreateDocumentType();
        var registry = CreateRegistryWithKeywords();
        var clock = new FakeClock();
        var version = documentType.CreateVersion(
            clock,
            registry,
            [
                new VersionSchemaFieldDefinition(
                    KeywordIdFor(registry, "vendor_name"),
                    FieldCatalogType.Text,
                    IsRequired: true)
            ]).Value!;

        var result = documentType.ValidateMetadataAgainstVersion(
            version.Id,
            registry,
            new VersionMetadataPayload());

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(KeywordValidationErrors.ValueRequired);
    }

    [Fact]
    public void fails_missing_required_keyword_group_instance()
    {
        var documentType = CreateDocumentType();
        var registry = CreateRegistryWithKeywords();
        var group = CreateRequiredHeaderGroup(registry);
        var clock = new FakeClock();
        var version = documentType.CreateVersion(
            clock,
            registry,
            [group],
            fieldDefinitions: [],
            groupDefinitions:
            [
                new VersionSchemaGroupAssignmentDefinition(group.Id, Order: 1, InstanceKey: "hdr")
            ]).Value!;

        var result = documentType.ValidateMetadataAgainstVersion(
            version.Id,
            registry,
            new VersionMetadataPayload());

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(SchemaValidationErrors.MissingRequiredGroupInstance);
    }

    [Fact]
    public void fails_invalid_keyword_value_delegates_to_keyword_validator()
    {
        var documentType = CreateDocumentType();
        var registry = CreateRegistryWithKeywords();
        var clock = new FakeClock();
        var version = documentType.CreateVersion(
            clock,
            registry,
            [
                new VersionSchemaFieldDefinition(
                    KeywordIdFor(registry, "amount"),
                    FieldCatalogType.Number,
                    IsRequired: true)
            ]).Value!;

        var result = documentType.ValidateMetadataAgainstVersion(
            version.Id,
            registry,
            new VersionMetadataPayload(
                standaloneValues: new Dictionary<string, string?> { ["amount"] = "not-a-number" }));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(KeywordValidationErrors.InvalidNumber);
    }

    [Fact]
    public void fails_unknown_metadata_key()
    {
        var documentType = CreateDocumentType();
        var registry = CreateRegistryWithKeywords();
        var clock = new FakeClock();
        var version = documentType.CreateVersion(
            clock,
            registry,
            [
                new VersionSchemaFieldDefinition(
                    KeywordIdFor(registry, "vendor_name"),
                    FieldCatalogType.Text)
            ]).Value!;

        var result = documentType.ValidateMetadataAgainstVersion(
            version.Id,
            registry,
            new VersionMetadataPayload(
                standaloneValues: new Dictionary<string, string?> { ["unknown_field"] = "x" }));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(SchemaValidationErrors.UnknownMetadataKey);
    }

    [Fact]
    public void rejects_hidden_field_on_capture()
    {
        var documentType = CreateDocumentType();
        var registry = CreateRegistryWithKeywords();
        var clock = new FakeClock();
        var version = documentType.CreateVersion(
            clock,
            registry,
            [
                new VersionSchemaFieldDefinition(
                    KeywordIdFor(registry, "vendor_name"),
                    FieldCatalogType.Text,
                    IsHidden: true)
            ]).Value!;

        var result = documentType.ValidateMetadataAgainstVersion(
            version.Id,
            registry,
            new VersionMetadataPayload(
                standaloneValues: new Dictionary<string, string?> { ["vendor_name"] = "Acme" }));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(SchemaValidationErrors.HiddenFieldNotAllowedOnCapture);
    }

    [Fact]
    public void returns_multiple_validation_errors_when_applicable()
    {
        var documentType = CreateDocumentType();
        var registry = CreateRegistryWithKeywords();
        var clock = new FakeClock();
        var version = documentType.CreateVersion(
            clock,
            registry,
            [
                new VersionSchemaFieldDefinition(
                    KeywordIdFor(registry, "vendor_name"),
                    FieldCatalogType.Text,
                    IsRequired: true),
                new VersionSchemaFieldDefinition(
                    KeywordIdFor(registry, "amount"),
                    FieldCatalogType.Number,
                    IsRequired: true)
            ]).Value!;

        var result = documentType.ValidateMetadataAgainstVersion(
            version.Id,
            registry,
            new VersionMetadataPayload(
                standaloneValues: new Dictionary<string, string?>
                {
                    ["vendor_name"] = "   ",
                    ["amount"] = "bad"
                }));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(KeywordValidationErrors.ValueRequired);
        result.Errors.Should().Contain(KeywordValidationErrors.InvalidNumber);
        result.Errors.Should().HaveCountGreaterThanOrEqualTo(2);
    }

    [Fact]
    public void repeatable_group_enforces_instance_rules_on_payload()
    {
        var documentType = CreateDocumentType();
        var registry = CreateRegistryWithKeywords();
        var group = CreateVendorDetailsGroup(registry);
        var clock = new FakeClock();
        var version = documentType.CreateVersion(
            clock,
            registry,
            [group],
            fieldDefinitions: [],
            groupDefinitions:
            [
                new VersionSchemaGroupAssignmentDefinition(group.Id, Order: 1, InstanceKey: "line-1"),
                new VersionSchemaGroupAssignmentDefinition(group.Id, Order: 2, InstanceKey: "line-2")
            ]).Value!;

        var unexpectedInstance = documentType.ValidateMetadataAgainstVersion(
            version.Id,
            registry,
            new VersionMetadataPayload(
                groupInstances:
                [
                    new MetadataGroupInstancePayload(
                        "vendor_details",
                        "line-99",
                        new Dictionary<string, string?> { ["vendor_name"] = "Acme" })
                ]));

        unexpectedInstance.IsValid.Should().BeFalse();
        unexpectedInstance.Errors.Should().Contain(SchemaValidationErrors.UnexpectedGroupInstance);

        var completePayload = new VersionMetadataPayload(
            groupInstances:
            [
                new MetadataGroupInstancePayload(
                    "vendor_details",
                    "line-1",
                    new Dictionary<string, string?>
                    {
                        ["vendor_name"] = "Acme",
                        ["vendor_tax_id"] = "TX-1"
                    }),
                new MetadataGroupInstancePayload(
                    "vendor_details",
                    "line-2",
                    new Dictionary<string, string?>
                    {
                        ["vendor_name"] = "Beta",
                        ["vendor_tax_id"] = "TX-2"
                    })
            ]);

        documentType.ValidateMetadataAgainstVersion(version.Id, registry, completePayload)
            .IsValid.Should().BeTrue();
    }

    [Fact]
    public void optional_standalone_field_allows_missing_or_empty_value()
    {
        var documentType = CreateDocumentType();
        var registry = CreateRegistryWithKeywords();
        var clock = new FakeClock();
        var version = documentType.CreateVersion(
            clock,
            registry,
            [
                new VersionSchemaFieldDefinition(
                    KeywordIdFor(registry, "vendor_name"),
                    FieldCatalogType.Text)
            ]).Value!;

        documentType.ValidateMetadataAgainstVersion(version.Id, registry, new VersionMetadataPayload())
            .IsValid.Should().BeTrue();

        documentType.ValidateMetadataAgainstVersion(
                version.Id,
                registry,
                new VersionMetadataPayload(
                    standaloneValues: new Dictionary<string, string?> { ["vendor_name"] = "" }))
            .IsValid.Should().BeTrue();
    }

    [Fact]
    public void required_group_validates_keyword_values_with_group_required_semantics()
    {
        var documentType = CreateDocumentType();
        var registry = CreateRegistryWithKeywords();
        var group = CreateRequiredHeaderGroup(registry);
        var clock = new FakeClock();
        var version = documentType.CreateVersion(
            clock,
            registry,
            [group],
            fieldDefinitions: [],
            groupDefinitions:
            [
                new VersionSchemaGroupAssignmentDefinition(group.Id, Order: 1)
            ]).Value!;

        var missingValue = documentType.ValidateMetadataAgainstVersion(
            version.Id,
            registry,
            new VersionMetadataPayload(
                groupInstances:
                [
                    new MetadataGroupInstancePayload(
                        "header",
                        null,
                        new Dictionary<string, string?>())
                ]));

        missingValue.IsValid.Should().BeFalse();
        missingValue.Errors.Should().Contain(KeywordValidationErrors.ValueRequired);

        var valid = documentType.ValidateMetadataAgainstVersion(
            version.Id,
            registry,
            new VersionMetadataPayload(
                groupInstances:
                [
                    new MetadataGroupInstancePayload(
                        "header",
                        null,
                        new Dictionary<string, string?> { ["vendor_name"] = "Acme" })
                ]));

        valid.IsValid.Should().BeTrue();
    }

    [Fact]
    public void rejects_group_keyword_submitted_as_standalone_value()
    {
        var documentType = CreateDocumentType();
        var registry = CreateRegistryWithKeywords();
        var group = CreateRequiredHeaderGroup(registry);
        var clock = new FakeClock();
        var version = documentType.CreateVersion(
            clock,
            registry,
            [group],
            fieldDefinitions: [],
            groupDefinitions:
            [
                new VersionSchemaGroupAssignmentDefinition(group.Id, Order: 1)
            ]).Value!;

        var result = documentType.ValidateMetadataAgainstVersion(
            version.Id,
            registry,
            new VersionMetadataPayload(
                standaloneValues: new Dictionary<string, string?> { ["vendor_name"] = "Acme" },
                groupInstances:
                [
                    new MetadataGroupInstancePayload(
                        "header",
                        null,
                        new Dictionary<string, string?> { ["vendor_name"] = "Acme" })
                ]));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(SchemaValidationErrors.GroupKeywordMustUseGroupPayload);
    }

    [Fact]
    public void validation_results_are_deterministic_for_same_input()
    {
        var documentType = CreateDocumentType();
        var registry = CreateRegistryWithKeywords();
        var clock = new FakeClock();
        var version = documentType.CreateVersion(
            clock,
            registry,
            [
                new VersionSchemaFieldDefinition(
                    KeywordIdFor(registry, "amount"),
                    FieldCatalogType.Number,
                    IsRequired: true),
                new VersionSchemaFieldDefinition(
                    KeywordIdFor(registry, "vendor_name"),
                    FieldCatalogType.Text,
                    IsRequired: true)
            ]).Value!;

        var payload = new VersionMetadataPayload(
            standaloneValues: new Dictionary<string, string?>
            {
                ["amount"] = "bad",
                ["vendor_name"] = "   "
            });

        var first = documentType.ValidateMetadataAgainstVersion(version.Id, registry, payload);
        var second = documentType.ValidateMetadataAgainstVersion(version.Id, registry, payload);

        first.Errors.Select(error => error.Code).Should().Equal(second.Errors.Select(error => error.Code));
    }

    [Fact]
    public void fails_when_version_id_is_unknown()
    {
        var documentType = CreateDocumentType();
        var registry = CreateRegistryWithKeywords();

        var result = documentType.ValidateMetadataAgainstVersion(
            DocumentTypeVersionId.New(),
            registry,
            new VersionMetadataPayload());

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle().Which.Should().Be(SchemaValidationErrors.VersionNotFound);
    }

    [Fact]
    public void slice_005_through_014_behavior_remains_valid()
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
