

using LampFactory.Application.Features.Inventory.Warehouses;

namespace LampFactory.Application.Features.Inventory.Warehouses
{
    public class WarehouseDetailsDTO
    {
        public Warehouse Warehouse { get; set; }
        public List<WarehouseProduct> Products { get; set; } = new List<WarehouseProduct>();
    }
}
