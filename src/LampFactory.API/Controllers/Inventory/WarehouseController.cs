using LampFactory.Application.Common.Results;
using LampFactory.Application.Features.Inventory.Warehouses;
using LampFactory.API.Authorization;
using LampFactory.API.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using LampFactory.API.Common;

namespace LampFactory.API.Controllers.Inventory
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class WarehouseController : ControllerBase
    {
        private readonly IWarehouseService _warehouseService;

        public WarehouseController(IWarehouseService warehouseService)
        {
            _warehouseService = warehouseService;
        }

        [HttpGet]
        [HasPermission("Warehouse", "Read")]
        public async Task<ActionResult<APIResponse<List<WarehouseDetailsDTO>>>> GetAll()
        {
            var result = await _warehouseService.GetAllWarehousesAsync();
            return result.ToActionResult();
        }

        [HttpGet("{id:int}")]
        [HasPermission("Warehouse", "Read")]
        public async Task<ActionResult<APIResponse<WarehouseDetailsDTO>>> GetById(int id)
        {
            var result = await _warehouseService.GetWarehouseByIdAsync(id);
            return result.ToActionResult();
        }

        [HttpPost]
        [HasPermission("Warehouse", "Create")]
        public async Task<ActionResult<APIResponse<WarehouseDetailsDTO>>> Create([FromBody] WarehouseDetailsDTO dto)
        {
            var result = await _warehouseService.CreateWarehouseAsync(dto);
            return result.ToCreatedAtActionResult(this, nameof(GetById), v => new { id = v!.Warehouse!.WarehouseID });
        }

        [HttpPut("{id:int}")]
        [HasPermission("Warehouse", "Update")]
        public async Task<ActionResult<APIResponse<WarehouseDetailsDTO>>> Update(int id, [FromBody] WarehouseDetailsDTO dto)
        {
            var result = await _warehouseService.UpdateWarehouseAsync(id, dto);
            return result.ToActionResult();
        }

        [HttpDelete("{id:int}")]
        [HasPermission("Warehouse", "Delete")]
        public async Task<ActionResult<APIResponse>> Delete(int id)
        {
            var result = await _warehouseService.DeleteWarehouseAsync(id);
            return result.ToNoContentActionResult();
        }

    }
}
