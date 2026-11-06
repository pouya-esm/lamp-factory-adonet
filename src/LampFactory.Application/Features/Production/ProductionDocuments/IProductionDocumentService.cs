using LampFactory.Application.Common.Results;
using LampFactory.Application.Features.Production.ProductionDocuments;

namespace LampFactory.Application.Features.Production.ProductionDocuments;

public interface IProductionDocumentService
{
    Task<Result<List<object>>> GetAllDocumentsAsync();
    Task<Result<object>> GetDocumentByIdAsync(int id);
    Task<Result<object>> CreateDocumentAsync(ProductionDocumentDTO dto);
    Task<Result<ProductionDocumentDTO>> UpdateDocumentAsync(int id, ProductionDocumentDTO dto);
    Task<Result> DeleteDocumentAsync(int id);
}