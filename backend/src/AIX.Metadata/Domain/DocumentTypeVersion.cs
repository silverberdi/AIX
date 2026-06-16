namespace AIX.Metadata.Domain;

public sealed record DocumentTypeVersion(
    DocumentTypeVersionId Id,
    int VersionNumber,
    DateTimeOffset CreatedAt,
    VersionSchemaComposition SchemaComposition,
    LayoutSchema LayoutSchema);
