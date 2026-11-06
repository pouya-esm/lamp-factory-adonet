namespace LampFactory.Domain.IdentityAccess
{
    public class Resource : AuditableEntity
    {
        public int ResourceID { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
