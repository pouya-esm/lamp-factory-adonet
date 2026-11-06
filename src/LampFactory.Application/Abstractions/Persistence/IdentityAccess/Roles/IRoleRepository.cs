using LampFactory.Domain.IdentityAccess;

namespace LampFactory.Application.Abstractions.Persistence.IdentityAccess.Roles;

// IRepository<Role> supplies GetAll/GetById/Remove/Create/Update; this extends
// it with the role-specific permission assignment the role service needs.
public interface IRoleRepository : IRepository<Role>
{
    Task<int> CreateWithPermissionsAsync(string roleName, string? description, List<int> permissionIds);
    Task<bool> UpdateWithPermissionsAsync(int roleId, string roleName, string? description, List<int> permissionIds);
    Task<List<int>> GetPermissionIdsForRoleAsync(int roleId);
}
