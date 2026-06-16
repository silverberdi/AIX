namespace AIX.Metadata.Contracts;

/// <summary>
/// Version-bound renderer contract aggregating composition and layout for a document type version.
/// </summary>
public sealed record DocumentSchemaContract(
    Guid DocumentTypeId,
    string DocumentTypeCode,
    string DocumentTypeName,
    Guid DocumentTypeVersionId,
    int VersionNumber,
    /// <summary>
    /// Stable binding key for renderer version selection (e.g. <c>INV/v2</c>).
    /// </summary>
    string SchemaBindingKey,
    IReadOnlyList<FieldSchemaContract> Fields,
    IReadOnlyList<GroupAssignmentContract> Groups,
    LayoutSchemaContract Layout);
