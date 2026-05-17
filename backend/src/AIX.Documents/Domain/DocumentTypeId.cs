using AIX.SharedKernel.Primitives;

namespace AIX.Documents.Domain;

public sealed record DocumentTypeId(Guid Value) : StronglyTypedId<Guid>(Value);
