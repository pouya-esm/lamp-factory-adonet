
using LampFactory.Application.Features.Reports;

namespace LampFactory.Application.Features.Reports
{
    public class MaterialConsumptionVarianceDTO
    {
        public int ProductionDocumentID { get; set; }
        public string FinalProductName { get; set; } = string.Empty;
        public DateTime? ProductionDate { get; set; }
        public int? ProducedQuantity { get; set; }
        public string? RawMaterialStandardNumber { get; set; }
        public string? RawMaterialName { get; set; }
        public string? ConsumptionUnit { get; set; }
        public double? StandardConsumptionPerUnit { get; set; }
        public double? StandardTotalConsumption { get; set; }
        public int? ActualConsumption { get; set; }
        public double? ConsumptionVariance { get; set; }
    }
}
