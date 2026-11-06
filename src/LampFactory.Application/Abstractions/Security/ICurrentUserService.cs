
using LampFactory.Application.Abstractions.Security;

namespace LampFactory.Application.Abstractions.Security;

// Abstraction over the current request's user so application services never
// touch HttpContext directly. Implemented in the API layer.
public interface ICurrentUserService
{
    // The authenticated user's id, or null when the request is anonymous.
    int? UserId { get; }

    // The acting user for audit columns. Falls back to a system id for
    // background/seed paths where no HTTP context exists.
    int ActorId { get; }

    // SQL-specific audit plumbing kept off the interface; implementations that
    // need it expose it on their own concrete type.
}
