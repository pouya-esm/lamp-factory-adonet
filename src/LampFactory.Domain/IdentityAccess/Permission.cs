namespace LampFactory.Domain.IdentityAccess
{
    public class Permission : AuditableEntity
    {
        public int PermissionID { get; set; }
        public int ResourceID { get; set; }
        public Resource? Resource { get; set; }
        public int ActionID { get; set; }
        public SysAction? Action { get; set; }
    }
}
