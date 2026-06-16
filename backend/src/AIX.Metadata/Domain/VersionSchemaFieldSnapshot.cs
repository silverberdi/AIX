namespace AIX.Metadata.Domain;

public sealed record VersionSchemaFieldSnapshot(
    FieldSchemaId FieldSchemaId,
    KeywordId KeywordId,
    string KeywordCode,
    FieldCatalogType CatalogType);
