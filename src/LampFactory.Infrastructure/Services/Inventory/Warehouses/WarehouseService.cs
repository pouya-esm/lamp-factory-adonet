using FluentValidation;
using LampFactory.Application.Common.Results;
using LampFactory.Application.Features.Inventory.Warehouses;
using LampFactory.Application.Abstractions.Persistence.Inventory.Warehouses;

namespace LampFactory.Infrastructure.Services.Inventory.Warehouses
{
    public class WarehouseService : IWarehouseService
    {
        private readonly IWarehouseRepository _warehouseRepository;
        private readonly IValidator<WarehouseDetailsDTO> _validator;

        public WarehouseService(IWarehouseRepository warehouseRepository, IValidator<WarehouseDetailsDTO> validator)
        {
            _warehouseRepository = warehouseRepository;
            _validator = validator;
        }

        public async Task<Result<List<WarehouseDetailsDTO>>> GetAllWarehousesAsync()
        {

            var data = await _warehouseRepository.GetAllWithStockAsync();
            var result = data.Select(d => new WarehouseDetailsDTO
            {
                Warehouse = d.Warehouse,
                Products = d.Stock
            }).ToList();

            return Result<List<WarehouseDetailsDTO>>.Success(result);
        }

        public async Task<Result<WarehouseDetailsDTO>> GetWarehouseByIdAsync(int id)
        {

            if (id <= 0)
                return Result<WarehouseDetailsDTO>.BadRequest("Invalid Warehouse ID.");

            var result = await _warehouseRepository.GetByIdWithStockAsync(id);

            if (result.Warehouse == null)
                return Result<WarehouseDetailsDTO>.NotFound("Warehouse not found.");

            var dto = new WarehouseDetailsDTO
            {
                Warehouse = result.Warehouse,
                Products = result.Stock
            };

            return Result<WarehouseDetailsDTO>.Success(dto);
        }

        public async Task<Result<WarehouseDetailsDTO>> CreateWarehouseAsync(WarehouseDetailsDTO dto)
        {

            var validationResult = await _validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                return Result<WarehouseDetailsDTO>.BadRequest("Validation failed", validationResult.Errors.Select(e => e.ErrorMessage).ToList());
            }

            var newWarehouseId = await _warehouseRepository.CreateWithProductsAsync(dto.Warehouse, dto.Products);
            if (newWarehouseId <= 0)
            {
                return Result<WarehouseDetailsDTO>.BadRequest("Failed to create warehouse.");
            }

            var created = await _warehouseRepository.GetByIdWithStockAsync(newWarehouseId);

            if (created.Warehouse == null)
            {
                return Result<WarehouseDetailsDTO>.NotFound("Warehouse could not be loaded after creation.");
            }

            return Result<WarehouseDetailsDTO>.Success(new WarehouseDetailsDTO { Warehouse = created.Warehouse, Products = created.Stock });
        }

        public async Task<Result<WarehouseDetailsDTO>> UpdateWarehouseAsync(int id, WarehouseDetailsDTO dto)
        {

            var context = new ValidationContext<WarehouseDetailsDTO>(dto);
            context.RootContextData["RouteId"] = id;

            var validationResult = await _validator.ValidateAsync(context);
            if (!validationResult.IsValid)
            {
                return Result<WarehouseDetailsDTO>.BadRequest("Validation failed", validationResult.Errors.Select(e => e.ErrorMessage).ToList());
            }

            var existing = await _warehouseRepository.GetByIdWithStockAsync(id);
            if (existing.Warehouse == null)
                return Result<WarehouseDetailsDTO>.NotFound("Warehouse not found.");

            var success = await _warehouseRepository.UpdateWithProductsAsync(dto.Warehouse, dto.Products);
            if (!success)
            {
                return Result<WarehouseDetailsDTO>.BadRequest("Failed to update warehouse.");
            }

            return Result<WarehouseDetailsDTO>.Success(dto);
        }

        public async Task<Result> DeleteWarehouseAsync(int id)
        {

            if (id <= 0)
                return Result.BadRequest("Invalid Warehouse ID.");

            var existing = await _warehouseRepository.GetByIdWithStockAsync(id);
            if (existing.Warehouse == null)
                return Result.NotFound("Warehouse not found.");

            var success = await _warehouseRepository.RemoveWithProductsAsync(id);
            if (!success)
            {
                return Result.BadRequest("Failed to delete warehouse.");
            }

            return Result.Success();
        }
    }
}