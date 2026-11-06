using LampFactory.Application.Common.Results;
using LampFactory.Application.Features.Purchasing.PurchaseVouchers;
using LampFactory.API.Authorization;
using LampFactory.API.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using LampFactory.API.Common;

namespace LampFactory.API.Controllers.Purchasing
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PurchaseVoucherController : ControllerBase
    {
        private readonly IPurchaseVoucherService _purchaseVoucherService;

        public PurchaseVoucherController(IPurchaseVoucherService purchaseVoucherService)
        {
            _purchaseVoucherService = purchaseVoucherService;
        }

        [HttpGet]
        [HasPermission("PurchaseVoucher", "Read")]
        public async Task<ActionResult<APIResponse<List<object>>>> GetAll()
        {
            var result = await _purchaseVoucherService.GetAllAsync();
            return result.ToActionResult();
        }

        [HttpGet("{id:int}")]
        [HasPermission("PurchaseVoucher", "Read")]
        public async Task<ActionResult<APIResponse<object>>> GetById(int id)
        {
            var result = await _purchaseVoucherService.GetByIdAsync(id);
            return result.ToActionResult();
        }

        [HttpPost]
        [HasPermission("PurchaseVoucher", "Create")]
        public async Task<ActionResult<APIResponse<object>>> Create([FromBody] PurchaseVoucherProductDTO dto)
        {
            var result = await _purchaseVoucherService.CreateAsync(dto);
            return result.ToCreatedAtActionResult(this);
        }

        [HttpPut("{id:int}")]
        [HasPermission("PurchaseVoucher", "Update")]
        public async Task<ActionResult<APIResponse<PurchaseVoucherProductDTO>>> Update(int id, [FromBody] PurchaseVoucherProductDTO dto)
        {
            var result = await _purchaseVoucherService.UpdateAsync(id, dto);
            return result.ToActionResult();
        }

        [HttpDelete("{id:int}")]
        [HasPermission("PurchaseVoucher", "Delete")]
        public async Task<ActionResult<APIResponse>> Delete(int id)
        {
            var result = await _purchaseVoucherService.DeleteAsync(id);
            return result.ToNoContentActionResult();
        }

    }
}
