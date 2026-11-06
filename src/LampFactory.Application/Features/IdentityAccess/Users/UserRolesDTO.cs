

using LampFactory.Application.Features.IdentityAccess.Users;

namespace LampFactory.Application.Features.IdentityAccess.Users
{
    public class UserRolesDTO
    {
        public User User { get; set; } = new User();
        public List<int> RoleIds { get; set; } = new List<int>();
    }
}
