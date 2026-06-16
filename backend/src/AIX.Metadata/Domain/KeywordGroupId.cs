using AIX.SharedKernel.Primitives;

namespace AIX.Metadata.Domain;

public sealed record KeywordGroupId(Guid Value) : StronglyTypedId<Guid>(Value)
{
    public static KeywordGroupId New() => new(Guid.NewGuid());
}
