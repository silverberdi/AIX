namespace AIX.SharedKernel.Primitives;

public readonly record struct Error(string Code, string Message);

public class Result
{
    public bool IsSuccess { get; }
    public Error? Error { get; }

    protected Result(bool isSuccess, Error? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, null);

    public static Result Failure(Error error) => new(false, error);
}

public class Result<T> : Result
{
    public T? Value { get; }

    private Result(T value) : base(true, null) => Value = value;

    private Result(Error error) : base(false, error) => Value = default;

    public static Result<T> Success(T value) => new(value);

    public static new Result<T> Failure(Error error) => new(error);
}
