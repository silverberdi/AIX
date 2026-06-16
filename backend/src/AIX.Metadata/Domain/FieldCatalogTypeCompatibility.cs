namespace AIX.Metadata.Domain;

internal static class FieldCatalogTypeCompatibility
{
    public static bool IsCompatible(KeywordDataType dataType, FieldCatalogType catalogType) =>
        dataType switch
        {
            KeywordDataType.Text => catalogType is FieldCatalogType.Text or FieldCatalogType.TextArea,
            KeywordDataType.Number => catalogType is FieldCatalogType.Number or FieldCatalogType.Decimal,
            KeywordDataType.Date => catalogType is FieldCatalogType.Date or FieldCatalogType.DateTime,
            KeywordDataType.Boolean => catalogType is FieldCatalogType.Boolean,
            _ => false
        };
}
