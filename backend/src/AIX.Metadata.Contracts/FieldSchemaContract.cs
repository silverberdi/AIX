namespace AIX.Metadata.Contracts;

public sealed record FieldSchemaContract(
    Guid FieldSchemaId,
    Guid KeywordId,
    string KeywordCode,
    RendererFieldCatalogType CatalogType,
    string? LabelOverride,
    string? HelpText,
    int? OrderHint,
    bool IsDeprecated,
    bool IsHidden,
    /// <summary>
    /// Placeholder for Reference Data dataset binding on SELECT-like fields. Not resolved in MVP.
    /// </summary>
    Guid? DatasetId);
