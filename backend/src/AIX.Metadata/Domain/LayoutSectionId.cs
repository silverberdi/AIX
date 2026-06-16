using AIX.SharedKernel.Primitives;

namespace AIX.Metadata.Domain;

public sealed record LayoutSectionId(Guid Value) : StronglyTypedId<Guid>(Value)
{
    public static LayoutSectionId New() => new(Guid.NewGuid());
}
