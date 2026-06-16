using AIX.SharedKernel.Primitives;

namespace AIX.Documents.Domain;

public sealed record DocumentFileId(Guid Value) : StronglyTypedId<Guid>(Value);
