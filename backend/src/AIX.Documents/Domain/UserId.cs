using AIX.SharedKernel.Primitives;

namespace AIX.Documents.Domain;

public sealed record UserId(Guid Value) : StronglyTypedId<Guid>(Value);
