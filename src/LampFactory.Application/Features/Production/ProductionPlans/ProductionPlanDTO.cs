

using LampFactory.Application.Features.Production.ProductionPlans;

namespace LampFactory.Application.Features.Production.ProductionPlans
{
    public class ProductionPlanDTO
    {
        public ProductionPlan ProductionPlan { get; set; } = new ProductionPlan();
        public List<ProductionPlanProduct> Products { get; set; } = new List<ProductionPlanProduct>();
    }
}
