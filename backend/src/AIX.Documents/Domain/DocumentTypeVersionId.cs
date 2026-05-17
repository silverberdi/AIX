using AIX.SharedKernel.Primitives;

namespace AIX.Documents.Domain;

public sealed record DocumentTypeVersionId(Guid Value) : StronglyTypedId<Guid>(Value);
