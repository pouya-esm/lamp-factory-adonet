using FluentValidation;

using LampFactory.Application.Features.Production.ProductionPlans;

namespace LampFactory.Application.Features.Production.ProductionPlans.Validators
{
    public class ProductionPlanDTOValidator : AbstractValidator<ProductionPlanDTO>
    {
        public ProductionPlanDTOValidator()
        {
            RuleFor(x => x)
                .NotNull().WithMessage("Request body cannot be null.");

            RuleFor(x => x.ProductionPlan)
                .NotNull().WithMessage("ProductionPlan data is required.");

            RuleFor(x => x.Products)
                .Must(p => p != null && p.Count > 0).WithMessage("At least one product must be specified for the production plan.");

            // Validating the ID passed via the route (URL) and matching it with the ProductionPlanId.
            RuleFor(x => x).Custom((dto, context) =>
            {
                if (context.RootContextData.TryGetValue("RouteId", out var routeIdObj) && routeIdObj is int routeId)
                {
                    if (routeId <= 0)
                    {
                        context.AddFailure("Invalid ProductionPlan ID.");
                    }

                    if (dto?.ProductionPlan != null && dto.ProductionPlan.ProductionPlanId != routeId)
                    {
                        context.AddFailure("ProductionPlan ID mismatch.");
                    }
                }
            });
        }
    }
}