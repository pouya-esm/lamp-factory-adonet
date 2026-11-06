namespace LampFactory.Domain.IdentityAccess
{
    public class SysAction : AuditableEntity
    {
        public int ActionID { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
