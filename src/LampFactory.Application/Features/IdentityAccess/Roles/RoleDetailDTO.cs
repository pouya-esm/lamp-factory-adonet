
using LampFactory.Application.Features.IdentityAccess.Roles;

namespace LampFactory.Application.Features.IdentityAccess.Roles
{
    public class RoleDetailDTO
    {
        public int RoleID { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public List<int> PermissionIds { get; set; } = new List<int>();

        // Audit stamp, copied straight off the entity in RoleService so the
        // Roles table can render "Last change" without an extra query.
        public DateTime? CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public int? ModifiedBy { get; set; }
    }
}
