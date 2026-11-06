namespace LampFactory.Application.Common.Results;

// Typed outcome. Deliberately does not inherit from the non-generic Result:
// inheriting forced the old design to shadow factory methods with "new" and
// could return a wrongly typed (non-generic) result through the base call.
// Value is only ever assigned on the success path, so a failed result can
// never carry a success value.
public sealed class Result<T> : IResult
{
    public bool IsSuccess { get; }

    public AppError? Failure { get; }

    // default(T) only when failed; callers must check IsSuccess first.
    public T? Value { get; }

    private Result(bool isSuccess, AppError? failure, T? value)
    {
        IsSuccess = isSuccess;
        Failure = failure;
        Value = value;
    }

    public static Result<T> Success(T value)
    {
        return new Result<T>(true, null, value);
    }

    public static Result<T> Fail(AppError error)
    {
        return new Result<T>(false, error, default);
    }

    // Factory names mirror the previous API minus the HTTP status argument so
    // call sites keep their shape; BadRequest belongs to the Validation
    // category and no longer stores a status code.
    public static Result<T> BadRequest(string message, IEnumerable<string>? validationErrors = null)
    {
        return Fail(AppError.Validation(message, validationErrors));
    }

    public static Result<T> NotFound(string message = "Resource not found.")
    {
        return Fail(AppError.NotFound(message));
    }

    public static Result<T> Conflict(string message)
    {
        return Fail(AppError.Conflict(message));
    }

    public static Result<T> Unauthorized(string message = "Unauthorized.")
    {
        return Fail(AppError.Unauthorized(message));
    }

    public static Result<T> Forbidden(string message = "Forbidden.")
    {
        return Fail(AppError.Forbidden(message));
    }
}
