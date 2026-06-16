namespace AIX.Metadata.Domain;

public sealed record VersionSchemaFieldDefinition(
    KeywordId KeywordId,
    FieldCatalogType CatalogType,
    string? LabelOverride = null,
    string? HelpText = null,
    int? OrderHint = null,
    bool IsDeprecated = false,
    bool IsHidden = false,
    bool IsRequired = false);
