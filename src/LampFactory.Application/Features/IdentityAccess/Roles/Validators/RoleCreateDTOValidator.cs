using FluentValidation;

using LampFactory.Application.Features.IdentityAccess.Roles;

namespace LampFactory.Application.Features.IdentityAccess.Roles.Validators
{
    public class RoleCreateDTOValidator : AbstractValidator<RoleCreateDTO>
    {
        public RoleCreateDTOValidator()
        {
            RuleFor(x => x)
                .NotNull().WithMessage("Request body cannot be null.");

            RuleFor(x => x.RoleName)
                .NotEmpty().WithMessage("نام نقش الزامی است.")
                .MaximumLength(50).WithMessage("Role name cannot exceed 50 characters.");

            RuleFor(x => x.Description)
                .MaximumLength(200).WithMessage("Description cannot exceed 200 characters.");
        }
    }
}
