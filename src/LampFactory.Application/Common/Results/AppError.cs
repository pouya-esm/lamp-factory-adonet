namespace LampFactory.Application.Common.Results;

// Describes an expected application failure. Unexpected exceptions are never
// wrapped in this type: they propagate to the centralized exception handler
// in the API layer, which logs them and returns a safe generic response.
public sealed class AppError
{
    public ErrorType Type { get; }

    public string Message { get; }

    // Populated for validation failures; empty for every other category.
    public IReadOnlyList<string> ValidationErrors { get; }

    private AppError(ErrorType type, string message, IEnumerable<string>? validationErrors)
    {
        Type = type;
        Message = message;
        ValidationErrors = validationErrors?.ToList() ?? new List<string>();
    }

    public static AppError Validation(string message, IEnumerable<string>? validationErrors = null)
    {
        return new AppError(ErrorType.Validation, message, validationErrors);
    }

    public static AppError NotFound(string message = "Resource not found.")
    {
        return new AppError(ErrorType.NotFound, message, null);
    }

    public static AppError Conflict(string message)
    {
        return new AppError(ErrorType.Conflict, message, null);
    }

    public static AppError Unauthorized(string message = "Unauthorized.")
    {
        return new AppError(ErrorType.Unauthorized, message, null);
    }

    public static AppError Forbidden(string message = "Forbidden.")
    {
        return new AppError(ErrorType.Forbidden, message, null);
    }
}
