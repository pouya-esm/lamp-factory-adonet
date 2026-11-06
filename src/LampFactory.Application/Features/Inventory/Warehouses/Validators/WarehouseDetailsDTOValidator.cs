using FluentValidation;

using LampFactory.Application.Features.Inventory.Warehouses;

namespace LampFactory.Application.Features.Inventory.Warehouses.Validators
{
    public class WarehouseDetailsDTOValidator : AbstractValidator<WarehouseDetailsDTO>
    {
        public WarehouseDetailsDTOValidator()
        {
            // Checking that the entire request body is not null
            RuleFor(x => x)
                .NotNull().WithMessage("Request body cannot be null.");

            // Checking if the warehouse entity is not null
            RuleFor(x => x.Warehouse)
                .NotNull().WithMessage("Warehouse data is required.");

            When(x => x != null && x.Warehouse != null, () =>
            {
                RuleFor(x => x.Warehouse.WarehouseType)
                    .IsInEnum().WithMessage("Invalid WarehouseType value.");

                RuleFor(x => x.Warehouse.Name)
                    .MaximumLength(30).WithMessage("Warehouse name cannot exceed 30 characters.");
            });

            // Reviewing the product list
            RuleForEach(x => x.Products).ChildRules(product =>
            {
                product.RuleFor(p => p.ProductID)
                    .GreaterThan(0).WithMessage("ProductID must be greater than zero.");

                product.RuleFor(p => p.Quantity)
                    .GreaterThanOrEqualTo(0).When(p => p.Quantity.HasValue)
                    .WithMessage("Quantity cannot be negative.");
            });

            // Validating the ID passed via the route (URL) and matching it with the warehouseID.
            RuleFor(x => x).Custom((dto, context) =>
            {
                if (context.RootContextData.TryGetValue("RouteId", out var routeIdObj) && routeIdObj is int routeId)
                {
                    if (routeId <= 0)
                    {
                        context.AddFailure("Invalid Warehouse ID.");
                    }

                    if (dto?.Warehouse != null && dto.Warehouse.WarehouseID != routeId)
                    {
                        context.AddFailure("Warehouse ID mismatch.");
                    }
                }
            });

        }
    }
}
