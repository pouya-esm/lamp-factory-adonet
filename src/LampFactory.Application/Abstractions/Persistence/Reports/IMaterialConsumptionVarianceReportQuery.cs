using LampFactory.Application.Features.Reports;

namespace LampFactory.Application.Abstractions.Persistence.Reports;

// Read-side query contract for the material consumption variance report. The
// SQL implementation lives in Infrastructure; the service depends only on this
// interface so report logic stays testable without a database.
public interface IMaterialConsumptionVarianceReportQuery
{
    Task<List<MaterialConsumptionVarianceDTO>> GetMaterialConsumptionVarianceAsync(DateTime? startDate, string? rawMaterialStandardNumber);
}
