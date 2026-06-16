using AIX.SharedKernel.Primitives;

namespace AIX.Metadata.Domain;

internal static class FieldSchemaErrors
{
    public static readonly Error KeywordNotRegistered =
        new("Metadata.FieldSchema.Keyword.NotRegistered", "Field schema references a keyword that is not registered.");

    public static readonly Error IncompatibleCatalogType =
        new("Metadata.FieldSchema.CatalogType.Incompatible", "Field catalog type is incompatible with the keyword data type.");

    public static readonly Error OrderHintInvalid =
        new("Metadata.FieldSchema.OrderHint.Invalid", "Field order hint must be zero or greater.");
}
