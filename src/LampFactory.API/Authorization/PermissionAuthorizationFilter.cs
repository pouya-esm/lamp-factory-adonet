using LampFactory.Application.Abstractions.Security;
using LampFactory.Application.Features.IdentityAccess.Permissions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace LampFactory.API.Authorization;

// Authorizes a request against the resource/action pair on the attribute by
// asking the permission service whether this user has that grant. Both
// dependencies are interfaces, so the filter carries no Infrastructure coupling.
public class PermissionAuthorizationFilter : IAsyncAuthorizationFilter
{
    private readonly string _resource;
    private readonly string _action;
    private readonly ICurrentUserService _currentUserService;
    private readonly IPermissionService _permissionService;

    public PermissionAuthorizationFilter(string resource, string action,
        ICurrentUserService currentUserService,
        IPermissionService permissionService)
    {
        _resource = resource;
        _action = action;
        _currentUserService = currentUserService;
        _permissionService = permissionService;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        // The user must be authenticated before a permission check means anything.
        var userId = _currentUserService.UserId;
        if (userId == null)
        {
            context.Result = new UnauthorizedResult(); // 401: not logged in.
            return;
        }

        bool hasPermission = await _permissionService.HasUserPermissionAsync(userId.Value, _resource, _action);

        if (!hasPermission)
        {
            context.Result = new ForbidResult(); // Authenticated, but lacks the grant.
        }
    }
}
