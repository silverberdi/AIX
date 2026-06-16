using AIX.SharedKernel.Primitives;

namespace AIX.Metadata.Domain;

internal static class KeywordErrors
{
    public static readonly Error CodeRequired =
        new("Metadata.Keyword.Code.Required", "Keyword code is required.");

    public static readonly Error NameRequired =
        new("Metadata.Keyword.Name.Required", "Keyword name is required.");

    public static readonly Error DuplicateCode =
        new("Metadata.Keyword.Code.Duplicate", "Keyword code already exists.");

    public static readonly Error DuplicateName =
        new("Metadata.Keyword.Name.Duplicate", "Keyword name already exists.");

    public static readonly Error MaxLengthInvalid =
        new("Metadata.Keyword.MaxLength.Invalid", "Keyword max length must be greater than zero.");

    public static readonly Error MaxLengthNotAllowed =
        new("Metadata.Keyword.MaxLength.NotAllowed", "Keyword max length is only allowed for text keywords.");
}
