using LampFactory.Application.Common.Results;

namespace LampFactory.Application.Features.Production.BillOfMaterials;

public interface IBOMService
{
    Task<Result<IReadOnlyList<BOMWithComponentsDTO>>> GetAllBOMsAsync();
    Task<Result<BOMWithComponentsDTO>> GetBOMByIdAsync(int id);
    Task<Result<BOMWithComponentsDTO>> CreateBOMAsync(BOMDTO dto);
    Task<Result<BOMWithComponentsDTO>> UpdateBOMAsync(int id, BOMDTO dto);
    Task<Result> DeleteBOMAsync(int id);
}
