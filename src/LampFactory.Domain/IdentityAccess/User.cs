namespace LampFactory.Domain.IdentityAccess
{
    public class User : AuditableEntity
    {
        public int UserID { get; set; }
        public string Username { get; set; }
        public string Userpass { get; set; }
        public string? Fullname { get; set; }
        public string? Phone { get; set; }
    }

}
