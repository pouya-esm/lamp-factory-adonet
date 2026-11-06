
using LampFactory.Application.Features.IdentityAccess.Permissions;

namespace LampFactory.Application.Features.IdentityAccess.Permissions
{
    public class PermissionDTO
    {
        public int PermissionID { get; set; }
        public int ResourceID { get; set; }
        public string ResourceName { get; set; } = string.Empty;
        public int ActionID { get; set; }
        public string ActionName { get; set; } = string.Empty;

        // Audit stamp, copied straight off the entity in PermissionService so
        // the Permissions table can render "Last change".
        public DateTime? CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public int? ModifiedBy { get; set; }
    }
}
