namespace LampFactory.Domain.Production
{
    public class BOMComponent
    {
        public double? ConsumptionAmount { get; set; }
        public int BOM_ID { get; set; }
        public int ProductID { get; set; }
        public Product? Product { get; set; }
        public BOM? BOM { get; set; }
    }
}
