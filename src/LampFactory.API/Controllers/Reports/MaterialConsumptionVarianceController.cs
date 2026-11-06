using LampFactory.Application.Common.Results;
using LampFactory.Application.Features.Reports;
using LampFactory.API.Authorization;
using LampFactory.API.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using LampFactory.API.Common;

namespace LampFactory.API.Controllers.Reports
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class MaterialConsumptionVarianceController : ControllerBase
    {
        private readonly IMaterialConsumptionVarianceReportService _materialConsumptionVarianceReportService;

        public MaterialConsumptionVarianceController(IMaterialConsumptionVarianceReportService materialConsumptionVarianceReportService)
        {
            _materialConsumptionVarianceReportService = materialConsumptionVarianceReportService;
        }

        [HttpGet]
        [HasPermission("MaterialConsumptionVariance", "Read")]
        public async Task<ActionResult<APIResponse<List<MaterialConsumptionVarianceDTO>>>> GetAll([FromQuery] DateTime? startDate, [FromQuery] string? rawMaterialStandardNumber)
        {
            var result = await _materialConsumptionVarianceReportService.GetReportAsync(startDate, rawMaterialStandardNumber);
            return result.ToActionResult();
        }

    }
}
