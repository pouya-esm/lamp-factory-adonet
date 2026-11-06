using FluentValidation;

using LampFactory.Application.Features.IdentityAccess.Permissions;

namespace LampFactory.Application.Features.IdentityAccess.Permissions.Validators
{
    public class PermissionCreateDTOValidator : AbstractValidator<PermissionCreateDTO>
    {
        public PermissionCreateDTOValidator()
        {
            RuleFor(x => x)
                .NotNull().WithMessage("Request body cannot be null.");

            RuleFor(x => x.ResourceID)
                .GreaterThan(0).WithMessage("Invalid Resource ID or Action ID.");

            RuleFor(x => x.ActionID)
                .GreaterThan(0).WithMessage("Invalid Resource ID or Action ID.");
        }
    }
}