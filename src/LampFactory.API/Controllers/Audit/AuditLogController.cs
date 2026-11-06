using LampFactory.Application.Common.Results;
using LampFactory.Application.Features.Audit;
using LampFactory.API.Authorization;
using LampFactory.API.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using LampFactory.API.Common;

namespace LampFactory.API.Controllers.Audit
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AuditLogController : ControllerBase
    {
        private readonly IAuditLogService _auditLogService;

        public AuditLogController(IAuditLogService auditLogService)
        {
            _auditLogService = auditLogService;
        }

        [HttpGet]
        [HasPermission("AuditLog", "Read")]
        public async Task<ActionResult<APIResponse<object>>> GetAll(
            [FromQuery] string? entityName,
            [FromQuery] int? page,
            [FromQuery] int? pageSize)
        {
            var result = await _auditLogService.GetAllAsync(entityName, page, pageSize);
            return result.ToActionResult();
        }

        [HttpGet("entities")]
        [HasPermission("AuditLog", "Read")]
        public async Task<ActionResult<APIResponse<List<string>>>> GetEntityNames()
        {
            var result = await _auditLogService.GetEntityNamesAsync();
            return result.ToActionResult();
        }

    }
}
