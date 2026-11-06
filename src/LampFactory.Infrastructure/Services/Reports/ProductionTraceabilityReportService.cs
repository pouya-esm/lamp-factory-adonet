using LampFactory.Application.Common.Results;
using LampFactory.Application.Features.Reports;
using LampFactory.Application.Abstractions.Persistence.Reports;

namespace LampFactory.Infrastructure.Services.Reports
{
    public class ProductionTraceabilityReportService : IProductionTraceabilityReportService
    {
        private readonly IProductionTraceabilityReportQuery _report;

        public ProductionTraceabilityReportService(IProductionTraceabilityReportQuery report)
        {
            _report = report;
        }

        public async Task<Result<List<ProductionTraceabilityReportDTO>>> GetReportAsync(DateTime? startDate, string? standardNumber)
        {

            var data = await _report.GetProductionTraceabilityAsync(startDate, standardNumber);
            return Result<List<ProductionTraceabilityReportDTO>>.Success(data);
        }
    }
}