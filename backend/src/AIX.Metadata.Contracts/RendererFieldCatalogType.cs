namespace AIX.Metadata.Contracts;

/// <summary>
/// Renderer-facing field catalog types aligned with <c>docs/architecture/renderer-runtime.md</c>.
/// MVP domain composition uses a subset; additional values are reserved for future catalog expansion.
/// </summary>
public enum RendererFieldCatalogType
{
    Text,
    TextArea,
    Number,
    Decimal,
    Date,
    DateTime,
    Boolean,
    Select,
    MultiSelect,
    Table,
    File,
    RichText
}
