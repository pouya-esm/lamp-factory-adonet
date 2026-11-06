using LampFactory.Application.Common.Results;
using LampFactory.Application.Features.Vouchers.ChargeVouchers;
using LampFactory.API.Authorization;
using LampFactory.API.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using LampFactory.API.Common;

namespace LampFactory.API.Controllers.Vouchers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ChargeVoucherController : ControllerBase
    {
        private readonly IChargeVoucherService _chargeVoucherService;

        public ChargeVoucherController(IChargeVoucherService chargeVoucherService)
        {
            _chargeVoucherService = chargeVoucherService;
        }

        [HttpGet]
        [HasPermission("ChargeVoucher", "Read")]
        public async Task<ActionResult<APIResponse<List<object>>>> GetAll()
        {
            var result = await _chargeVoucherService.GetAllAsync();
            return result.ToActionResult();
        }

        [HttpGet("{id:int}")]
        [HasPermission("ChargeVoucher", "Read")]
        public async Task<ActionResult<APIResponse<object>>> GetById(int id)
        {
            var result = await _chargeVoucherService.GetByIdAsync(id);
            return result.ToActionResult();
        }

        [HttpPost]
        [HasPermission("ChargeVoucher", "Create")]
        public async Task<ActionResult<APIResponse<object>>> Create([FromBody] ChargeVoucherProductDTO dto)
        {
            var result = await _chargeVoucherService.CreateAsync(dto);
            return result.ToCreatedAtActionResult(this);
        }

        [HttpPut("{id:int}")]
        [HasPermission("ChargeVoucher", "Update")]
        public async Task<ActionResult<APIResponse<ChargeVoucherProductDTO>>> Update(int id, [FromBody] ChargeVoucherProductDTO dto)
        {
            var result = await _chargeVoucherService.UpdateAsync(id, dto);
            return result.ToActionResult();
        }

        [HttpDelete("{id:int}")]
        [HasPermission("ChargeVoucher", "Delete")]
        public async Task<ActionResult<APIResponse>> Delete(int id)
        {
            var result = await _chargeVoucherService.DeleteAsync(id);
            return result.ToNoContentActionResult();
        }

    }
}
