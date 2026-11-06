using LampFactory.Application.Common.Results;
using LampFactory.Application.Features.Production.BillOfMaterials;
using LampFactory.API.Authorization;
using LampFactory.API.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using LampFactory.API.Common;

namespace LampFactory.API.Controllers.Production
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class BOMController : ControllerBase
    {
        private readonly IBOMService _bomService;

        public BOMController(IBOMService bomService)
        {
            _bomService = bomService;
        }

        [HttpGet]
        [HasPermission("BOM", "Read")]
        public async Task<ActionResult<APIResponse<IReadOnlyList<BOMWithComponentsDTO>>>> GetAll()
        {
            var result = await _bomService.GetAllBOMsAsync();
            return result.ToActionResult();
        }

        [HttpGet("{id:int}")]
        [HasPermission("BOM", "Read")]
        public async Task<ActionResult<APIResponse<BOMWithComponentsDTO>>> GetById(int id)
        {
            var result = await _bomService.GetBOMByIdAsync(id);
            return result.ToActionResult();
        }

        [HttpPost]
        [HasPermission("BOM", "Create")]
        public async Task<ActionResult<APIResponse<BOMWithComponentsDTO>>> Create([FromBody] BOMDTO dto)
        {
            var result = await _bomService.CreateBOMAsync(dto);
            return result.ToCreatedAtActionResult(this, nameof(GetById), v => new { id = v!.BOM!.BomId });
        }

        [HttpPut("{id:int}")]
        [HasPermission("BOM", "Update")]
        public async Task<ActionResult<APIResponse<BOMWithComponentsDTO>>> Update(int id, [FromBody] BOMDTO dto)
        {
            var result = await _bomService.UpdateBOMAsync(id, dto);
            return result.ToActionResult();
        }

        [HttpDelete("{id:int}")]
        [HasPermission("BOM", "Delete")]
        public async Task<ActionResult<APIResponse>> Delete(int id)
        {
            var result = await _bomService.DeleteBOMAsync(id);
            return result.ToNoContentActionResult();
        }

    }
}
