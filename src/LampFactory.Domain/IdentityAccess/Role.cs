namespace LampFactory.Domain.IdentityAccess
{
    public class Role : AuditableEntity
    {
        public int RoleID { get; set; }
        public string RoleName { get; set; }
        public string? Description { get; set; }
    }
}
