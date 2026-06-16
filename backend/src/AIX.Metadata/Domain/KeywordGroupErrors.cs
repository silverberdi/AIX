using AIX.SharedKernel.Primitives;

namespace AIX.Metadata.Domain;

internal static class KeywordGroupErrors
{
    public static readonly Error CodeRequired =
        new("Metadata.KeywordGroup.Code.Required", "Keyword group code is required.");

    public static readonly Error NameRequired =
        new("Metadata.KeywordGroup.Name.Required", "Keyword group name is required.");

    public static readonly Error KeywordsRequired =
        new("Metadata.KeywordGroup.Keywords.Required", "Keyword group must reference at least one keyword.");

    public static readonly Error DuplicateKeyword =
        new("Metadata.KeywordGroup.Keyword.Duplicate", "Keyword group cannot reference the same keyword more than once.");

    public static readonly Error KeywordNotRegistered =
        new("Metadata.KeywordGroup.Keyword.NotRegistered", "Keyword group references a keyword that is not registered.");
}
