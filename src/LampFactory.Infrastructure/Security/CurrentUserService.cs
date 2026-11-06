using LampFactory.Application.Abstractions.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace LampFactory.Infrastructure.Security;

// Implements the application-layer current-user contract by reading the
// authenticated principal from the HTTP request pipeline. Application services
// and repositories depend on the interface, so neither ever touches HttpContext.
public partial class CurrentUserService : ICurrentUserService
{
    // The seeded system/admin account (UserID 1). Audit stamps must satisfy the
    // [User] foreign key, so every unresolvable actor falls back to this id
    // rather than dropping the stamp; the constant lives here so the fallback
    // has a single named owner.
    internal const int SystemUserId = 1;

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<CurrentUserService>? _logger;

    // CA1848: the rare warning on an anonymous mutating request goes through a
    // source-generated LoggerMessage delegate instead of LoggerExtensions.
    [LoggerMessage(Level = LogLevel.Warning,
        Message = "Anonymous or unresolvable HTTP request attributed to the system audit actor id {SystemUserId}.")]
    private partial void LogAnonymousSystemActor(int systemUserId);

    public CurrentUserService(IHttpContextAccessor httpContextAccessor, ILogger<CurrentUserService>? logger = null)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public int? UserId
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (user is { Identity.IsAuthenticated: true } &&
                user.FindFirst(ClaimTypes.NameIdentifier) is { } userIdClaim &&
                int.TryParse(userIdClaim.Value, out var id))
            {
                return id;
            }

            return null;
        }
    }

    // True when the audit stamp comes from the system fallback instead of an
    // authenticated principal. Consumers that care can branch on it, and the
    // two fallback reasons are distinguishable: no HttpContext at all means a
    // seed or background operation (a deliberately system-attributed write),
    // while an existing context with an unresolvable principal means an
    // anonymous or invalid HTTP request — logged, never silently trusted.
    public bool IsSystemActor => _httpContextAccessor.HttpContext is null || UserId is null;

    // The acting user, for audit columns. Background/seed paths (no HTTP
    // context) are explicitly identified as system operations above. For an
    // anonymous HTTP request the system id is still the stamp — the login and
    // refresh flows are the only such writers, and their rows must satisfy the
    // [User] foreign key — but the attribution is logged, not silent.
    public int ActorId
    {
        get
        {
            var isAnonymousHttpRequest = _httpContextAccessor.HttpContext is not null && UserId is null;
            if (isAnonymousHttpRequest)
            {
                LogAnonymousSystemActor(SystemUserId);
            }

            return UserId ?? SystemUserId;
        }
    }
}