using LampFactory.Application.Common.Results;
using LampFactory.Application.Features.Production.Operators;
using LampFactory.API.Authorization;
using LampFactory.API.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using LampFactory.Domain.Operators;
using LampFactory.API.Common;

namespace LampFactory.API.Controllers.Production
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class OperatorController : ControllerBase
    {
        private readonly IOperatorService _operatorService;

        public OperatorController(IOperatorService operatorService)
        {
            _operatorService = operatorService;
        }

        [HttpGet]
        [HasPermission("Operator", "Read")]
        public async Task<ActionResult<APIResponse<List<Operator>>>> GetAll()
        {
            var result = await _operatorService.GetAllOperatorsAsync();
            return result.ToActionResult();
        }

        [HttpGet("{id:int}")]
        [HasPermission("Operator", "Read")]
        public async Task<ActionResult<APIResponse<Operator>>> GetById(int id)
        {
            var result = await _operatorService.GetOperatorByIdAsync(id);
            return result.ToActionResult();
        }

        [HttpPost]
        [HasPermission("Operator", "Create")]
        public async Task<ActionResult<APIResponse<Operator>>> Create([FromBody] OperatorDTO dto)
        {
            var result = await _operatorService.CreateOperatorAsync(dto);
            return result.ToCreatedAtActionResult(this, nameof(GetById), v => new { id = v!.OperatorID });
        }

        [HttpPut("{id:int}")]
        [HasPermission("Operator", "Update")]
        public async Task<ActionResult<APIResponse<Operator>>> Update(int id, [FromBody] OperatorDTO dto)
        {
            var result = await _operatorService.UpdateOperatorAsync(id, dto);
            return result.ToActionResult();
        }

        [HttpDelete("{id:int}")]
        [HasPermission("Operator", "Delete")]
        public async Task<ActionResult<APIResponse>> Delete(int id)
        {
            var result = await _operatorService.DeleteOperatorAsync(id);
            return result.ToNoContentActionResult();
        }

    }
}
