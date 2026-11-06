
using LampFactory.Application.Features.IdentityAccess.Permissions;

namespace LampFactory.Application.Features.IdentityAccess.Permissions
{
    public class PermissionCreateDTO
    {
        public int ResourceID { get; set; }
        public int ActionID { get; set; }
    }
}
