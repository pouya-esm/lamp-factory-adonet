using LampFactory.Application.Common.Results;
using LampFactory.Application.Features.IdentityAccess.Resources;
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
    public class ResourceController : ControllerBase
    {
        private readonly IResourceService _resourceService;

        public ResourceController(IResourceService resourceService)
        {
            _resourceService = resourceService;
        }

        [HttpGet]
        [HasPermission("Resource", "Read")]
        public async Task<ActionResult<APIResponse<List<Resource>>>> GetAll()
        {
            var result = await _resourceService.GetAllResourcesAsync();
            return result.ToActionResult();
        }

        [HttpGet("my-accessible")]
        public async Task<ActionResult<APIResponse<List<ResourceDTO>>>> GetMyAccessible()
        {
            var result = await _resourceService.GetAccessibleResourcesAsync();
            return result.ToActionResult();
        }

        [HttpGet("{id:int}")]
        [HasPermission("Resource", "Read")]
        public async Task<ActionResult<APIResponse<Resource>>> GetById(int id)
        {
            var result = await _resourceService.GetResourceByIdAsync(id);
            return result.ToActionResult();
        }

        [HttpPost]
        [HasPermission("Resource", "Create")]
        public async Task<ActionResult<APIResponse<Resource>>> Create([FromBody] ResourceDetailsDTO dto)
        {
            var result = await _resourceService.CreateResourceAsync(dto);
            return result.ToCreatedAtActionResult(this, nameof(GetById), v => new { id = v!.ResourceID });
        }

        [HttpPut("{id:int}")]
        [HasPermission("Resource", "Update")]
        public async Task<ActionResult<APIResponse<Resource>>> Update(int id, [FromBody] ResourceDetailsDTO dto)
        {
            var result = await _resourceService.UpdateResourceAsync(id, dto);
            return result.ToActionResult();
        }

        [HttpDelete("{id:int}")]
        [HasPermission("Resource", "Delete")]
        public async Task<ActionResult<APIResponse>> Delete(int id)
        {
            var result = await _resourceService.DeleteResourceAsync(id);
            return result.ToNoContentActionResult();
        }

    }
}
