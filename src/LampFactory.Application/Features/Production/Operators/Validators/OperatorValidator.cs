using FluentValidation;

namespace LampFactory.Application.Features.Production.Operators.Validators
{
    public class OperatorValidator : AbstractValidator<OperatorDTO>
    {
        public OperatorValidator()
        {
            RuleFor(x => x)
                .NotNull().WithMessage("Request body cannot be null.");

            RuleFor(x => x.Operator)
                .NotNull().WithMessage("Operator data is required.");

            When(x => x.Operator != null, () =>
            {
                RuleFor(x => x.Operator.NationalID)
                    .NotEmpty().WithMessage("NationalID is required.")
                    .MaximumLength(20).WithMessage("NationalID cannot exceed 20 characters.");

                RuleFor(x => x.Operator.FirstName)
                    .MaximumLength(50).WithMessage("First name cannot exceed 50 characters.");

                RuleFor(x => x.Operator.LastName)
                    .MaximumLength(50).WithMessage("Last name cannot exceed 50 characters.");
            });
        }
    }
}
