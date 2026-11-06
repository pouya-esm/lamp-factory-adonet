namespace LampFactory.Domain.Production
{
    public class ProductionDocument : AuditableEntity
    {
        public int ProductionDocumentID { get; set; }
        public DateTime? ProductionDate { get; set; }
        public int? ProducedQuantity { get; set; }
        public int? RejectedQuantity { get; set; }
        public int? ProductionShift { get; set; }
        public int ProductionPlanID { get; set; }
        public ProductionPlan? ProductionPlan { get; set; }
        public int ProductID { get; set; }
        public Product? Product { get; set; }
        public int ProductionLineID { get; set; }
        public ProductionLine? productionLine { get; set; }
    }
}
