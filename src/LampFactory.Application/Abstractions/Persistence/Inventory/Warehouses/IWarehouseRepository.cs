

namespace LampFactory.Application.Abstractions.Persistence.Inventory.Warehouses;

public interface IWarehouseRepository
{
    Task<int> CreateWithProductsAsync(Warehouse warehouse, List<WarehouseProduct> products);
    Task<List<(Warehouse Warehouse, List<WarehouseProduct> Stock)>> GetAllWithStockAsync();
    Task<(Warehouse? Warehouse, List<WarehouseProduct> Stock)> GetByIdWithStockAsync(int warehouseId);
    Task<bool> RemoveWithProductsAsync(int warehouseId);
    Task<bool> UpdateWithProductsAsync(Warehouse warehouse, List<WarehouseProduct> products);
}
