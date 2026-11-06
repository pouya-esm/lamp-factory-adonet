using System.Net;

namespace LampFactory.API.Common.Models;

// Typed view of the same envelope for endpoints whose payload type is known
// at compile time (controllers and Swagger get a concrete Result type).
// Deliberately NOT derived from APIResponse: inheriting would force a "new"
// Hiding of Result, so the two classes stand alone with identical members and
// an identical JSON shape.
public class APIResponse<T>
{
    public HttpStatusCode StatusCode { get; set; }

    public List<string>? ErrorMessage { get; set; }

    public bool IsSuccess { get; set; } = true;

    public T? Result { get; set; }
}
