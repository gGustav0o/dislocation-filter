namespace DislocationFilter.Application.Common;

public readonly record struct Result<TError, TValue>
{
    private Result(TValue value)
    {
        IsSuccess = true;
        Value = value;
        Error = default;
    }

    private Result(TError error)
    {
        IsSuccess = false;
        Error = error;
        Value = default;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public TValue? Value { get; }

    public TError? Error { get; }

    public static Result<TError, TValue> Success(TValue value) => new(value);

    public static Result<TError, TValue> Failure(TError error) => new(error);

    public TResult Match<TResult>(Func<TValue, TResult> onSuccess, Func<TError, TResult> onFailure)
    {
        return IsSuccess
            ? onSuccess(Value!)
            : onFailure(Error!);
    }
}
