using FluentValidation;

using LampFactory.Application.Features.Production.BillOfMaterials;

namespace LampFactory.Application.Features.Production.BillOfMaterials.Validators
{
    public class BOMDTOValidator : AbstractValidator<BOMDTO>
    {
        public BOMDTOValidator()
        {
            RuleFor(x => x)
                .NotNull().WithMessage("Request body cannot be null.");

            RuleFor(x => x.BOM)
                .NotNull().WithMessage("BOM data is required.");

            When(x => x.BOM != null, () =>
            {
                RuleFor(x => x.BOM.ProductID)
                    .GreaterThan(0).WithMessage("Valid ProductID is required for BOM.");
            });

            RuleFor(x => x.Components)
                .Must(c => c != null && c.Count > 0).WithMessage("At least one component must be specified for BOM.");

            // Validating the ID passed via the route (URL) and matching it with the BOM_ID.
            RuleFor(x => x).Custom((dto, context) =>
            {
                if (context.RootContextData.TryGetValue("RouteId", out var routeIdObj) && routeIdObj is int routeId)
                {
                    if (routeId <= 0)
                    {
                        context.AddFailure("Invalid BOM ID.");
                    }

                    if (dto?.BOM != null && dto.BOM.BOM_ID != routeId)
                    {
                        context.AddFailure("BOM ID mismatch.");
                    }
                }
            });
        }
    }
}