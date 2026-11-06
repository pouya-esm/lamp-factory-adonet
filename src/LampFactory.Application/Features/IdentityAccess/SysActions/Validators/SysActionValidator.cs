using FluentValidation;

namespace LampFactory.Application.Features.IdentityAccess.SysActions.Validators
{
    public class SysActionValidator : AbstractValidator<SysActionDTO>
    {
        public SysActionValidator()
        {
            RuleFor(x => x)
                .NotNull().WithMessage("Request body cannot be null.");

            RuleFor(x => x.SysAction)
                .NotNull().WithMessage("Action data is required.");

            When(x => x.SysAction != null, () =>
            {
                RuleFor(x => x.SysAction.Name)
                    .NotEmpty().WithMessage("Action name is required.")
                    .MaximumLength(50).WithMessage("Action name cannot exceed 50 characters.");
            });
        }
    }
}
