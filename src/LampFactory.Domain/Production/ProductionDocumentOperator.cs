namespace LampFactory.Domain.Production
{
    public class ProductionDocumentOperator
    {
        public string? OperatorRole { get; set; }
        public DateTime? AssignedDateFrom { get; set; }
        public DateTime? AssignedDateUntil { get; set; }
        public int ProductionDocumentID { get; set; }
        public ProductionDocument? ProductionDocument { get; set; }
        public int OperatorID { get; set; }
        public Operator? Operator { get; set; }
    }
}
