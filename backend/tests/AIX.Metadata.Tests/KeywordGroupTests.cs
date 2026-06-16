using AIX.Metadata.Domain;
using AIX.Metadata.Events;
using FluentAssertions;

namespace AIX.Metadata.Tests;

public class KeywordGroupTests
{
    [Fact]
    public void creates_keyword_group_successfully()
    {
        var registry = CreateRegistryWithKeywords();
        var clock = new FakeClock();

        var result = KeywordGroup.Create(
            "vendor_details",
            "Vendor Details",
            isRepeatable: true,
            isRequired: false,
            [KeywordIdFor(registry, "vendor_name"), KeywordIdFor(registry, "vendor_tax_id")],
            registry,
            clock);

        result.IsSuccess.Should().BeTrue();
        var group = result.Value!;
        group.Id.Value.Should().NotBe(Guid.Empty);
        group.Code.Should().Be("vendor_details");
        group.Name.Should().Be("Vendor Details");
        group.IsRepeatable.Should().BeTrue();
        group.IsRequired.Should().BeFalse();
        group.KeywordIds.Should().HaveCount(2);
    }

    [Fact]
    public void keyword_group_requires_code()
    {
        var registry = CreateRegistryWithKeywords();

        var result = KeywordGroup.Create(
            "   ",
            "Vendor Details",
            isRepeatable: false,
            isRequired: true,
            [KeywordIdFor(registry, "vendor_name")],
            registry,
            new FakeClock());

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(KeywordGroupErrors.CodeRequired);
    }

    [Fact]
    public void keyword_group_requires_name()
    {
        var registry = CreateRegistryWithKeywords();

        var result = KeywordGroup.Create(
            "vendor_details",
            "",
            isRepeatable: false,
            isRequired: true,
            [KeywordIdFor(registry, "vendor_name")],
            registry,
            new FakeClock());

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(KeywordGroupErrors.NameRequired);
    }

    [Fact]
    public void keyword_group_requires_at_least_one_keyword()
    {
        var registry = CreateRegistryWithKeywords();

        var result = KeywordGroup.Create(
            "vendor_details",
            "Vendor Details",
            isRepeatable: false,
            isRequired: true,
            [],
            registry,
            new FakeClock());

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(KeywordGroupErrors.KeywordsRequired);
    }

    [Fact]
    public void duplicate_keyword_references_inside_group_are_rejected()
    {
        var registry = CreateRegistryWithKeywords();
        var keywordId = KeywordIdFor(registry, "vendor_name");

        var result = KeywordGroup.Create(
            "vendor_details",
            "Vendor Details",
            isRepeatable: false,
            isRequired: true,
            [keywordId, keywordId],
            registry,
            new FakeClock());

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(KeywordGroupErrors.DuplicateKeyword);
    }

    [Fact]
    public void unregistered_keyword_reference_is_rejected()
    {
        var registry = CreateRegistryWithKeywords();
        var unknownKeywordId = KeywordId.New();

        var result = KeywordGroup.Create(
            "vendor_details",
            "Vendor Details",
            isRepeatable: false,
            isRequired: true,
            [unknownKeywordId],
            registry,
            new FakeClock());

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(KeywordGroupErrors.KeywordNotRegistered);
    }

    [Fact]
    public void keyword_group_references_only_registered_keywords()
    {
        var registry = CreateRegistryWithKeywords();
        var clock = new FakeClock();

        var result = KeywordGroup.Create(
            "vendor_details",
            "Vendor Details",
            isRepeatable: false,
            isRequired: true,
            [KeywordIdFor(registry, "vendor_name"), KeywordIdFor(registry, "vendor_tax_id")],
            registry,
            clock);

        result.IsSuccess.Should().BeTrue();
        var registeredIds = registry.Keywords.Select(keyword => keyword.Id).ToHashSet();
        result.Value!.KeywordIds.Should().OnlyContain(id => registeredIds.Contains(id));
    }

    [Fact]
    public void repeatable_and_required_group_behavior_is_modeled()
    {
        var registry = CreateRegistryWithKeywords();

        var optionalRepeatable = KeywordGroup.Create(
            "line_items",
            "Line Items",
            isRepeatable: true,
            isRequired: false,
            [KeywordIdFor(registry, "vendor_name")],
            registry,
            new FakeClock()).Value!;

        var requiredSingle = KeywordGroup.Create(
            "header",
            "Header",
            isRepeatable: false,
            isRequired: true,
            [KeywordIdFor(registry, "vendor_tax_id")],
            registry,
            new FakeClock()).Value!;

        optionalRepeatable.IsRepeatable.Should().BeTrue();
        optionalRepeatable.IsRequired.Should().BeFalse();
        requiredSingle.IsRepeatable.Should().BeFalse();
        requiredSingle.IsRequired.Should().BeTrue();
    }

    [Fact]
    public void keyword_group_created_event_contains_identity_and_definition()
    {
        var registry = CreateRegistryWithKeywords();
        var clock = new FakeClock { UtcNow = new DateTimeOffset(2026, 5, 22, 12, 0, 0, TimeSpan.Zero) };
        var keywordIds = new[] { KeywordIdFor(registry, "vendor_name"), KeywordIdFor(registry, "vendor_tax_id") };

        var group = KeywordGroup.Create(
            "vendor_details",
            "Vendor Details",
            isRepeatable: true,
            isRequired: false,
            keywordIds,
            registry,
            clock).Value!;

        var created = group.DomainEvents
            .OfType<KeywordGroupCreated>()
            .Should()
            .ContainSingle()
            .Subject;

        created.KeywordGroupId.Should().Be(group.Id);
        created.Code.Should().Be("vendor_details");
        created.Name.Should().Be("Vendor Details");
        created.IsRepeatable.Should().BeTrue();
        created.IsRequired.Should().BeFalse();
        created.KeywordIds.Should().BeEquivalentTo(keywordIds);
        created.OccurredOn.Should().Be(clock.UtcNow);
    }

    [Fact]
    public void slice_005_through_007_behavior_remains_valid()
    {
        var clock = new FakeClock();
        var documentType = DocumentType.Create("Invoice", "INV", clock).Value!;
        documentType.CreateVersion(clock).IsSuccess.Should().BeTrue();

        var registry = KeywordRegistry.Create(clock).Value!;
        registry.Register("amount", "Amount", KeywordDataType.Number, null, clock)
            .IsSuccess.Should().BeTrue();
        registry.Keywords.Should().ContainSingle();
    }

    private static KeywordRegistry CreateRegistryWithKeywords()
    {
        var clock = new FakeClock();
        var registry = KeywordRegistry.Create(clock).Value!;
        RegisterKeyword(registry, "vendor_name", "Vendor Name", KeywordDataType.Text, 100, clock);
        RegisterKeyword(registry, "vendor_tax_id", "Vendor Tax Id", KeywordDataType.Text, 50, clock);
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
