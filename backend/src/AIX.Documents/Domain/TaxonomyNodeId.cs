using AIX.SharedKernel.Primitives;

namespace AIX.Documents.Domain;

public sealed record TaxonomyNodeId(Guid Value) : StronglyTypedId<Guid>(Value);
