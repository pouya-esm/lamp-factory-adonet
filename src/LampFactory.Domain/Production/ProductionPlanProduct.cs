namespace LampFactory.Domain.Production
{
    public class ProductionPlanProduct
    {
        public int? MinQuantity { get; set; }
        public int? MaxQuantity { get; set; }
        public int ProductionPlanID { get; set; }
        public int ProductID { get; set; }
        public ProductionPlan? ProductionPlan { get; set; }
        public Product? Product { get; set; }
    }
}
