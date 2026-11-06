

using LampFactory.Application.Features.Production.ProductionDocuments;

namespace LampFactory.Application.Features.Production.ProductionDocuments
{
    public class ProductionDocumentDTO
    {
        public ProductionDocument ProductionDocument { get; set; } = new ProductionDocument();
        public List<ProductionDocumentOperator> Operators { get; set; } = new List<ProductionDocumentOperator>();
    }
}
