using LampFactory.API.Common.Models;
using LampFactory.Application.Common.Exceptions;
using System.Net;

namespace LampFactory.API.Middleware;

public partial class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    // CA1848: warnings on the hot path are logged through source-generated
    // LoggerMessage delegates instead of the allocation-heavy LoggerExtensions.
    [LoggerMessage(Level = LogLevel.Warning, Message = "Resource not found: {Message}")]
    private partial void LogResourceNotFound(string message);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Conflict: {Message}")]
    private partial void LogConflict(string message);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Unauthorized: {Message}")]
    private partial void LogUnauthorized(string message);

    [LoggerMessage(Level = LogLevel.Error, Message = "Unhandled exception: {Message}")]
    private partial void LogUnhandledException(string message);

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (NotFoundException ex)
        {
            LogResourceNotFound(ex.Message);
            await WriteAsync(context, HttpStatusCode.NotFound, ex.Message);
        }
        catch (ConflictException ex)
        {
            LogConflict(ex.Message);
            await WriteAsync(context, HttpStatusCode.Conflict, ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            LogUnauthorized(ex.Message);
            await WriteAsync(context, HttpStatusCode.Unauthorized, ex.Message);
        }
        catch (Exception ex)
        {
            LogUnhandledException(ex.Message);
            await WriteAsync(context, HttpStatusCode.InternalServerError, "An unexpected error occurred.");
        }
    }

    private static async Task WriteAsync(HttpContext context, HttpStatusCode statusCode, string message)
    {
        if (context.Response.HasStarted) return;

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        var response = new APIResponse
        {
            IsSuccess = false,
            StatusCode = statusCode,
            ErrorMessage = new List<string> { message }
        };

        await context.Response.WriteAsJsonAsync(response);
    }
}