using QualityControlEntity = LampFactory.Domain.QualityControl.QualityControl;

namespace LampFactory.Application.Features.QualityControl
{
    // Write-side contract for the QualityControl aggregate. The API binds this, not
    // the Domain entity, so the presentation layer never shapes aggregate state.
    // The aggregate shares its namespace name, so it is imported under an alias.
    public class QualityControlDTO
    {
        public QualityControlEntity QualityControl { get; set; } = new();
    }
}
