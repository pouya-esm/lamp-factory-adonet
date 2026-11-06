namespace LampFactory.Domain.Inventory
{
    public class WarehouseProduct
    {
        public int WarehouseID { get; set; }
        public Warehouse? Warehouse { get; set; }
        public int ProductID { get; set; }
        public Product? Product { get; set; }
        public int? Quantity { get; set; }
    }
}
