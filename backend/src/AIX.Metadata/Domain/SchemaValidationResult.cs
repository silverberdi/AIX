using AIX.SharedKernel.Primitives;

namespace AIX.Metadata.Domain;

public sealed class SchemaValidationResult
{
    public bool IsValid { get; }
    public IReadOnlyList<Error> Errors { get; }

    private SchemaValidationResult(bool isValid, IReadOnlyList<Error> errors)
    {
        IsValid = isValid;
        Errors = errors;
    }

    public static SchemaValidationResult Valid() =>
        new(true, []);

    public static SchemaValidationResult Invalid(IReadOnlyList<Error> errors) =>
        new(false, errors);
}
