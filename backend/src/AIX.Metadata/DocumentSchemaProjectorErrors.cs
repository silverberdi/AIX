using AIX.SharedKernel.Primitives;

namespace AIX.Metadata;

public static class DocumentSchemaProjectorErrors
{
    public static readonly Error VersionNotFound = new("DocumentSchema.VersionNotFound", "Document type version was not found.");
}
