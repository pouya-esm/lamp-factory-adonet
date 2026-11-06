using FluentValidation;

namespace LampFactory.Application.Features.Production.ProductionLines.Validators
{
    public class ProductionLineValidator : AbstractValidator<ProductionLineDTO>
    {
        public ProductionLineValidator()
        {
            RuleFor(x => x)
                .NotNull().WithMessage("Request body cannot be null.");

            RuleFor(x => x.ProductionLine)
                .NotNull().WithMessage("ProductionLine data is required.");
        }
    }
}
