
using LampFactory.Application.Features.IdentityAccess.Roles;

namespace LampFactory.Application.Features.IdentityAccess.Roles
{
    public class RoleUpdateDTO
    {
        public string RoleName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public List<int> PermissionIds { get; set; } = new List<int>();
    }

}
