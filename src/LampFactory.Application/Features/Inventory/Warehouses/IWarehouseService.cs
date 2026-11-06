using LampFactory.Application.Common.Results;
using LampFactory.Application.Features.Inventory.Warehouses;

namespace LampFactory.Application.Features.Inventory.Warehouses;

public interface IWarehouseService
{
    Task<Result<List<WarehouseDetailsDTO>>> GetAllWarehousesAsync();
    Task<Result<WarehouseDetailsDTO>> GetWarehouseByIdAsync(int id);
    Task<Result<WarehouseDetailsDTO>> CreateWarehouseAsync(WarehouseDetailsDTO dto);
    Task<Result<WarehouseDetailsDTO>> UpdateWarehouseAsync(int id, WarehouseDetailsDTO dto);
    Task<Result> DeleteWarehouseAsync(int id);
}