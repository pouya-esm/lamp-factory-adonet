using LampFactory.Application.Features.Production.ProductionLines;

namespace LampFactory.Application.Features.Production.ProductionLines
{
    // Write-side contract for the ProductionLine aggregate. The API binds this,
    // not the Domain entity, so the presentation layer never shapes aggregate state.
    public class ProductionLineDTO
    {
        public ProductionLine ProductionLine { get; set; } = new();
    }
}
