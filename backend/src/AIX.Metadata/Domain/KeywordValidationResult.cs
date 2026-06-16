using AIX.SharedKernel.Primitives;

namespace AIX.Metadata.Domain;

public sealed class KeywordValidationResult
{
    public bool IsValid { get; }
    public IReadOnlyList<Error> Errors { get; }

    private KeywordValidationResult(bool isValid, IReadOnlyList<Error> errors)
    {
        IsValid = isValid;
        Errors = errors;
    }

    public static KeywordValidationResult Valid() =>
        new(true, []);

    public static KeywordValidationResult Invalid(IReadOnlyList<Error> errors) =>
        new(false, errors);
}
