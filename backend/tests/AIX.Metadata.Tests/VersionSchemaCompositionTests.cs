using AIX.Metadata.Domain;
using AIX.Metadata.Events;
using FluentAssertions;

namespace AIX.Metadata.Tests;

public class VersionSchemaCompositionTests
{
    [Fact]
    public void create_version_with_empty_composition_succeeds()
    {
        var documentType = CreateDocumentType();
        var clock = new FakeClock();

        var result = documentType.CreateVersion(clock);

        result.IsSuccess.Should().BeTrue();
        result.Value!.SchemaComposition.Fields.Should().BeEmpty();
    }

    [Fact]
    public void create_version_with_field_composition_succeeds()
    {
        var documentType = CreateDocumentType();
        var registry = CreateRegistryWithKeywords();
        var clock = new FakeClock();

        var result = documentType.CreateVersion(
            clock,
            registry,
            [
                new VersionSchemaFieldDefinition(
                    KeywordIdFor(registry, "vendor_name"),
                    FieldCatalogType.Text,
                    LabelOverride: "Vendor",
                    OrderHint: 1),
                new VersionSchemaFieldDefinition(
                    KeywordIdFor(registry, "amount"),
                    FieldCatalogType.Number,
                    OrderHint: 2)
            ]);

        result.IsSuccess.Should().BeTrue();
        var version = result.Value!;
        version.SchemaComposition.Fields.Should().HaveCount(2);
        version.SchemaComposition.Fields[0].KeywordId.Should().Be(KeywordIdFor(registry, "vendor_name"));
        version.SchemaComposition.Fields[1].CatalogType.Should().Be(FieldCatalogType.Number);
    }

    [Fact]
    public void prior_version_composition_unchanged_after_new_version()
    {
        var documentType = CreateDocumentType();
        var registry = CreateRegistryWithKeywords();
        var firstClock = new FakeClock { UtcNow = new DateTimeOffset(2026, 5, 1, 10, 0, 0, TimeSpan.Zero) };
        var first = documentType.CreateVersion(
            firstClock,
            registry,
            [new VersionSchemaFieldDefinition(KeywordIdFor(registry, "vendor_name"), FieldCatalogType.Text)]).Value!;
        var secondClock = new FakeClock { UtcNow = new DateTimeOffset(2026, 5, 2, 10, 0, 0, TimeSpan.Zero) };

        documentType.CreateVersion(
            secondClock,
            registry,
            [new VersionSchemaFieldDefinition(KeywordIdFor(registry, "amount"), FieldCatalogType.Number)])
            .IsSuccess.Should().BeTrue();

        documentType.Versions[0].SchemaComposition.Fields.Should().HaveCount(1);
        documentType.Versions[0].SchemaComposition.Fields[0].KeywordId.Should().Be(KeywordIdFor(registry, "vendor_name"));
        documentType.LatestVersion!.SchemaComposition.Fields.Should().HaveCount(1);
        documentType.LatestVersion.SchemaComposition.Fields[0].KeywordId.Should().Be(KeywordIdFor(registry, "amount"));
        first.SchemaComposition.Fields.Should().HaveCount(1);
    }

    [Fact]
    public void rejects_duplicate_field_in_composition()
    {
        var documentType = CreateDocumentType();
        var registry = CreateRegistryWithKeywords();
        var keywordId = KeywordIdFor(registry, "vendor_name");

        var result = documentType.CreateVersion(
            new FakeClock(),
            registry,
            [
                new VersionSchemaFieldDefinition(keywordId, FieldCatalogType.Text),
                new VersionSchemaFieldDefinition(keywordId, FieldCatalogType.TextArea)
            ]);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(VersionSchemaCompositionErrors.DuplicateKeyword);
    }

    [Fact]
    public void rejects_invalid_field_schema_in_composition()
    {
        var documentType = CreateDocumentType();
        var registry = CreateRegistryWithKeywords();

        var result = documentType.CreateVersion(
            new FakeClock(),
            registry,
            [new VersionSchemaFieldDefinition(KeywordIdFor(registry, "vendor_name"), FieldCatalogType.Number)]);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(FieldSchemaErrors.IncompatibleCatalogType);
        documentType.Versions.Should().BeEmpty();
    }

    [Fact]
    public void version_created_event_includes_composition_snapshot()
    {
        var documentType = CreateDocumentType();
        var registry = CreateRegistryWithKeywords();
        var clock = new FakeClock { UtcNow = new DateTimeOffset(2026, 5, 22, 12, 0, 0, TimeSpan.Zero) };

        var version = documentType.CreateVersion(
            clock,
            registry,
            [
                new VersionSchemaFieldDefinition(
                    KeywordIdFor(registry, "vendor_name"),
                    FieldCatalogType.Text,
                    OrderHint: 1)
            ]).Value!;

        var created = documentType.DomainEvents
            .OfType<DocumentTypeVersionCreated>()
            .Should()
            .ContainSingle()
            .Subject;

        created.FieldSnapshots.Should().ContainSingle();
        var snapshot = created.FieldSnapshots[0];
        snapshot.FieldSchemaId.Should().Be(version.SchemaComposition.Fields[0].Id);
        snapshot.KeywordId.Should().Be(KeywordIdFor(registry, "vendor_name"));
        snapshot.KeywordCode.Should().Be("vendor_name");
        snapshot.CatalogType.Should().Be(FieldCatalogType.Text);
    }

    [Fact]
    public void latest_version_reflects_new_composition()
    {
        var documentType = CreateDocumentType();
        var registry = CreateRegistryWithKeywords();
        documentType.CreateVersion(new FakeClock()).IsSuccess.Should().BeTrue();

        var latest = documentType.CreateVersion(
            new FakeClock(),
            registry,
            [new VersionSchemaFieldDefinition(KeywordIdFor(registry, "is_paid"), FieldCatalogType.Boolean)]).Value!;

        documentType.LatestVersion.Should().Be(latest);
        documentType.LatestVersion!.SchemaComposition.Fields.Should().ContainSingle();
    }

    [Fact]
    public void slice_005_through_010_behavior_remains_valid()
    {
        var clock = new FakeClock();
        DocumentType.Create("Invoice", "INV", clock).IsSuccess.Should().BeTrue();

        var registry = KeywordRegistry.Create(clock).Value!;
        registry.Register("amount", "Amount", KeywordDataType.Number, null, clock)
            .IsSuccess.Should().BeTrue();

        FieldSchema.Create(registry.Keywords.Single().Id, FieldCatalogType.Number, registry)
            .IsSuccess.Should().BeTrue();
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
        RegisterKeyword(registry, "amount", "Amount", KeywordDataType.Number, null, clock);
        RegisterKeyword(registry, "is_paid", "Is Paid", KeywordDataType.Boolean, null, clock);
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
}
