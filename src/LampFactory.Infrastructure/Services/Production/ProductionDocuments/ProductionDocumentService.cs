using FluentValidation;
using LampFactory.Application.Common.Results;
using LampFactory.Application.Features.Production.ProductionDocuments;
using LampFactory.Application.Abstractions.Persistence.Production.ProductionDocuments;

namespace LampFactory.Infrastructure.Services.Production.ProductionDocuments
{
    public class ProductionDocumentService : IProductionDocumentService
    {
        private readonly IProductionDocumentRepository _documentRepository;
        private readonly IValidator<ProductionDocumentDTO> _validator;

        public ProductionDocumentService(IProductionDocumentRepository documentRepository, IValidator<ProductionDocumentDTO> validator)
        {
            _documentRepository = documentRepository;
            _validator = validator;
        }

        public async Task<Result<List<object>>> GetAllDocumentsAsync()
        {

            var documents = await _documentRepository.GetAllWithOperatorsAsync();

            var data = documents.Select(d => new
            {
                ProductionDocument = d.Document,
                Operators = d.Operators
            }).ToList<object>();

            return Result<List<object>>.Success(data);
        }

        public async Task<Result<object>> GetDocumentByIdAsync(int id)
        {

            if (id <= 0)
            {
                return Result<object>.BadRequest("Invalid Document ID.");
            }

            var resultData = await _documentRepository.GetByIdWithOperatorsAsync(id);
            if (resultData.Document == null)
            {
                return Result<object>.NotFound("Production document not found.");
            }

            return Result<object>.Success(new
            {
                ProductionDocument = resultData.Document,
                Operators = resultData.Operators
            });
        }

        public async Task<Result<object>> CreateDocumentAsync(ProductionDocumentDTO dto)
        {

            var validationResult = await _validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                return Result<object>.BadRequest("Validation failed", validationResult.Errors.Select(e => e.ErrorMessage).ToList());
            }

            var newDocumentId = await _documentRepository.CreateWithOperatorsAsync(dto.ProductionDocument, dto.Operators);
            if (newDocumentId <= 0)
            {
                return Result<object>.BadRequest("Failed to create production document.");
            }

            var created = await _documentRepository.GetByIdWithOperatorsAsync(newDocumentId);
            return Result<object>.Success(new
            {
                ProductionDocument = created.Document,
                Operators = created.Operators
            });
        }

        public async Task<Result<ProductionDocumentDTO>> UpdateDocumentAsync(int id, ProductionDocumentDTO dto)
        {

            var context = new ValidationContext<ProductionDocumentDTO>(dto);
            context.RootContextData["RouteId"] = id;

            var validationResult = await _validator.ValidateAsync(context);
            if (!validationResult.IsValid)
            {
                return Result<ProductionDocumentDTO>.BadRequest("Validation failed", validationResult.Errors.Select(e => e.ErrorMessage).ToList());
            }

            var existingDocument = await _documentRepository.GetByIdWithOperatorsAsync(id);
            if (existingDocument.Document == null)
            {
                return Result<ProductionDocumentDTO>.NotFound("Production document not found for update.");
            }

            var success = await _documentRepository.UpdateWithOperatorsAsync(dto.ProductionDocument, dto.Operators);
            if (!success)
            {
                return Result<ProductionDocumentDTO>.BadRequest("Failed to update production document.");
            }

            return Result<ProductionDocumentDTO>.Success(dto);
        }

        public async Task<Result> DeleteDocumentAsync(int id)
        {

            var existingDocument = await _documentRepository.GetByIdWithOperatorsAsync(id);
            if (existingDocument.Document == null)
            {
                return Result.NotFound("Production document not found for deletion.");
            }

            var success = await _documentRepository.RemoveWithOperatorsAsync(id);
            if (!success)
            {
                return Result.BadRequest("Failed to delete production document.");
            }

            return Result.Success();
        }
    }
}