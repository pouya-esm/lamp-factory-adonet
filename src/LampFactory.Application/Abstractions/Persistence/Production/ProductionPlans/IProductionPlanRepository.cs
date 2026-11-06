

namespace LampFactory.Application.Abstractions.Persistence.Production.ProductionPlans;

public interface IProductionPlanRepository
{
    Task<int> CreateWithProductsAsync(ProductionPlan plan, List<ProductionPlanProduct> products);
    Task<(ProductionPlan? Plan, List<ProductionPlanProduct> Products)> GetByIdWithProductsAsync(int id);
    Task<List<(ProductionPlan Plan, List<ProductionPlanProduct> Products)>> GetAllWithProductsAsync();
    Task<bool> RemoveWithProductsAsync(int planId);
    Task<bool> UpdateWithProductsAsync(ProductionPlan plan, List<ProductionPlanProduct> products);
}
