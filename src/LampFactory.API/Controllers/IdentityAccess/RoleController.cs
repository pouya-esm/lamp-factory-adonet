using LampFactory.Application.Common.Results;
using LampFactory.Application.Features.IdentityAccess.Roles;
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
    public class RoleController : ControllerBase
    {
        private readonly IRoleService _roleService;

        public RoleController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        [HttpGet]
        [HasPermission("Role", "Read")]
        public async Task<ActionResult<APIResponse<List<RoleDetailDTO>>>> GetAll()
        {
            var result = await _roleService.GetAllRolesAsync();
            return result.ToActionResult();
        }

        [HttpPost]
        [HasPermission("Role", "Create")]
        public async Task<ActionResult<APIResponse<RoleDetailDTO>>> Create([FromBody] RoleCreateDTO dto)
        {
            var result = await _roleService.CreateRoleAsync(dto.RoleName, dto.Description, dto.PermissionIds);
            return result.ToCreatedAtActionResult(this);
        }

        [HttpPut("{id:int}")]
        [HasPermission("Role", "Update")]
        public async Task<ActionResult<APIResponse<string>>> Update(int id, [FromBody] RoleUpdateDTO dto)
        {
            var result = await _roleService.UpdateRoleAsync(id, dto.RoleName, dto.Description, dto.PermissionIds);
            return result.ToActionResult();
        }

        [HttpDelete("{id:int}")]
        [HasPermission("Role", "Delete")]
        public async Task<ActionResult<APIResponse>> Delete(int id)
        {
            var result = await _roleService.DeleteRoleAsync(id);
            return result.ToNoContentActionResult();
        }

    }
}
