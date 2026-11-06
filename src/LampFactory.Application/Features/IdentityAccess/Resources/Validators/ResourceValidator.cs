using FluentValidation;

namespace LampFactory.Application.Features.IdentityAccess.Resources.Validators
{
    public class ResourceValidator : AbstractValidator<ResourceDetailsDTO>
    {
        public ResourceValidator()
        {
            RuleFor(x => x)
                .NotNull().WithMessage("Request body cannot be null.");

            RuleFor(x => x.Resource)
                .NotNull().WithMessage("Resource data is required.");

            When(x => x.Resource != null, () =>
            {
                RuleFor(x => x.Resource.Name)
                    .NotEmpty().WithMessage("Resource name is required.")
                    .MaximumLength(50).WithMessage("Resource name cannot exceed 50 characters.");
            });
        }
    }
}
