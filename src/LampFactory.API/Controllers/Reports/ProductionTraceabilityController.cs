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
    public class ProductionTraceabilityController : ControllerBase
    {
        private readonly IProductionTraceabilityReportService _productionTraceabilityReportService;

        public ProductionTraceabilityController(IProductionTraceabilityReportService productionTraceabilityReportService)
        {
            _productionTraceabilityReportService = productionTraceabilityReportService;
        }

        [HttpGet]
        [HasPermission("ProductionTraceability", "Read")]
        public async Task<ActionResult<APIResponse<List<ProductionTraceabilityReportDTO>>>> GetAll([FromQuery] DateTime? startDate, [FromQuery] string? standardNumber)
        {
            var result = await _productionTraceabilityReportService.GetReportAsync(startDate, standardNumber);
            return result.ToActionResult();
        }

    }
}
