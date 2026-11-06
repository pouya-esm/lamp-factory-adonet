

namespace LampFactory.Application.Abstractions.Persistence.Production.BillOfMaterials;

public interface IBOMRepository
{
    Task<int> CreateWithComponentsAsync(BOM bom, List<BOMComponent> components);
    Task<(BOM? Bom, List<BOMComponent> Components)> GetByIdWithComponentsAsync(int bomId);
    Task<List<(BOM Bom, List<BOMComponent> Components)>> GetAllWithComponentsAsync();
    Task<bool> RemoveWithComponentsAsync(int bomId);
    Task<bool> UpdateWithComponentsAsync(BOM bom, List<BOMComponent> components);
}
