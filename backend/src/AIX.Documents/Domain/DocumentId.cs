using AIX.SharedKernel.Primitives;

namespace AIX.Documents.Domain;

public sealed record DocumentId(Guid Value) : StronglyTypedId<Guid>(Value)
{
    public static DocumentId New() => new(Guid.NewGuid());
}
