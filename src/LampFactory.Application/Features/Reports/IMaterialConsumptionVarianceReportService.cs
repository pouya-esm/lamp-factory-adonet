using LampFactory.Application.Common.Results;
using LampFactory.Application.Features.Reports;

namespace LampFactory.Application.Features.Reports;

public interface IMaterialConsumptionVarianceReportService
{
    Task<Result<List<MaterialConsumptionVarianceDTO>>> GetReportAsync(DateTime? startDate, string? rawMaterialStandardNumber);
}