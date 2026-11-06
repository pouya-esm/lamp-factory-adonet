namespace LampFactory.Application.Abstractions.Persistence.IdentityAccess.Permissions;

public interface IPermissionRepository : IRepository<Permission>
{
    Task<List<Resource>> GetAccessibleResourcesAsync(int userId);
    Task<bool> CheckUserPermissionAsync(int userId, string resourceName, string actionName);
}
