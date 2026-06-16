using System.Reflection;
using AIX.Metadata.Domain;
using FluentAssertions;

namespace AIX.Metadata.Tests;

public class FieldSchemaTests
{
    [Fact]
    public void creates_field_schema_with_registered_keyword_successfully()
    {
        var registry = CreateRegistryWithKeywords();
        var keywordId = KeywordIdFor(registry, "vendor_name");

        var result = FieldSchema.Create(
            keywordId,
            FieldCatalogType.Text,
            registry,
            labelOverride: "Vendor",
            helpText: "Legal name of the vendor",
            orderHint: 1);

        result.IsSuccess.Should().BeTrue();
        var field = result.Value!;
        field.Id.Value.Should().NotBe(Guid.Empty);
        field.KeywordId.Should().Be(keywordId);
        field.CatalogType.Should().Be(FieldCatalogType.Text);
        field.LabelOverride.Should().Be("Vendor");
        field.HelpText.Should().Be("Legal name of the vendor");
        field.OrderHint.Should().Be(1);
        field.IsDeprecated.Should().BeFalse();
        field.IsHidden.Should().BeFalse();
        field.IsActive.Should().BeTrue();
    }

    [Fact]
    public void rejects_unregistered_keyword_id()
    {
        var registry = CreateRegistryWithKeywords();
        var unknownKeywordId = KeywordId.New();

        var result = FieldSchema.Create(unknownKeywordId, FieldCatalogType.Text, registry);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(FieldSchemaErrors.KeywordNotRegistered);
    }

    [Theory]
    [InlineData(KeywordDataType.Text, FieldCatalogType.Number)]
    [InlineData(KeywordDataType.Text, FieldCatalogType.Boolean)]
    [InlineData(KeywordDataType.Number, FieldCatalogType.Text)]
    [InlineData(KeywordDataType.Number, FieldCatalogType.Date)]
    [InlineData(KeywordDataType.Date, FieldCatalogType.TextArea)]
    [InlineData(KeywordDataType.Date, FieldCatalogType.Number)]
    [InlineData(KeywordDataType.Boolean, FieldCatalogType.Text)]
    [InlineData(KeywordDataType.Boolean, FieldCatalogType.Decimal)]
    public void rejects_incompatible_control_type_for_keyword_data_type(
        KeywordDataType dataType,
        FieldCatalogType catalogType)
    {
        var registry = CreateRegistryWithKeyword("sample", "Sample", dataType);
        var keywordId = KeywordIdFor(registry, "sample");

        var result = FieldSchema.Create(keywordId, catalogType, registry);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(FieldSchemaErrors.IncompatibleCatalogType);
    }

    [Theory]
    [InlineData(KeywordDataType.Text, FieldCatalogType.Text)]
    [InlineData(KeywordDataType.Text, FieldCatalogType.TextArea)]
    [InlineData(KeywordDataType.Number, FieldCatalogType.Number)]
    [InlineData(KeywordDataType.Number, FieldCatalogType.Decimal)]
    [InlineData(KeywordDataType.Date, FieldCatalogType.Date)]
    [InlineData(KeywordDataType.Date, FieldCatalogType.DateTime)]
    [InlineData(KeywordDataType.Boolean, FieldCatalogType.Boolean)]
    public void accepts_compatible_control_type_for_keyword_data_type(
        KeywordDataType dataType,
        FieldCatalogType catalogType)
    {
        var registry = CreateRegistryWithKeyword("sample", "Sample", dataType);
        var keywordId = KeywordIdFor(registry, "sample");

        var result = FieldSchema.Create(keywordId, catalogType, registry);

        result.IsSuccess.Should().BeTrue();
        result.Value!.CatalogType.Should().Be(catalogType);
    }

    [Fact]
    public void field_definition_is_immutable()
    {
        var registry = CreateRegistryWithKeywords();
        var field = FieldSchema.Create(
            KeywordIdFor(registry, "vendor_name"),
            FieldCatalogType.Text,
            registry).Value!;

        var publicMethodNames = typeof(FieldSchema)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Select(method => method.Name)
            .ToList();

        publicMethodNames.Should().NotContain("SetCatalogType");
        publicMethodNames.Should().NotContain("ChangeKeyword");
        publicMethodNames.Where(name => name.StartsWith("set_", StringComparison.Ordinal))
            .Should()
            .BeEmpty();

        field.Id.Should().Be(field.Id);
        field.KeywordId.Should().Be(KeywordIdFor(registry, "vendor_name"));
    }

    [Fact]
    public void deprecated_field_is_not_active()
    {
        var registry = CreateRegistryWithKeywords();

        var result = FieldSchema.Create(
            KeywordIdFor(registry, "vendor_name"),
            FieldCatalogType.Text,
            registry,
            isDeprecated: true);

        result.IsSuccess.Should().BeTrue();
        result.Value!.IsActive.Should().BeFalse();
    }

    [Fact]
    public void hidden_field_is_not_active()
    {
        var registry = CreateRegistryWithKeywords();

        var result = FieldSchema.Create(
            KeywordIdFor(registry, "vendor_name"),
            FieldCatalogType.Text,
            registry,
            isHidden: true);

        result.IsSuccess.Should().BeTrue();
        result.Value!.IsActive.Should().BeFalse();
    }

    [Fact]
    public void deprecate_or_hide_flag_prevents_active_inclusion()
    {
        var registry = CreateRegistryWithKeywords();
        var active = FieldSchema.Create(
            KeywordIdFor(registry, "vendor_name"),
            FieldCatalogType.Text,
            registry).Value!;
        var deprecated = FieldSchema.Create(
            KeywordIdFor(registry, "vendor_tax_id"),
            FieldCatalogType.Text,
            registry,
            isDeprecated: true).Value!;
        var hidden = FieldSchema.Create(
            KeywordIdFor(registry, "amount"),
            FieldCatalogType.Number,
            registry,
            isHidden: true).Value!;

        FieldSchema.SelectActive([active, deprecated, hidden])
            .Should()
            .ContainSingle()
            .Which
            .Should()
            .Be(active);
    }

    [Fact]
    public void rejects_negative_order_hint()
    {
        var registry = CreateRegistryWithKeywords();

        var result = FieldSchema.Create(
            KeywordIdFor(registry, "vendor_name"),
            FieldCatalogType.Text,
            registry,
            orderHint: -1);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(FieldSchemaErrors.OrderHintInvalid);
    }

    [Fact]
    public void slice_005_through_009_behavior_remains_valid()
    {
        var clock = new FakeClock();
        var documentType = DocumentType.Create("Invoice", "INV", clock).Value!;
        documentType.CreateVersion(clock).IsSuccess.Should().BeTrue();

        var registry = KeywordRegistry.Create(clock).Value!;
        registry.Register("amount", "Amount", KeywordDataType.Number, null, clock)
            .IsSuccess.Should().BeTrue();

        var group = KeywordGroup.Create(
            "header",
            "Header",
            isRepeatable: false,
            isRequired: true,
            [registry.Keywords.Single().Id],
            registry,
            clock);

        group.IsSuccess.Should().BeTrue();
        registry.ValidateKeywordValue(registry.Keywords.Single().Id, "42", isRequired: true)
            .IsSuccess.Should().BeTrue();
    }

    private static KeywordRegistry CreateRegistryWithKeywords()
    {
        var clock = new FakeClock();
        var registry = KeywordRegistry.Create(clock).Value!;
        RegisterKeyword(registry, "vendor_name", "Vendor Name", KeywordDataType.Text, 100, clock);
        RegisterKeyword(registry, "vendor_tax_id", "Vendor Tax Id", KeywordDataType.Text, 50, clock);
        RegisterKeyword(registry, "amount", "Amount", KeywordDataType.Number, null, clock);
        RegisterKeyword(registry, "invoice_date", "Invoice Date", KeywordDataType.Date, null, clock);
        RegisterKeyword(registry, "is_paid", "Is Paid", KeywordDataType.Boolean, null, clock);
        return registry;
    }

    private static KeywordRegistry CreateRegistryWithKeyword(
        string code,
        string name,
        KeywordDataType dataType)
    {
        var clock = new FakeClock();
        var registry = KeywordRegistry.Create(clock).Value!;
        RegisterKeyword(registry, code, name, dataType, dataType == KeywordDataType.Text ? 100 : null, clock);
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
