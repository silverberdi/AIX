namespace AIX.Documents.Contracts;

public sealed class CaptureValidationResult
{
    public bool IsValid { get; }
    public IReadOnlyList<CaptureValidationError> Errors { get; }

    private CaptureValidationResult(bool isValid, IReadOnlyList<CaptureValidationError> errors)
    {
        IsValid = isValid;
        Errors = errors;
    }

    public static CaptureValidationResult Success() =>
        new(true, []);

    public static CaptureValidationResult Failure(IReadOnlyList<CaptureValidationError> errors) =>
        new(false, errors.ToList());
}
