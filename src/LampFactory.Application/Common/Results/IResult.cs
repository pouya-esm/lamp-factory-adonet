namespace LampFactory.Application.Common.Results;

// Abstraction shared by Result and Result<T> so the API mapping layer can
// translate any outcome without knowing the payload type. Success and failure
// are distinguishable purely by IsSuccess - never by interpreting messages.
public interface IResult
{
    bool IsSuccess { get; }

    // Populated only when the operation failed. Unexpected failures
    // (exceptions) are not represented here; they propagate to the API
    // exception handler.
    AppError? Failure { get; }
}
