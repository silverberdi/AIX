using AIX.SharedKernel.Primitives;

namespace AIX.Metadata.Domain;

public static class KeywordValidator
{
    private static readonly IKeywordValueSpecification[] ValueSpecifications =
    [
        new RequiredKeywordValueSpecification(),
        new KeywordDataTypeValueSpecification(),
        new TextMaxLengthValueSpecification()
    ];

    public static KeywordValidationResult ValidateValue(Keyword keyword, string? value, bool isRequired)
    {
        ArgumentNullException.ThrowIfNull(keyword);

        var errors = new List<Error>();
        foreach (var specification in ValueSpecifications)
        {
            if (!specification.IsSatisfiedBy(keyword, value, isRequired, out var error) && error is not null)
            {
                errors.Add(error.Value);
            }
        }

        return errors.Count == 0
            ? KeywordValidationResult.Valid()
            : KeywordValidationResult.Invalid(errors);
    }

    public static Result ValidateConfigurationChange(
        Keyword keyword,
        KeywordDataType proposedDataType,
        int? proposedMaxLength)
    {
        ArgumentNullException.ThrowIfNull(keyword);

        if (proposedDataType != keyword.DataType)
        {
            return Result.Failure(KeywordValidationErrors.DataTypeChangeForbidden);
        }

        if (keyword.MaxLength is not null &&
            proposedMaxLength is not null &&
            proposedMaxLength < keyword.MaxLength)
        {
            return Result.Failure(KeywordValidationErrors.MaxLengthReductionForbidden);
        }

        return Result.Success();
    }
}
