
using LampFactory.Application.Features.Reports;

namespace LampFactory.Application.Features.Reports
{
    public class ProductionTraceabilityReportDTO
    {
        public int ProductionDocumentID { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int LineNumber { get; set; }
        // ProductionLine.Status is a NOT NULL BIT column (false = Stopped,
        // true = Running); the client renders it as a boolean, never an int.
        public bool? LineStatus { get; set; }
        public string OperatorName { get; set; } = string.Empty;
        public string? OperatorRole { get; set; }
        public DateTime? ProductionDate { get; set; }
        public int? ProductionShift { get; set; }
        public int? ProducedQuantity { get; set; }
        public int? RejectedQuantity { get; set; }
        public int? PlanPriority { get; set; }
        public DateTime? TargetDate { get; set; }
    }
}
