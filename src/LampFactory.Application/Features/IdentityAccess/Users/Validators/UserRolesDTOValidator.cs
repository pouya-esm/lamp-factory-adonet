using FluentValidation;

using LampFactory.Application.Features.IdentityAccess.Users;

namespace LampFactory.Application.Features.IdentityAccess.Users.Validators
{
    public class UserRolesDTOValidator : AbstractValidator<UserRolesDTO>
    {
        public UserRolesDTOValidator()
        {
            RuleFor(x => x)
                .NotNull().WithMessage("Request body cannot be null.");

            RuleFor(x => x.User)
                .NotNull().WithMessage("User data is required.");

            When(x => x.User != null, () =>
            {
                RuleFor(x => x.User.Username)
                    .NotEmpty().WithMessage("Username is required.")
                    .MaximumLength(50).WithMessage("Username cannot exceed 50 characters.");

                RuleFor(x => x.User.Userpass)
                    .NotEmpty().WithMessage("Password is required.")
                    .MaximumLength(100).WithMessage("Password cannot exceed 100 characters.");

                RuleFor(x => x.User.Fullname)
                    .MaximumLength(100).WithMessage("Fullname cannot exceed 100 characters.");

                RuleFor(x => x.User.Phone)
                    .MaximumLength(20).WithMessage("Phone number cannot exceed 20 characters.");
            });

            // Validating the ID passed via the route (URL) and matching it with the UserID.
            RuleFor(x => x).Custom((dto, context) =>
            {
                if (context.RootContextData.TryGetValue("RouteId", out var routeIdObj) && routeIdObj is int routeId)
                {
                    if (routeId <= 0)
                    {
                        context.AddFailure("Invalid ID.");
                    }

                    if (dto?.User != null && dto.User.UserID != routeId)
                    {
                        context.AddFailure("ID mismatch.");
                    }
                }
            });
        }
    }
}