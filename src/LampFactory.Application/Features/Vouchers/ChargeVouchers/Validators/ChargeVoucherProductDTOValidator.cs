using FluentValidation;

using LampFactory.Application.Features.Vouchers.ChargeVouchers;

namespace LampFactory.Application.Features.Vouchers.ChargeVouchers.Validators
{
    public class ChargeVoucherProductDTOValidator : AbstractValidator<ChargeVoucherProductDTO>
    {
        public ChargeVoucherProductDTOValidator()
        {
            RuleFor(x => x)
                .NotNull().WithMessage("Request body cannot be null.");

            RuleFor(x => x.Voucher)
                .NotNull().WithMessage("ChargeVoucher data is required.");

            RuleFor(x => x.Products)
                .Must(p => p != null && p.Count > 0).WithMessage("At least one product item is required.");

            RuleForEach(x => x.Products).ChildRules(product =>
            {
                // ChargeVoucherID is an IDENTITY column assigned by the repository
                // (SCOPE_IDENTITY on the header insert), never read from the product
                // rows — requiring it here rejects every create, since the ID does not
                // exist yet when the client submits the payload.
                product.RuleFor(p => p.ProductID)
                    .GreaterThan(0).WithMessage("ProductID must be greater than zero.");

                product.RuleFor(p => p.ChargeQuantity)
                    .GreaterThan(0).When(p => p.ChargeQuantity.HasValue)
                    .WithMessage("Quantity must be greater than zero.");
            });
        }
    }
}