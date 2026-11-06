using System.Net;

namespace LampFactory.API.Common.Models;

// Wire contract shared by every endpoint:
// { isSuccess, statusCode, errorMessage, result } (camelCase on the wire).
// The frontend client unwraps this envelope and joins errorMessage strings, so
// the field names and types are a compatibility surface: do not rename them.
public class APIResponse
{
    public HttpStatusCode StatusCode { get; set; }

    public List<string>? ErrorMessage { get; set; }

    public bool IsSuccess { get; set; } = true;

    public object? Result { get; set; }
}
