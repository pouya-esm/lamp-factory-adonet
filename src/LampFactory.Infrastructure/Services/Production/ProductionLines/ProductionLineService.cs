using FluentValidation;
using LampFactory.Application.Common.Results;
using LampFactory.Application.Features.Production.ProductionLines;
using LampFactory.Application.Abstractions.Persistence.Production.ProductionLines;
using LampFactory.Domain.Production;

namespace LampFactory.Infrastructure.Services.Production.ProductionLines
{
    public class ProductionLineService : IProductionLineService
    {
        private readonly IProductionLineRepository _productionLineRepository;
        private readonly IValidator<ProductionLineDTO> _validator;

        public ProductionLineService(IProductionLineRepository productionLineRepository, IValidator<ProductionLineDTO> validator)
        {
            _productionLineRepository = productionLineRepository;
            _validator = validator;
        }

        public async Task<Result<List<ProductionLine>>> GetAllProductionLinesAsync()
        {

            var lines = await _productionLineRepository.GetAllAsync();
            return Result<List<ProductionLine>>.Success(lines);
        }

        public async Task<Result<ProductionLine>> GetProductionLineByIdAsync(int id)
        {

            if (id <= 0)
            {
                return Result<ProductionLine>.BadRequest("Invalid ProductionLine ID.");
            }

            var line = await _productionLineRepository.GetByIdAsync(id);
            if (line == null)
            {
                return Result<ProductionLine>.NotFound("ProductionLine not found.");
            }

            return Result<ProductionLine>.Success(line);
        }

        public async Task<Result<ProductionLine>> CreateProductionLineAsync(ProductionLineDTO dto)
        {

            var validationResult = await _validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                return Result<ProductionLine>.BadRequest("Validation failed", validationResult.Errors.Select(e => e.ErrorMessage).ToList());
            }

            var newLineId = await _productionLineRepository.CreateAsync(dto.ProductionLine);
            if (newLineId <= 0)
            {
                return Result<ProductionLine>.BadRequest("Failed to create ProductionLine.");
            }

            var created = await _productionLineRepository.GetByIdAsync(newLineId);
            return Result<ProductionLine>.Success(created);
        }

        public async Task<Result<ProductionLine>> UpdateProductionLineAsync(int id, ProductionLineDTO dto)
        {

            if (dto == null || dto.ProductionLine == null || id != dto.ProductionLine.ProductionLineID)
            {
                return Result<ProductionLine>.BadRequest("ProductionLine ID mismatch or invalid data.");
            }

            var existingLine = await _productionLineRepository.GetByIdAsync(id);
            if (existingLine == null)
            {
                return Result<ProductionLine>.NotFound("ProductionLine not found for update.");
            }

            var success = await _productionLineRepository.UpdateAsync(dto.ProductionLine);
            if (!success)
            {
                return Result<ProductionLine>.BadRequest("Failed to update ProductionLine.");
            }

            var updated = await _productionLineRepository.GetByIdAsync(id);
            return Result<ProductionLine>.Success(updated);
        }

        public async Task<Result> DeleteProductionLineAsync(int id)
        {

            var existingLine = await _productionLineRepository.GetByIdAsync(id);
            if (existingLine == null)
            {
                return Result.NotFound("ProductionLine not found for deletion.");
            }

            var success = await _productionLineRepository.RemoveAsync(id);
            if (!success)
            {
                return Result.BadRequest("Failed to delete ProductionLine.");
            }

            return Result.Success();
        }
    }
}