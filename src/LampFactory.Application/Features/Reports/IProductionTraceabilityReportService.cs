using LampFactory.Application.Common.Results;
using LampFactory.Application.Features.Reports;

namespace LampFactory.Application.Features.Reports;

public interface IProductionTraceabilityReportService
{
    Task<Result<List<ProductionTraceabilityReportDTO>>> GetReportAsync(DateTime? startDate, string? standardNumber);
}