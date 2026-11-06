

namespace LampFactory.Application.Abstractions.Persistence.Production.ProductionDocuments;

public interface IProductionDocumentRepository
{
    Task<int> CreateWithOperatorsAsync(ProductionDocument document, List<ProductionDocumentOperator> operators);
    Task<(ProductionDocument? Document, List<ProductionDocumentOperator> Operators)> GetByIdWithOperatorsAsync(int documentId);
    Task<List<(ProductionDocument Document, List<ProductionDocumentOperator> Operators)>> GetAllWithOperatorsAsync();
    Task<bool> UpdateWithOperatorsAsync(ProductionDocument document, List<ProductionDocumentOperator> operators);
    Task<bool> RemoveWithOperatorsAsync(int documentId);
}
