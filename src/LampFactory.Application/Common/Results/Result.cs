namespace LampFactory.Application.Common.Results;

// Non-generic outcome for operations that only succeed or fail (e.g. delete).
// A failure always carries a typed AppError; a success carries no payload and
// knows nothing about HTTP status codes. The API layer owns the translation
// from ErrorType to HTTP status codes in one place.
public sealed class Result : IResult
{
    public bool IsSuccess { get; }

    public AppError? Failure { get; }

    private Result(bool isSuccess, AppError? failure)
    {
        IsSuccess = isSuccess;
        Failure = failure;
    }

    public static Result Success()
    {
        return new Result(true, null);
    }

    public static Result Fail(AppError error)
    {
        return new Result(false, error);
    }

    // Expected failure categories. BadRequest stays as the historical factory
    // name for client-input failures (invalid ids, validator output); it maps
    // to the Validation error category, no longer to a raw HTTP status.
    public static Result BadRequest(string message, IEnumerable<string>? validationErrors = null)
    {
        return Fail(AppError.Validation(message, validationErrors));
    }

    public static Result NotFound(string message = "Resource not found.")
    {
        return Fail(AppError.NotFound(message));
    }

    public static Result Conflict(string message)
    {
        return Fail(AppError.Conflict(message));
    }

    public static Result Unauthorized(string message = "Unauthorized.")
    {
        return Fail(AppError.Unauthorized(message));
    }

    public static Result Forbidden(string message = "Forbidden.")
    {
        return Fail(AppError.Forbidden(message));
    }
}
