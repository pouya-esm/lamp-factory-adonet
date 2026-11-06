using LampFactory.Application.Common.Results;
using LampFactory.Application.Features.Sales;
using LampFactory.API.Authorization;
using LampFactory.API.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using LampFactory.API.Common;

namespace LampFactory.API.Controllers.Sales
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class SalesVoucherController : ControllerBase
    {
        private readonly ISalesVoucherService _salesVoucherService;

        public SalesVoucherController(ISalesVoucherService salesVoucherService)
        {
            _salesVoucherService = salesVoucherService;
        }

        [HttpGet]
        [HasPermission("SalesVoucher", "Read")]
        public async Task<ActionResult<APIResponse<List<object>>>> GetAll()
        {
            var result = await _salesVoucherService.GetAllAsync();
            return result.ToActionResult();
        }

        [HttpGet("{id:int}")]
        [HasPermission("SalesVoucher", "Read")]
        public async Task<ActionResult<APIResponse<object>>> GetById(int id)
        {
            var result = await _salesVoucherService.GetByIdAsync(id);
            return result.ToActionResult();
        }

        [HttpPost]
        [HasPermission("SalesVoucher", "Create")]
        public async Task<ActionResult<APIResponse<object>>> Create([FromBody] SalesVoucherProductDTO dto)
        {
            var result = await _salesVoucherService.CreateAsync(dto);
            return result.ToCreatedAtActionResult(this);
        }

        [HttpPut("{id:int}")]
        [HasPermission("SalesVoucher", "Update")]
        public async Task<ActionResult<APIResponse<SalesVoucherProductDTO>>> Update(int id, [FromBody] SalesVoucherProductDTO dto)
        {
            var result = await _salesVoucherService.UpdateAsync(id, dto);
            return result.ToActionResult();
        }

        [HttpDelete("{id:int}")]
        [HasPermission("SalesVoucher", "Delete")]
        public async Task<ActionResult<APIResponse>> Delete(int id)
        {
            var result = await _salesVoucherService.DeleteAsync(id);
            return result.ToNoContentActionResult();
        }

    }
}
