using AIX.SharedKernel.Primitives;

namespace AIX.Metadata.Domain;

internal static class VersionSchemaCompositionErrors
{
    public static readonly Error DuplicateFieldSchemaId =
        new("Metadata.VersionSchemaComposition.FieldSchemaId.Duplicate", "Version schema composition contains duplicate field schema identifiers.");

    public static readonly Error DuplicateKeyword =
        new("Metadata.VersionSchemaComposition.Keyword.Duplicate", "Version schema composition contains duplicate keyword references.");

    public static readonly Error KeywordGroupNotFound =
        new("Metadata.VersionSchemaComposition.KeywordGroup.NotFound", "Version schema composition references an unknown keyword group.");

    public static readonly Error DuplicateGroupPlacement =
        new("Metadata.VersionSchemaComposition.KeywordGroup.DuplicatePlacement", "Non-repeatable keyword group cannot be assigned more than once in the same version.");

    public static readonly Error DuplicateGroupInstanceKey =
        new("Metadata.VersionSchemaComposition.KeywordGroup.DuplicateInstanceKey", "Repeatable keyword group placement requires a unique instance key per assignment.");

    public static readonly Error KeywordInGroupAndStandaloneField =
        new("Metadata.VersionSchemaComposition.Keyword.GroupAndStandaloneConflict", "A keyword cannot appear in both a group assignment and a standalone field in the same version.");
}
