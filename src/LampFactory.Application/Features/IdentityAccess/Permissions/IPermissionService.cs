using LampFactory.Application.Common.Results;
using LampFactory.Application.Features.IdentityAccess.Permissions;
using LampFactory.Application.Features.IdentityAccess.Resources;

namespace LampFactory.Application.Features.IdentityAccess.Permissions;

public interface IPermissionService
{
    Task<bool> HasUserPermissionAsync(int userId, string resourceName, string actionName);
    Task<Result<List<ResourceDTO>>> GetAccessibleResourcesAsync();
    Task<Result<List<PermissionDTO>>> GetAllPermissionsAsync();
    Task<Result<PermissionDTO>> GetPermissionByIdAsync(int id);
    Task<Result<PermissionDTO>> CreatePermissionAsync(int resourceId, int actionId);
    Task<Result<string>> UpdatePermissionAsync(int id, int resourceId, int actionId);
    Task<Result> DeletePermissionAsync(int id);
}