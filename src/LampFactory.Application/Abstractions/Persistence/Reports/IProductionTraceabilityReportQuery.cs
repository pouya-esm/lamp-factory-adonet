using LampFactory.Application.Features.Reports;

namespace LampFactory.Application.Abstractions.Persistence.Reports;

// Read-side query contract for the production traceability report. The SQL
// implementation lives in Infrastructure; the service depends only on this
// interface so report logic stays testable without a database.
public interface IProductionTraceabilityReportQuery
{
    Task<List<ProductionTraceabilityReportDTO>> GetProductionTraceabilityAsync(DateTime? startDate, string? standardNumber);
}
