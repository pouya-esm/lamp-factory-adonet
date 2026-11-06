using LampFactory.Application.Common.Results;
using LampFactory.Application.Features.Production.ProductionPlans;

namespace LampFactory.Application.Features.Production.ProductionPlans;

public interface IProductionPlanService
{
    Task<Result<List<object>>> GetAllProductionPlansAsync();
    Task<Result<object>> GetProductionPlanByIdAsync(int id);
    Task<Result<object>> CreateProductionPlanAsync(ProductionPlanDTO dto);
    Task<Result<ProductionPlanDTO>> UpdateProductionPlanAsync(int id, ProductionPlanDTO dto);
    Task<Result> DeleteProductionPlanAsync(int id);
}