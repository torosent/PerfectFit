namespace PerfectFit.UseCases.Gamification;

/// <summary>
/// Represents the result of an operation that can fail.
/// </summary>
public record Result
{
    public bool IsSuccess { get; }
    public string? Error { get; }

    protected Result(bool isSuccess, string? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, null);
    public static Result Failure(string error) => new(false, error);
}

/// <summary>
/// Represents the result of an operation that can fail and returns a value on success.
/// </summary>
/// <typeparam name="T">The type of the value on success.</typeparam>
public record Result<T> : Result
{
    public T? Value { get; }

    private Result(bool isSuccess, T? value, string? error) : base(isSuccess, error)
    {
        Value = value;
    }

    public static Result<T> Success(T value) => new(true, value, null);
    public new static Result<T> Failure(string error) => new(false, default, error);
}
