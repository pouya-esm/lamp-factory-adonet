using System.Net;
using LampFactory.API.Common.Models;
using LampFactory.Application.Common.Results;
using Microsoft.AspNetCore.Mvc;

namespace LampFactory.API.Common;

// Single place where application results become HTTP responses.
//
// Envelope stays identical to the historical contract
// { isSuccess, statusCode, errorMessage, result }; only the transport status
// improves: creation endpoints return 201 and deletions return 204.
//
// Error mapping rules (ErrorType -> HTTP):
//   Validation -> 400, NotFound -> 404, Conflict -> 409,
//   Unauthorized -> 401, Forbidden -> 403.
// Unexpected exceptions never become results; the centralized exception
// handler returns the generic 500 response.
public static class ResultMappingExtensions
{    // Success with payload -> 200 OK. Used by GET, and PUT/POST endpoints that
    // return the persisted state without creating a new resource.
    public static ActionResult<APIResponse<T>> ToActionResult<T>(this Result<T> result)
    {
        if (result.IsSuccess)
        {
            return new OkObjectResult(SuccessEnvelope(result.Value));
        }

        return ErrorResult<T>(result.Failure!);
    }

    // Success with a newly created resource -> 201 Created. When the route
    // values of the created entity can be produced the Location header is
    // populated via CreatedAtAction; route and action are optional, otherwise
    // the response is a plain 201 without a fabricated Location.
    public static ActionResult<APIResponse<T>> ToCreatedAtActionResult<T>(
        this Result<T> result, ControllerBase controller, string? actionName = null, Func<T, object?>? routeValues = null)
    {
        if (!result.IsSuccess)
        {
            return ErrorResult<T>(result.Failure!);
        }

        var envelope = SuccessEnvelope(result.Value);
        var values = actionName != null ? routeValues?.Invoke(result.Value!) : null;
        if (values == null)
        {
            return new ObjectResult(envelope) { StatusCode = (int)HttpStatusCode.Created };
        }

        // A null controllerName makes Url resolution stay on the current controller.
        return new CreatedAtActionResult(actionName, null, values, envelope);
    }

    // Non-generic success: 200 OK envelope without a payload. Used by the few
    // update/delete-adjacent endpoints whose service still signals plain
    // success/failure (e.g. UpdatePermissionAsync).
    public static ActionResult<APIResponse> ToActionResult(this Result result)
    {
        if (result.IsSuccess)
        {
            return new OkObjectResult(new APIResponse { StatusCode = HttpStatusCode.OK });
        }

        return ErrorResult<object>(result.Failure!);
    }

    // Successful delete -> 204 No Content (no envelope body, matches the
    // decided delete contract); failures map to the standard error envelope.
    // Deferred work: migrate every non-delete operation that still returns a
    // string payload message (e.g. Role/Permission updates) away from
    // Result<string> towards typed response DTOs.
    public static ActionResult<APIResponse> ToNoContentActionResult(this Result result)
    {
        if (result.IsSuccess)
        {
            return new ActionResult<APIResponse>(new NoContentResult());
        }

        return ErrorResult<object>(result.Failure!);
    }

    private static APIResponse<T> SuccessEnvelope<T>(T? value)
    {
        return new APIResponse<T>
        {
            IsSuccess = true,
            StatusCode = HttpStatusCode.OK,
            Result = value
        };
    }

    private static ObjectResult ErrorResult<T>(AppError error)
    {
        var response = new APIResponse<T>
        {
            IsSuccess = false,
            StatusCode = MapToHttpStatus(error.Type),
            ErrorMessage = error.ValidationErrors.Count > 0
                ? error.ValidationErrors.ToList()
                : new List<string> { error.Message }
        };

        return new ObjectResult(response) { StatusCode = (int)response.StatusCode };
    }

    // The authoritative ErrorType -> HTTP table.
    private static HttpStatusCode MapToHttpStatus(ErrorType type)
    {
        return type switch
        {
            ErrorType.Validation => HttpStatusCode.BadRequest,
            ErrorType.NotFound => HttpStatusCode.NotFound,
            ErrorType.Conflict => HttpStatusCode.Conflict,
            ErrorType.Unauthorized => HttpStatusCode.Unauthorized,
            ErrorType.Forbidden => HttpStatusCode.Forbidden,
            _ => HttpStatusCode.BadRequest
        };
    }
}
