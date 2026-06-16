using AIX.SharedKernel.Primitives;

namespace AIX.Metadata.Domain;

public sealed record DocumentTypeVersionId(Guid Value) : StronglyTypedId<Guid>(Value)
{
    public static DocumentTypeVersionId New() => new(Guid.NewGuid());
}
