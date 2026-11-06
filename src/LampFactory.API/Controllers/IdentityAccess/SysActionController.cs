using LampFactory.Application.Common.Results;
using LampFactory.Application.Features.IdentityAccess.SysActions;
using LampFactory.API.Authorization;
using LampFactory.API.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using LampFactory.Domain.IdentityAccess;
using LampFactory.API.Common;

namespace LampFactory.API.Controllers.IdentityAccess
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SysActionController : ControllerBase
    {
        private readonly ISysActionService _actionService;

        public SysActionController(ISysActionService actionService)
        {
            _actionService = actionService;
        }

        [HttpGet]
        [HasPermission("SysAction", "Read")]
        public async Task<ActionResult<APIResponse<List<SysAction>>>> GetAll()
        {
            var result = await _actionService.GetAllActionsAsync();
            return result.ToActionResult();
        }

        [HttpGet("{id:int}")]
        [HasPermission("SysAction", "Read")]
        public async Task<ActionResult<APIResponse<SysAction>>> GetById(int id)
        {
            var result = await _actionService.GetActionByIdAsync(id);
            return result.ToActionResult();
        }

        [HttpPost]
        [HasPermission("SysAction", "Create")]
        public async Task<ActionResult<APIResponse<SysAction>>> Create([FromBody] SysActionDTO dto)
        {
            var result = await _actionService.CreateActionAsync(dto);
            return result.ToCreatedAtActionResult(this, nameof(GetById), v => new { id = v!.ActionID });
        }

        [HttpPut("{id:int}")]
        [HasPermission("SysAction", "Update")]
        public async Task<ActionResult<APIResponse<SysAction>>> Update(int id, [FromBody] SysActionDTO dto)
        {
            var result = await _actionService.UpdateActionAsync(id, dto);
            return result.ToActionResult();
        }

        [HttpDelete("{id:int}")]
        [HasPermission("SysAction", "Delete")]
        public async Task<ActionResult<APIResponse>> Delete(int id)
        {
            var result = await _actionService.DeleteActionAsync(id);
            return result.ToNoContentActionResult();
        }

    }
}
