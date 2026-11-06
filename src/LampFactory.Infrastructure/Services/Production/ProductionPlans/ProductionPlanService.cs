using FluentValidation;
using LampFactory.Application.Common.Results;
using LampFactory.Application.Features.Production.ProductionPlans;
using LampFactory.Application.Abstractions.Persistence.Production.ProductionPlans;

namespace LampFactory.Infrastructure.Services.Production.ProductionPlans
{
    public class ProductionPlanService : IProductionPlanService
    {
        private readonly IProductionPlanRepository _productionPlanRepository;
        private readonly IValidator<ProductionPlanDTO> _validator;

        public ProductionPlanService(IProductionPlanRepository productionPlanRepository, IValidator<ProductionPlanDTO> validator)
        {
            _productionPlanRepository = productionPlanRepository;
            _validator = validator;
        }

        public async Task<Result<List<object>>> GetAllProductionPlansAsync()
        {

            var plans = await _productionPlanRepository.GetAllWithProductsAsync();

            var data = plans.Select(p => new
            {
                ProductionPlan = p.Plan,
                Products = p.Products
            }).ToList<object>();

            return Result<List<object>>.Success(data);
        }

        public async Task<Result<object>> GetProductionPlanByIdAsync(int id)
        {

            if (id <= 0)
            {
                return Result<object>.BadRequest("Invalid ProductionPlan ID.");
            }

            var resultData = await _productionPlanRepository.GetByIdWithProductsAsync(id);
            if (resultData.Plan == null)
            {
                return Result<object>.NotFound("ProductionPlan not found.");
            }

            return Result<object>.Success(new
            {
                ProductionPlan = resultData.Plan,
                Products = resultData.Products
            });
        }

        public async Task<Result<object>> CreateProductionPlanAsync(ProductionPlanDTO dto)
        {

            var validationResult = await _validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                return Result<object>.BadRequest("Validation failed", validationResult.Errors.Select(e => e.ErrorMessage).ToList());
            }

            var newPlanId = await _productionPlanRepository.CreateWithProductsAsync(dto.ProductionPlan, dto.Products);
            if (newPlanId <= 0)
            {
                return Result<object>.BadRequest("Failed to create ProductionPlan.");
            }

            var created = await _productionPlanRepository.GetByIdWithProductsAsync(newPlanId);
            return Result<object>.Success(new
            {
                ProductionPlan = created.Plan,
                Products = created.Products
            });
        }

        public async Task<Result<ProductionPlanDTO>> UpdateProductionPlanAsync(int id, ProductionPlanDTO dto)
        {

            var context = new ValidationContext<ProductionPlanDTO>(dto);
            context.RootContextData["RouteId"] = id;

            var validationResult = await _validator.ValidateAsync(context);
            if (!validationResult.IsValid)
            {
                return Result<ProductionPlanDTO>.BadRequest("Validation failed", validationResult.Errors.Select(e => e.ErrorMessage).ToList());
            }

            var existingPlanData = await _productionPlanRepository.GetByIdWithProductsAsync(id);
            if (existingPlanData.Plan == null)
            {
                return Result<ProductionPlanDTO>.NotFound("ProductionPlan not found for update.");
            }

            var success = await _productionPlanRepository.UpdateWithProductsAsync(dto.ProductionPlan, dto.Products);
            if (!success)
            {
                return Result<ProductionPlanDTO>.BadRequest("Failed to update ProductionPlan.");
            }

            return Result<ProductionPlanDTO>.Success(dto);
        }

        public async Task<Result> DeleteProductionPlanAsync(int id)
        {

            var existingPlanData = await _productionPlanRepository.GetByIdWithProductsAsync(id);
            if (existingPlanData.Plan == null)
            {
                return Result.NotFound("ProductionPlan not found for deletion.");
            }

            var success = await _productionPlanRepository.RemoveWithProductsAsync(id);
            if (!success)
            {
                return Result.BadRequest("Failed to delete ProductionPlan.");
            }

            return Result.Success();
        }
    }
}