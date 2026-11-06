using LampFactory.Application.Common.Results;
using LampFactory.Application.Features.IdentityAccess.Roles;

namespace LampFactory.Application.Features.IdentityAccess.Roles;

public interface IRoleService
{
    Task<Result<List<RoleDetailDTO>>> GetAllRolesAsync();
    Task<Result<RoleDetailDTO>> CreateRoleAsync(string roleName, string? description, List<int> permissionIds);
    Task<Result<string>> UpdateRoleAsync(int id, string roleName, string? description, List<int> permissionIds);
    Task<Result> DeleteRoleAsync(int id);
}