using FluentValidation;

namespace LampFactory.Application.Features.QualityControl.Validators
{
    public class QualityControlValidator : AbstractValidator<QualityControlDTO>
    {
        public QualityControlValidator()
        {
            RuleFor(x => x)
                .NotNull().WithMessage("Request body cannot be null.");

            RuleFor(x => x.QualityControl)
                .NotNull().WithMessage("QC data is required.");

            When(x => x.QualityControl != null, () =>
            {
                RuleFor(x => x.QualityControl.QualityControlType)
                    .IsInEnum().WithMessage("Invalid QualityControlType value.");
            });
        }
    }
}
