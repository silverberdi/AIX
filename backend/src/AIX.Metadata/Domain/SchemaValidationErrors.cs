using AIX.SharedKernel.Primitives;

namespace AIX.Metadata.Domain;

internal static class SchemaValidationErrors
{
    public static readonly Error VersionNotFound =
        new("Metadata.SchemaValidation.Version.NotFound", "Document type version was not found.");

    public static readonly Error UnknownMetadataKey =
        new("Metadata.SchemaValidation.Payload.UnknownKey", "Metadata payload contains an unknown key.");

    public static readonly Error HiddenFieldNotAllowedOnCapture =
        new("Metadata.SchemaValidation.Payload.HiddenField", "Hidden fields cannot be submitted on capture.");

    public static readonly Error DeprecatedFieldNotAllowedOnCapture =
        new("Metadata.SchemaValidation.Payload.DeprecatedField", "Deprecated fields cannot be submitted on capture.");

    public static readonly Error MissingRequiredGroupInstance =
        new("Metadata.SchemaValidation.Payload.MissingRequiredGroup", "Required keyword group instance is missing.");

    public static readonly Error UnexpectedGroupInstance =
        new("Metadata.SchemaValidation.Payload.UnexpectedGroupInstance", "Metadata payload contains an unexpected group instance.");

    public static readonly Error DuplicateGroupInstanceInPayload =
        new("Metadata.SchemaValidation.Payload.DuplicateGroupInstance", "Metadata payload contains duplicate group instances.");

    public static readonly Error GroupKeywordMustUseGroupPayload =
        new("Metadata.SchemaValidation.Payload.GroupKeywordInStandalone", "Keyword values assigned to groups must be submitted within the group instance.");
}
