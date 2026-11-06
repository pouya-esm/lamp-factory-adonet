using LampFactory.API.Middleware;

namespace LampFactory.API.Extensions;

public static class DependencyInjectionExtensions
{
    // HTTP-specific wiring: middleware order only. The current-user adapter is
    // an Infrastructure concern now; AddInfrastructureServices registers both
    // ICurrentUserService and the IHttpContextAccessor it reads.
    public static WebApplication UseApiMiddleware(this WebApplication app)
    {
        // Registered first so it sees every downstream exception, including ones
        // thrown by model binding and the authorization filters.
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        return app;
    }
}