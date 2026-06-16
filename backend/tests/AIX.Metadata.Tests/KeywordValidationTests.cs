using AIX.Metadata.Domain;
using FluentAssertions;

namespace AIX.Metadata.Tests;

public class KeywordValidationTests
{
    [Fact]
    public void required_keyword_value_must_be_present()
    {
        var keyword = TextKeyword(maxLength: 50);

        var result = KeywordValidator.ValidateValue(keyword, value: null, isRequired: true);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle().Which.Should().Be(KeywordValidationErrors.ValueRequired);
    }

    [Fact]
    public void optional_keyword_value_allows_empty_and_null()
    {
        var keyword = TextKeyword(maxLength: 50);

        KeywordValidator.ValidateValue(keyword, value: null, isRequired: false).IsValid.Should().BeTrue();
        KeywordValidator.ValidateValue(keyword, value: "", isRequired: false).IsValid.Should().BeTrue();
        KeywordValidator.ValidateValue(keyword, value: "   ", isRequired: false).IsValid.Should().BeTrue();
    }

    [Fact]
    public void text_value_exceeding_max_length_fails()
    {
        var keyword = TextKeyword(maxLength: 5);

        var result = KeywordValidator.ValidateValue(keyword, value: "123456", isRequired: true);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(KeywordValidationErrors.ExceedsMaxLength);
    }

    [Fact]
    public void text_value_within_max_length_succeeds()
    {
        var keyword = TextKeyword(maxLength: 5);

        var result = KeywordValidator.ValidateValue(keyword, value: "12345", isRequired: true);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void number_keyword_rejects_non_numeric_value()
    {
        var keyword = KeywordWithType(KeywordDataType.Number);

        var result = KeywordValidator.ValidateValue(keyword, value: "not-a-number", isRequired: true);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(KeywordValidationErrors.InvalidNumber);
    }

    [Fact]
    public void number_keyword_accepts_valid_numeric_value()
    {
        var keyword = KeywordWithType(KeywordDataType.Number);

        KeywordValidator.ValidateValue(keyword, value: "42.5", isRequired: true).IsValid.Should().BeTrue();
    }

    [Fact]
    public void date_keyword_rejects_invalid_date()
    {
        var keyword = KeywordWithType(KeywordDataType.Date);

        var result = KeywordValidator.ValidateValue(keyword, value: "not-a-date", isRequired: true);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(KeywordValidationErrors.InvalidDate);
    }

    [Fact]
    public void date_keyword_accepts_valid_date()
    {
        var keyword = KeywordWithType(KeywordDataType.Date);

        KeywordValidator.ValidateValue(keyword, value: "2026-05-22", isRequired: true).IsValid.Should().BeTrue();
    }

    [Fact]
    public void boolean_keyword_rejects_invalid_boolean()
    {
        var keyword = KeywordWithType(KeywordDataType.Boolean);

        var result = KeywordValidator.ValidateValue(keyword, value: "maybe", isRequired: true);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(KeywordValidationErrors.InvalidBoolean);
    }

    [Fact]
    public void boolean_keyword_accepts_true_and_false()
    {
        var keyword = KeywordWithType(KeywordDataType.Boolean);

        KeywordValidator.ValidateValue(keyword, value: "true", isRequired: true).IsValid.Should().BeTrue();
        KeywordValidator.ValidateValue(keyword, value: "FALSE", isRequired: true).IsValid.Should().BeTrue();
    }

    [Fact]
    public void validation_collects_multiple_failures()
    {
        var keyword = KeywordWithType(KeywordDataType.Boolean);

        var result = KeywordValidator.ValidateValue(keyword, value: "   ", isRequired: true);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(KeywordValidationErrors.ValueRequired);
        result.Errors.Should().Contain(KeywordValidationErrors.InvalidBoolean);
    }

    [Fact]
    public void keyword_data_type_change_is_forbidden()
    {
        var keyword = TextKeyword(maxLength: 50);

        var result = KeywordValidator.ValidateConfigurationChange(
            keyword,
            KeywordDataType.Number,
            proposedMaxLength: null);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(KeywordValidationErrors.DataTypeChangeForbidden);
    }

    [Fact]
    public void keyword_max_length_reduction_is_forbidden()
    {
        var keyword = TextKeyword(maxLength: 100);

        var result = KeywordValidator.ValidateConfigurationChange(
            keyword,
            KeywordDataType.Text,
            proposedMaxLength: 50);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(KeywordValidationErrors.MaxLengthReductionForbidden);
    }

    [Fact]
    public void keyword_max_length_increase_is_allowed()
    {
        var keyword = TextKeyword(maxLength: 50);

        var result = KeywordValidator.ValidateConfigurationChange(
            keyword,
            KeywordDataType.Text,
            proposedMaxLength: 100);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void registry_validates_keyword_values_through_centralized_validator()
    {
        var registry = CreateRegistry();
        var keyword = registry.Register("amount", "Amount", KeywordDataType.Number, null, new FakeClock()).Value!;

        var missing = registry.ValidateKeywordValue(keyword.Id, value: null, isRequired: true);
        missing.IsSuccess.Should().BeFalse();
        missing.Error.Should().Be(KeywordValidationErrors.ValueRequired);

        var invalid = registry.ValidateKeywordValue(keyword.Id, value: "abc", isRequired: true);
        invalid.IsSuccess.Should().BeFalse();
        invalid.Error.Should().Be(KeywordValidationErrors.InvalidNumber);

        var valid = registry.ValidateKeywordValue(keyword.Id, value: "10", isRequired: true);
        valid.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void registry_rejects_validation_for_unknown_keyword()
    {
        var registry = CreateRegistry();

        var result = registry.ValidateKeywordValue(KeywordId.New(), value: "x", isRequired: true);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(KeywordValidationErrors.KeywordNotFound);
    }

    [Fact]
    public void slice_005_through_008_behavior_remains_valid()
    {
        var clock = new FakeClock();
        var documentType = DocumentType.Create("Invoice", "INV", clock).Value!;
        documentType.CreateVersion(clock).IsSuccess.Should().BeTrue();

        var registry = KeywordRegistry.Create(clock).Value!;
        var keyword = registry.Register("notes", "Notes", KeywordDataType.Text, 100, clock).Value!;
        KeywordGroup.Create(
            "header",
            "Header",
            isRepeatable: false,
            isRequired: true,
            [keyword.Id],
            registry,
            clock).IsSuccess.Should().BeTrue();
    }

    private static KeywordRegistry CreateRegistry()
    {
        var result = KeywordRegistry.Create(new FakeClock());
        result.IsSuccess.Should().BeTrue();
        return result.Value!;
    }

    private static Keyword TextKeyword(int? maxLength) =>
        RegisterKeyword("text_code", "Text", KeywordDataType.Text, maxLength);

    private static Keyword KeywordWithType(KeywordDataType dataType) =>
        RegisterKeyword($"{dataType}_code", dataType.ToString(), dataType, maxLength: null);

    private static Keyword RegisterKeyword(
        string code,
        string name,
        KeywordDataType dataType,
        int? maxLength)
    {
        var registry = CreateRegistry();
        var result = registry.Register(code, name, dataType, maxLength, new FakeClock());
        result.IsSuccess.Should().BeTrue();
        return result.Value!;
    }
}
