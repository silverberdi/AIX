using AIX.SharedKernel.Primitives;

namespace AIX.Metadata.Domain;

internal static class LayoutSchemaErrors
{
    public static readonly Error UnknownFieldReference =
        new("Metadata.LayoutSchema.Field.NotFound", "Layout references a field that is not present in the version schema composition.");

    public static readonly Error UnknownGroupReference =
        new("Metadata.LayoutSchema.Group.NotFound", "Layout references a group assignment that is not present in the version schema composition.");

    public static readonly Error DuplicateFieldPlacement =
        new("Metadata.LayoutSchema.Field.DuplicatePlacement", "Layout contains duplicate field placements.");

    public static readonly Error DuplicateGroupPlacement =
        new("Metadata.LayoutSchema.Group.DuplicatePlacement", "Layout contains duplicate group instance placements.");

    public static readonly Error DuplicateSectionId =
        new("Metadata.LayoutSchema.Section.DuplicateId", "Layout contains duplicate section identifiers.");

    public static readonly Error SectionTitleRequired =
        new("Metadata.LayoutSchema.Section.TitleRequired", "Layout section title is required.");
}
