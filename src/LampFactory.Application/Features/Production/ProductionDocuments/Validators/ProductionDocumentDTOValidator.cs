using FluentValidation;

using LampFactory.Application.Features.Production.ProductionDocuments;

namespace LampFactory.Application.Features.Production.ProductionDocuments.Validators
{
    public class ProductionDocumentDTOValidator : AbstractValidator<ProductionDocumentDTO>
    {
        public ProductionDocumentDTOValidator()
        {
            RuleFor(x => x)
                .NotNull().WithMessage("Request body cannot be null.");

            RuleFor(x => x.ProductionDocument)
                .NotNull().WithMessage("Production document data is required.");

            // Validating the ID passed via the route (URL) and matching it with the ProductionDocumentID.
            RuleFor(x => x).Custom((dto, context) =>
            {
                if (context.RootContextData.TryGetValue("RouteId", out var routeIdObj) && routeIdObj is int routeId)
                {
                    if (routeId <= 0)
                    {
                        context.AddFailure("Invalid Document ID.");
                    }

                    if (dto?.ProductionDocument != null && dto.ProductionDocument.ProductionDocumentID != routeId)
                    {
                        context.AddFailure("Document ID mismatch.");
                    }
                }
            });
        }
    }
}