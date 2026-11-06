using LampFactory.Application.Common.Results;
using LampFactory.Application.Features.Production.ProductionLines;
using LampFactory.Domain.Production;

namespace LampFactory.Application.Features.Production.ProductionLines;

public interface IProductionLineService
{
    Task<Result<List<ProductionLine>>> GetAllProductionLinesAsync();
    Task<Result<ProductionLine>> GetProductionLineByIdAsync(int id);
    Task<Result<ProductionLine>> CreateProductionLineAsync(ProductionLineDTO dto);
    Task<Result<ProductionLine>> UpdateProductionLineAsync(int id, ProductionLineDTO dto);
    Task<Result> DeleteProductionLineAsync(int id);
}