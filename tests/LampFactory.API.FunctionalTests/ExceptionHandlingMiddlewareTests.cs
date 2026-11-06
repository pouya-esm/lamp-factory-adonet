using LampFactory.API.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;

namespace LampFactory.API.FunctionalTests;

// The centralized handler must log-and-hide: API clients never see internals.
public class ExceptionHandlingMiddlewareTests
{
    [Fact]
    public async Task UnexpectedException_ReturnsGeneric500WithoutInternalDetails()
    {
        var context = BuildHttpContext(out var body);
        // InvalidOperationException passes through the specific exception
        // catches and must land in the generic handler (CA2201 prefers a more
        // specific type; plain Exception would still be valid here but the
        // more specific type also exercises the same fallback branch).
        var thrown = new InvalidOperationException("SELECT failed: connection string leaked!");

        await new ExceptionHandlingMiddleware(
                _ => throw thrown,
                NullLogger<ExceptionHandlingMiddleware>.Instance)
            .InvokeAsync(context);

        var json = ReadBody(body);
        Assert.Equal(500, context.Response.StatusCode);
        Assert.Contains("An unexpected error occurred.", json);
        Assert.DoesNotContain("connection string", json);
        Assert.DoesNotContain("SELECT failed", json);
    }

    [Fact]
    public async Task ConflictException_Returns409WithBusinessMessage()
    {
        var context = BuildHttpContext(out var body);

        await new ExceptionHandlingMiddleware(
                _ => throw new LampFactory.Application.Common.Exceptions.ConflictException("Insufficient stock."),
                NullLogger<ExceptionHandlingMiddleware>.Instance)
            .InvokeAsync(context);

        var json = ReadBody(body);
        Assert.Equal(409, context.Response.StatusCode);
        Assert.Contains("Insufficient stock.", json);
    }

    [Fact]
    public async Task NotFoundException_Returns404()
    {
        var context = BuildHttpContext(out var body);

        await new ExceptionHandlingMiddleware(
                _ => throw new LampFactory.Application.Common.Exceptions.NotFoundException("Product", 7),
                NullLogger<ExceptionHandlingMiddleware>.Instance)
            .InvokeAsync(context);

        var json = ReadBody(body);
        Assert.Equal(404, context.Response.StatusCode);
        Assert.Contains("Product", json);
    }

    private static DefaultHttpContext BuildHttpContext(out MemoryStream body)
    {
        body = new MemoryStream();
        var context = new DefaultHttpContext { Response = { Body = body } };
        context.Request.Scheme = "http";
        return context;
    }

    private static string ReadBody(MemoryStream stream)
    {
        stream.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(stream, leaveOpen: true);
        return reader.ReadToEnd();
    }
}
