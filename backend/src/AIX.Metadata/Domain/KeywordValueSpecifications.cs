using System.Globalization;
using AIX.SharedKernel.Primitives;

namespace AIX.Metadata.Domain;

internal sealed class RequiredKeywordValueSpecification : IKeywordValueSpecification
{
    public bool IsSatisfiedBy(Keyword keyword, string? value, bool isRequired, out Error? error)
    {
        if (!isRequired || !string.IsNullOrWhiteSpace(value))
        {
            error = null;
            return true;
        }

        error = KeywordValidationErrors.ValueRequired;
        return false;
    }
}

internal sealed class KeywordDataTypeValueSpecification : IKeywordValueSpecification
{
    public bool IsSatisfiedBy(Keyword keyword, string? value, bool isRequired, out Error? error)
    {
        if (!isRequired && string.IsNullOrWhiteSpace(value))
        {
            error = null;
            return true;
        }

        if (value is null)
        {
            error = null;
            return true;
        }

        var normalized = value.Trim();
        error = keyword.DataType switch
        {
            KeywordDataType.Text => null,
            KeywordDataType.Number when decimal.TryParse(
                normalized,
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out _) => null,
            KeywordDataType.Number => KeywordValidationErrors.InvalidNumber,
            KeywordDataType.Date when DateOnly.TryParse(
                normalized,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out _) => null,
            KeywordDataType.Date => KeywordValidationErrors.InvalidDate,
            KeywordDataType.Boolean when bool.TryParse(normalized, out _) => null,
            KeywordDataType.Boolean => KeywordValidationErrors.InvalidBoolean,
            _ => null
        };

        return error is null;
    }
}

internal sealed class TextMaxLengthValueSpecification : IKeywordValueSpecification
{
    public bool IsSatisfiedBy(Keyword keyword, string? value, bool isRequired, out Error? error)
    {
        if (keyword.DataType != KeywordDataType.Text ||
            keyword.MaxLength is null ||
            string.IsNullOrWhiteSpace(value))
        {
            error = null;
            return true;
        }

        if (value.Trim().Length <= keyword.MaxLength)
        {
            error = null;
            return true;
        }

        error = KeywordValidationErrors.ExceedsMaxLength;
        return false;
    }
}
