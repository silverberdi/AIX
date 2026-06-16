using AIX.SharedKernel.Primitives;

namespace AIX.Metadata.Domain;

public sealed record KeywordRegistryId(Guid Value) : StronglyTypedId<Guid>(Value)
{
    public static KeywordRegistryId New() => new(Guid.NewGuid());
}
