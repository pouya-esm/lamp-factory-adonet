using LampFactory.Application.Common.Results;
using LampFactory.Application.Features.IdentityAccess.Permissions;
using LampFactory.API.Authorization;
using LampFactory.API.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using LampFactory.API.Common;

namespace LampFactory.API.Controllers.IdentityAccess
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PermissionController : ControllerBase
    {
        private readonly IPermissionService _permissionService;

        public PermissionController(IPermissionService permissionService)
        {
            _permissionService = permissionService;
        }

        [HttpGet]
        [HasPermission("Permission", "Read")]
        public async Task<ActionResult<APIResponse<List<PermissionDTO>>>> GetAll()
        {
            var result = await _permissionService.GetAllPermissionsAsync();
            return result.ToActionResult();
        }

        [HttpGet("{id:int}")]
        [HasPermission("Permission", "Read")]
        public async Task<ActionResult<APIResponse<PermissionDTO>>> GetById(int id)
        {
            var result = await _permissionService.GetPermissionByIdAsync(id);
            return result.ToActionResult();
        }

        [HttpPost]
        [HasPermission("Permission", "Create")]
        public async Task<ActionResult<APIResponse<PermissionDTO>>> Create([FromBody] PermissionCreateDTO dto)
        {
            var result = await _permissionService.CreatePermissionAsync(dto.ResourceID, dto.ActionID);
            return result.ToCreatedAtActionResult(this, nameof(GetById), v => new { id = v!.PermissionID });
        }

        [HttpPut("{id:int}")]
        [HasPermission("Permission", "Update")]
        public async Task<ActionResult<APIResponse<string>>> Update(int id, [FromBody] PermissionCreateDTO dto)
        {
            var result = await _permissionService.UpdatePermissionAsync(id, dto.ResourceID, dto.ActionID);
            return result.ToActionResult();
        }

        [HttpDelete("{id:int}")]
        [HasPermission("Permission", "Delete")]
        public async Task<ActionResult<APIResponse>> Delete(int id)
        {
            var result = await _permissionService.DeletePermissionAsync(id);
            return result.ToNoContentActionResult();
        }

    }
}
