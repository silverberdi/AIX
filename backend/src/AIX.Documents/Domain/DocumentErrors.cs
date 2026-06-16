using AIX.SharedKernel.Primitives;

namespace AIX.Documents.Domain;

internal static class DocumentErrors
{
    public static readonly Error DocumentTypeRequired =
        new("Documents.DocumentType.Required", "Document type is required.");

    public static readonly Error DocumentTypeVersionRequired =
        new("Documents.DocumentTypeVersion.Required", "Document type version is required.");

    public static readonly Error AlreadyComplete =
        new("Documents.Document.AlreadyComplete", "Document is already complete.");

    public static readonly Error CannotModifyWhenComplete =
        new("Documents.Document.CannotModifyWhenComplete", "Complete documents cannot be modified.");

    public static readonly Error FileIdRequired =
        new("Documents.Document.FileId.Required", "File id is required.");

    public static readonly Error FileNameRequired =
        new("Documents.Document.FileName.Required", "File name is required.");

    public static readonly Error ContentTypeRequired =
        new("Documents.Document.ContentType.Required", "Content type is required.");

    public static readonly Error FileSizeInvalid =
        new("Documents.Document.FileSize.Invalid", "File size must be greater than zero.");

    public static readonly Error FileAlreadyAttached =
        new("Documents.Document.FileAlreadyAttached", "File is already attached to the document.");

    public static readonly Error PrimaryFileAlreadyExists =
        new("Documents.Document.PrimaryFileAlreadyExists", "Document already has a primary file.");
}
