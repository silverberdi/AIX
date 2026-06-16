using System.Reflection;
using AIX.Metadata.Domain;
using AIX.Metadata.Events;
using FluentAssertions;

namespace AIX.Metadata.Tests;

public class KeywordRegistryTests
{
    [Fact]
    public void registers_keyword_successfully()
    {
        var registry = CreateRegistry();
        var clock = new FakeClock();

        var result = registry.Register("invoice_number", "Invoice Number", KeywordDataType.Text, 50, clock);

        result.IsSuccess.Should().BeTrue();
        var keyword = result.Value!;
        keyword.Id.Value.Should().NotBe(Guid.Empty);
        keyword.Code.Should().Be("invoice_number");
        keyword.Name.Should().Be("Invoice Number");
        keyword.DataType.Should().Be(KeywordDataType.Text);
        keyword.MaxLength.Should().Be(50);
    }

    [Fact]
    public void registered_keyword_appears_in_registry()
    {
        var registry = CreateRegistry();
        var keyword = RegisterKeyword(registry, "amount", "Amount", KeywordDataType.Number);

        registry.Keywords.Should().ContainSingle().Which.Should().Be(keyword);
    }

    [Fact]
    public void keyword_requires_code()
    {
        var registry = CreateRegistry();

        var result = registry.Register("   ", "Invoice Number", KeywordDataType.Text, 50, new FakeClock());

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(KeywordErrors.CodeRequired);
    }

    [Fact]
    public void keyword_requires_name()
    {
        var registry = CreateRegistry();

        var result = registry.Register("invoice_number", "", KeywordDataType.Text, 50, new FakeClock());

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(KeywordErrors.NameRequired);
    }

    [Fact]
    public void duplicate_keyword_code_is_rejected()
    {
        var registry = CreateRegistry();
        var clock = new FakeClock();
        registry.Register("invoice_number", "Invoice Number", KeywordDataType.Text, 50, clock)
            .IsSuccess.Should().BeTrue();

        var result = registry.Register("invoice_number", "Other Name", KeywordDataType.Text, 40, clock);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(KeywordErrors.DuplicateCode);
    }

    [Fact]
    public void duplicate_keyword_code_is_rejected_case_insensitively()
    {
        var registry = CreateRegistry();
        var clock = new FakeClock();
        registry.Register("INV_NUM", "Invoice Number", KeywordDataType.Text, 50, clock)
            .IsSuccess.Should().BeTrue();

        var result = registry.Register("inv_num", "Duplicate", KeywordDataType.Text, 40, clock);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(KeywordErrors.DuplicateCode);
    }

    [Fact]
    public void duplicate_keyword_name_is_rejected()
    {
        var registry = CreateRegistry();
        var clock = new FakeClock();
        registry.Register("code_a", "Invoice Number", KeywordDataType.Text, 50, clock)
            .IsSuccess.Should().BeTrue();

        var result = registry.Register("code_b", "Invoice Number", KeywordDataType.Number, null, clock);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(KeywordErrors.DuplicateName);
    }

    [Fact]
    public void max_length_must_be_greater_than_zero_when_provided()
    {
        var registry = CreateRegistry();

        var result = registry.Register("invoice_number", "Invoice Number", KeywordDataType.Text, 0, new FakeClock());

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(KeywordErrors.MaxLengthInvalid);
    }

    [Fact]
    public void max_length_is_not_allowed_for_non_text_types()
    {
        var registry = CreateRegistry();

        var result = registry.Register("amount", "Amount", KeywordDataType.Number, 10, new FakeClock());

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(KeywordErrors.MaxLengthNotAllowed);
    }

    [Fact]
    public void text_keyword_allows_null_max_length()
    {
        var registry = CreateRegistry();

        var result = registry.Register("notes", "Notes", KeywordDataType.Text, null, new FakeClock());

        result.IsSuccess.Should().BeTrue();
        result.Value!.MaxLength.Should().BeNull();
    }

    [Fact]
    public void keyword_data_type_is_immutable_after_registration()
    {
        var keyword = RegisterKeyword(CreateRegistry(), "code", "Name", KeywordDataType.Text, 100);

        var publicMethodNames = typeof(Keyword)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Select(method => method.Name)
            .ToList();

        publicMethodNames.Should().NotContain("ChangeDataType");
        publicMethodNames.Should().NotContain("SetDataType");
        publicMethodNames.Where(name => name.Contains("DataType", StringComparison.Ordinal))
            .Should()
            .BeEquivalentTo("get_DataType");
    }

    [Fact]
    public void keyword_registered_event_contains_identity_and_definition()
    {
        var registry = CreateRegistry();
        var clock = new FakeClock { UtcNow = new DateTimeOffset(2026, 5, 22, 10, 0, 0, TimeSpan.Zero) };

        var keyword = registry.Register("invoice_number", "Invoice Number", KeywordDataType.Text, 50, clock).Value!;
        var registered = registry.DomainEvents
            .OfType<KeywordRegistered>()
            .Should()
            .ContainSingle()
            .Subject;

        registered.KeywordId.Should().Be(keyword.Id);
        registered.Code.Should().Be("invoice_number");
        registered.Name.Should().Be("Invoice Number");
        registered.DataType.Should().Be(KeywordDataType.Text);
        registered.MaxLength.Should().Be(50);
        registered.OccurredOn.Should().Be(clock.UtcNow);
    }

    [Fact]
    public void slice_005_and_006_document_type_behavior_remain_valid()
    {
        var clock = new FakeClock();
        var createResult = DocumentType.Create("Invoice", "INV", clock);

        createResult.IsSuccess.Should().BeTrue();
        var documentType = createResult.Value!;
        documentType.CreateVersion(clock).IsSuccess.Should().BeTrue();
        documentType.LatestVersion!.VersionNumber.Should().Be(1);
        documentType.Deactivate(clock).IsSuccess.Should().BeTrue();
        documentType.Activate(clock).IsSuccess.Should().BeTrue();
    }

    private static KeywordRegistry CreateRegistry()
    {
        var result = KeywordRegistry.Create(new FakeClock());
        result.IsSuccess.Should().BeTrue();
        return result.Value!;
    }

    private static Keyword RegisterKeyword(
        KeywordRegistry registry,
        string code,
        string name,
        KeywordDataType dataType,
        int? maxLength = null)
    {
        var result = registry.Register(code, name, dataType, maxLength, new FakeClock());
        result.IsSuccess.Should().BeTrue();
        return result.Value!;
    }
}
