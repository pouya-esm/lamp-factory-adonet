namespace LampFactory.Application.Common.Results;

// Application-level error categories. They are stable identifiers the API layer
// translates to HTTP status codes in a single place; they carry no HTTP
// semantics themselves so the Application layer stays transport-agnostic.
public enum ErrorType
{
    // Present only as the default; a returned failure always carries one of
    // the concrete categories below.
    None,

    Validation,
    NotFound,
    Conflict,
    Unauthorized,
    Forbidden
}
