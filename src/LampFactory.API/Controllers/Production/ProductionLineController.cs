using LampFactory.Application.Common.Results;
using LampFactory.Application.Features.Production.ProductionLines;
using LampFactory.API.Authorization;
using LampFactory.API.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using LampFactory.Domain.Production;
using LampFactory.API.Common;

namespace LampFactory.API.Controllers.Production
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ProductionLineController : ControllerBase
    {
        private readonly IProductionLineService _productionLineService;

        public ProductionLineController(IProductionLineService productionLineService)
        {
            _productionLineService = productionLineService;
        }

        [HttpGet]
        [HasPermission("ProductionLine", "Read")]
        public async Task<ActionResult<APIResponse<List<ProductionLine>>>> GetAll()
        {
            var result = await _productionLineService.GetAllProductionLinesAsync();
            return result.ToActionResult();
        }

        [HttpGet("{id:int}")]
        [HasPermission("ProductionLine", "Read")]
        public async Task<ActionResult<APIResponse<ProductionLine>>> GetById(int id)
        {
            var result = await _productionLineService.GetProductionLineByIdAsync(id);
            return result.ToActionResult();
        }

        [HttpPost]
        [HasPermission("ProductionLine", "Create")]
        public async Task<ActionResult<APIResponse<ProductionLine>>> Create([FromBody] ProductionLineDTO dto)
        {
            var result = await _productionLineService.CreateProductionLineAsync(dto);
            return result.ToCreatedAtActionResult(this, nameof(GetById), v => new { id = v!.ProductionLineID });
        }

        [HttpPut("{id:int}")]
        [HasPermission("ProductionLine", "Update")]
        public async Task<ActionResult<APIResponse<ProductionLine>>> Update(int id, [FromBody] ProductionLineDTO dto)
        {
            var result = await _productionLineService.UpdateProductionLineAsync(id, dto);
            return result.ToActionResult();
        }

        [HttpDelete("{id:int}")]
        [HasPermission("ProductionLine", "Delete")]
        public async Task<ActionResult<APIResponse>> Delete(int id)
        {
            var result = await _productionLineService.DeleteProductionLineAsync(id);
            return result.ToNoContentActionResult();
        }

    }
}
