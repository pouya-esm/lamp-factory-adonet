using LampFactory.Domain.Enums;

namespace LampFactory.Domain.Inventory
{
    public class Product : AuditableEntity
    {
        public int ProductID { get; set; }
        public ProductType ProductType { get; set; }
        public string? ProductColor { get; set; }
        public string? Material { get; set; }
        public string? StandardNumber { get; set; }
        public double? Weight { get; set; }
        public double? Voltage { get; set; }
        public double? Power { get; set; }
        public string? LampholderType { get; set; }
        public string? ConsumptionUnit { get; set; }
    }
}
