using FluentValidation;

using LampFactory.Application.Features.Purchasing.RequestVouchers;

namespace LampFactory.Application.Features.Purchasing.RequestVouchers.Validators
{
    public class RequestVoucherProductDTOValidator : AbstractValidator<RequestVoucherProductDTO>
    {
        public RequestVoucherProductDTOValidator()
        {
            RuleFor(x => x)
                .NotNull().WithMessage("Request body cannot be null.");

            RuleFor(x => x.Voucher)
                .NotNull().WithMessage("Request Voucher data is required.");

            RuleFor(x => x.Products)
                .Must(p => p != null && p.Count > 0).WithMessage("At least one product item must be included in the voucher.");

            RuleForEach(x => x.Products).ChildRules(product =>
            {
                // RequestVoucherID is an IDENTITY column assigned by the repository
                // (SCOPE_IDENTITY on the header insert), never read from the product
                // rows — requiring it here rejects every create, since the ID does not
                // exist yet when the client submits the payload.
                product.RuleFor(p => p.ProductID)
                    .GreaterThan(0).WithMessage("ProductID must be greater than zero.");

                product.RuleFor(p => p.RequestedQuantity)
                    .GreaterThan(0).When(p => p.RequestedQuantity.HasValue)
                    .WithMessage("Quantity must be greater than zero.");
            });
        }
    }
}