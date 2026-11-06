namespace LampFactory.Domain.Production
{
    public class ProductionLine : AuditableEntity
    {
        public int ProductionLineID { get; set; }
        // Matches the [Status] BIT NOT NULL DEFAULT 0 column: false = Stopped,
        // true = Running. Non-nullable because the column can never be NULL.
        public bool Status { get; set; }
    }
}
