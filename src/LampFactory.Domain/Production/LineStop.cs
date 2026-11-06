namespace LampFactory.Domain.Production
{
    public class LineStop : AuditableEntity
    {
        public int LineStopID { get; set; }
        public DateTime? LineStopDate { get; set; }
        public string? Reason { get; set; }
        public int ProductionDocumentID { get; set; }
        public ProductionDocument? ProductionDocument { get; set; }
    }
}
