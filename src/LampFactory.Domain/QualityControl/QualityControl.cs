using LampFactory.Domain.Enums;

namespace LampFactory.Domain.QualityControl
{
    public class QualityControl : AuditableEntity
    {
        public int QualityControlID { get; set; }
        public QualityControlType QualityControlType { get; set; }
        public DateTime? QualityControlDate { get; set; }
        public string? InspectionType { get; set; }
        public double? SampleRatio { get; set; }
        public string? BatchNumber { get; set; }
        // Maps to the QualityControlAcceptance BIT column: 1 = accepted (true),
        // 0 = rejected (false). Nullable so unchecked rows stay NULL.
        public bool? QualityControlAcceptance { get; set; }
        public int ProductionDocumentID { get; set; }
        public ProductionDocument? ProductionDocument { get; set; }
    }
}
