using AIX.SharedKernel.Primitives;

namespace AIX.Metadata.Domain;

public sealed record FieldSchemaId(Guid Value) : StronglyTypedId<Guid>(Value)
{
    public static FieldSchemaId New() => new(Guid.NewGuid());
}
