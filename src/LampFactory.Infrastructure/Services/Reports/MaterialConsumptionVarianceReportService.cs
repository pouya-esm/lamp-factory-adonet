using LampFactory.Application.Common.Results;
using LampFactory.Application.Features.Reports;
using LampFactory.Application.Abstractions.Persistence.Reports;

namespace LampFactory.Infrastructure.Services.Reports
{
    public class MaterialConsumptionVarianceReportService : IMaterialConsumptionVarianceReportService
    {
        private readonly IMaterialConsumptionVarianceReportQuery _report;

        public MaterialConsumptionVarianceReportService(IMaterialConsumptionVarianceReportQuery report)
        {
            _report = report;
        }

        public async Task<Result<List<MaterialConsumptionVarianceDTO>>> GetReportAsync(DateTime? startDate, string? rawMaterialStandardNumber)
        {

            var data = await _report.GetMaterialConsumptionVarianceAsync(startDate, rawMaterialStandardNumber);
            return Result<List<MaterialConsumptionVarianceDTO>>.Success(data);
        }
    }
}