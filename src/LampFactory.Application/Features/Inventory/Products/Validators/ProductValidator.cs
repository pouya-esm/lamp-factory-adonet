using FluentValidation;

namespace LampFactory.Application.Features.Inventory.Products.Validators
{
    public class ProductValidator : AbstractValidator<ProductDTO>
    {
        public ProductValidator()
        {
            RuleFor(x => x)
                .NotNull().WithMessage("Request body cannot be null.");

            RuleFor(x => x.Product)
                .NotNull().WithMessage("Product data is required.");

            When(x => x.Product != null, () =>
            {
                RuleFor(x => x.Product.ProductType)
                    .IsInEnum().WithMessage("Invalid ProductType value.");

                RuleFor(x => x.Product.ProductColor)
                    .MaximumLength(50).WithMessage("Product color cannot exceed 50 characters.");

                RuleFor(x => x.Product.Material)
                    .MaximumLength(50).WithMessage("Material cannot exceed 50 characters.");

                RuleFor(x => x.Product.StandardNumber)
                    .MaximumLength(50).WithMessage("Standard number cannot exceed 50 characters.");

                RuleFor(x => x.Product.LampholderType)
                    .MaximumLength(50).WithMessage("Lampholder type cannot exceed 50 characters.");

                RuleFor(x => x.Product.ConsumptionUnit)
                    .MaximumLength(50).WithMessage("Consumption unit cannot exceed 50 characters.");
            });
        }
    }
}
