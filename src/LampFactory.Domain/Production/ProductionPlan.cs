namespace LampFactory.Domain.Production
{
    public class ProductionPlan : AuditableEntity
    {
        public int ProductionPlanId { get; set; }
        public DateTime? IssueDate { get; set; }
        public DateTime? TargetDate { get; set; }
        public int? Priority { get; set; }
    }
}
