using AIX.SharedKernel.Primitives;

namespace AIX.Metadata.Domain;

public sealed record KeywordId(Guid Value) : StronglyTypedId<Guid>(Value)
{
    public static KeywordId New() => new(Guid.NewGuid());
}
