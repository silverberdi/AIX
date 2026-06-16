using AIX.SharedKernel.Primitives;

namespace AIX.Metadata.Domain;

internal interface IKeywordValueSpecification
{
    bool IsSatisfiedBy(Keyword keyword, string? value, bool isRequired, out Error? error);
}
