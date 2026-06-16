using AIX.SharedKernel.Primitives;

namespace AIX.Metadata.Domain;

internal static class DocumentTypeErrors
{
    public static readonly Error NameRequired =
        new("Metadata.DocumentType.Name.Required", "Document type name is required.");

    public static readonly Error CodeRequired =
        new("Metadata.DocumentType.Code.Required", "Document type code is required.");

    public static readonly Error AlreadyActive =
        new("Metadata.DocumentType.AlreadyActive", "Document type is already active.");

    public static readonly Error AlreadyInactive =
        new("Metadata.DocumentType.AlreadyInactive", "Document type is already inactive.");

    public static readonly Error VersionNumberInvalid =
        new("Metadata.DocumentType.Version.Number.Invalid", "Document type version number must be greater than zero.");

    public static readonly Error DuplicateVersionNumber =
        new("Metadata.DocumentType.Version.Number.Duplicate", "Document type version number already exists.");
}
