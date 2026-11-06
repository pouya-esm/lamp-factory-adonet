using LampFactory.Domain.IdentityAccess;

namespace LampFactory.Application.Abstractions.Persistence.IdentityAccess.Users;

// IRepository<User> supplies GetAll/GetById/Remove/Create/Update; this extends
// it with the user-role linkage the user service and auth service rely on.
public interface IUserRepository : IRepository<User>
{
    Task<int> CreateWithRolesAsync(User user, List<int> roleIds);
    Task<bool> UpdateWithRolesAsync(User user, List<int> roleIds);
    Task<List<(int UserId, string Username)>> GetLookupAsync();
    Task<List<int>> GetRoleIdsForUserAsync(int userId);
}
