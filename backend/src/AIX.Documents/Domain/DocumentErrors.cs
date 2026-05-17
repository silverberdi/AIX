using AIX.SharedKernel.Primitives;

namespace AIX.Documents.Domain;

internal static class DocumentErrors
{
    public static readonly Error DocumentTypeRequired =
        new("Documents.DocumentType.Required", "Document type is required.");

    public static readonly Error DocumentTypeVersionRequired =
        new("Documents.DocumentTypeVersion.Required", "Document type version is required.");
}
