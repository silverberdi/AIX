using AIX.SharedKernel.Primitives;

namespace AIX.Metadata.Domain;

internal static class KeywordValidationErrors
{
    public static readonly Error ValueRequired =
        new("Metadata.KeywordValidation.Value.Required", "Keyword value is required.");

    public static readonly Error ExceedsMaxLength =
        new("Metadata.KeywordValidation.Value.ExceedsMaxLength", "Keyword value exceeds the allowed max length.");

    public static readonly Error InvalidNumber =
        new("Metadata.KeywordValidation.Value.InvalidNumber", "Keyword value is not a valid number.");

    public static readonly Error InvalidDate =
        new("Metadata.KeywordValidation.Value.InvalidDate", "Keyword value is not a valid date.");

    public static readonly Error InvalidBoolean =
        new("Metadata.KeywordValidation.Value.InvalidBoolean", "Keyword value is not a valid boolean.");

    public static readonly Error KeywordNotFound =
        new("Metadata.KeywordValidation.Keyword.NotFound", "Keyword was not found in the registry.");

    public static readonly Error DataTypeChangeForbidden =
        new("Metadata.KeywordValidation.Configuration.DataTypeChangeForbidden", "Keyword data type cannot be changed.");

    public static readonly Error MaxLengthReductionForbidden =
        new("Metadata.KeywordValidation.Configuration.MaxLengthReductionForbidden", "Keyword max length cannot be reduced.");
}
