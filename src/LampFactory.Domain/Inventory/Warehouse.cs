using LampFactory.Domain.Enums;

namespace LampFactory.Domain.Inventory
{
    public class Warehouse : AuditableEntity
    {
        public int WarehouseID { get; set; }
        public WarehouseType WarehouseType { get; set; }
        public string? Name { get; set; }
        public string? Address { get; set; }
    }
}
