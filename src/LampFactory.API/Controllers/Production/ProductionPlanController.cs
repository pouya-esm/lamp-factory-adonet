using LampFactory.Application.Common.Results;
using LampFactory.Application.Features.Production.ProductionPlans;
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
    public class ProductionPlanController : ControllerBase
    {
        private readonly IProductionPlanService _productionPlanService;

        public ProductionPlanController(IProductionPlanService productionPlanService)
        {
            _productionPlanService = productionPlanService;
        }

        [HttpGet]
        [HasPermission("ProductionPlan", "Read")]
        public async Task<ActionResult<APIResponse<List<object>>>> GetAll()
        {
            var result = await _productionPlanService.GetAllProductionPlansAsync();
            return result.ToActionResult();
        }

        [HttpGet("{id:int}")]
        [HasPermission("ProductionPlan", "Read")]
        public async Task<ActionResult<APIResponse<object>>> GetById(int id)
        {
            var result = await _productionPlanService.GetProductionPlanByIdAsync(id);
            return result.ToActionResult();
        }

        [HttpPost]
        [HasPermission("ProductionPlan", "Create")]
        public async Task<ActionResult<APIResponse<object>>> Create([FromBody] ProductionPlanDTO dto)
        {
            var result = await _productionPlanService.CreateProductionPlanAsync(dto);
            return result.ToCreatedAtActionResult(this);
        }

        [HttpPut("{id:int}")]
        [HasPermission("ProductionPlan", "Update")]
        public async Task<ActionResult<APIResponse<ProductionPlanDTO>>> Update(int id, [FromBody] ProductionPlanDTO dto)
        {
            var result = await _productionPlanService.UpdateProductionPlanAsync(id, dto);
            return result.ToActionResult();
        }

        [HttpDelete("{id:int}")]
        [HasPermission("ProductionPlan", "Delete")]
        public async Task<ActionResult<APIResponse>> Delete(int id)
        {
            var result = await _productionPlanService.DeleteProductionPlanAsync(id);
            return result.ToNoContentActionResult();
        }

    }
}
