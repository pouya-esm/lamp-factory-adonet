using LampFactory.Application.Common.Results;
using LampFactory.Application.Features.IdentityAccess.Users;
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
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        [HasPermission("SystemUser", "Read")]
        public async Task<ActionResult<APIResponse<List<UserRolesDTO>>>> GetAll()
        {
            var result = await _userService.GetAllUsersAsync();
            return result.ToActionResult();
        }

        [HttpGet("{id:int}")]
        [HasPermission("SystemUser", "Read")]
        public async Task<ActionResult<APIResponse<UserRolesDTO>>> GetById(int id)
        {
            var result = await _userService.GetUserByIdAsync(id);
            return result.ToActionResult();
        }

        [HttpPost]
        [HasPermission("SystemUser", "Create")]
        public async Task<ActionResult<APIResponse<UserRolesDTO>>> Create([FromBody] UserRolesDTO userRolesDto)
        {
            var result = await _userService.CreateUserAsync(userRolesDto);
            return result.ToCreatedAtActionResult(this, nameof(GetById), v => new { id = v!.User!.UserID });
        }

        [HttpPut("{id:int}")]
        [HasPermission("SystemUser", "Update")]
        public async Task<ActionResult<APIResponse<UserRolesDTO>>> Update(int id, [FromBody] UserRolesDTO userRolesDto)
        {
            var result = await _userService.UpdateUserAsync(id, userRolesDto);
            return result.ToActionResult();
        }

        [HttpDelete("{id:int}")]
        [HasPermission("SystemUser", "Delete")]
        public async Task<ActionResult<APIResponse>> Delete(int id)
        {
            var result = await _userService.DeleteUserAsync(id);
            return result.ToNoContentActionResult();
        }

    }
}
