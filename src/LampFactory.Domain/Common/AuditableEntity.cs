namespace LampFactory.Domain.Common
{
    // Audit columns shared by every business entity. Values are written by the
    // repositories from CurrentUserService.UserId, never trusted from the client,
    // so an actor can never spoof another user's id.
    public abstract class AuditableEntity
    {
        public DateTime? CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public int? ModifiedBy { get; set; }
    }
}
