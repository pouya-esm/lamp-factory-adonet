using FluentValidation;

namespace LampFactory.Application.Features.Production.LineStops.Validators
{
    public class LineStopValidator : AbstractValidator<LineStopDTO>
    {
        public LineStopValidator()
        {
            RuleFor(x => x)
                .NotNull().WithMessage("Request body cannot be null.");

            RuleFor(x => x.LineStop)
                .NotNull().WithMessage("LineStop data is required.");
        }
    }
}
