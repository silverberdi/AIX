using AIX.SharedKernel.Primitives;

namespace AIX.Metadata.Domain;

public sealed record DocumentTypeId(Guid Value) : StronglyTypedId<Guid>(Value)
{
    public static DocumentTypeId New() => new(Guid.NewGuid());
}
