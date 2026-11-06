using LampFactory.Application.Common.Results;
using LampFactory.Application.Features.Purchasing.RequestVouchers;
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
    public class RequestVoucherController : ControllerBase
    {
        private readonly IRequestVoucherService _requestVoucherService;

        public RequestVoucherController(IRequestVoucherService requestVoucherService)
        {
            _requestVoucherService = requestVoucherService;
        }

        [HttpGet]
        [HasPermission("RequestVoucher", "Read")]
        public async Task<ActionResult<APIResponse<List<object>>>> GetAll()
        {
            var result = await _requestVoucherService.GetAllRequestVouchersAsync();
            return result.ToActionResult();
        }

        [HttpGet("{id:int}")]
        [HasPermission("RequestVoucher", "Read")]
        public async Task<ActionResult<APIResponse<object>>> GetById(int id)
        {
            var result = await _requestVoucherService.GetRequestVoucherByIdAsync(id);
            return result.ToActionResult();
        }

        [HttpPost]
        [HasPermission("RequestVoucher", "Create")]
        public async Task<ActionResult<APIResponse<object>>> Create([FromBody] RequestVoucherProductDTO dto)
        {
            var result = await _requestVoucherService.CreateRequestVoucherAsync(dto);
            return result.ToCreatedAtActionResult(this);
        }

        [HttpPut("{id:int}")]
        [HasPermission("RequestVoucher", "Update")]
        public async Task<ActionResult<APIResponse<RequestVoucherProductDTO>>> Update(int id, [FromBody] RequestVoucherProductDTO dto)
        {
            var result = await _requestVoucherService.UpdateRequestVoucherAsync(id, dto);
            return result.ToActionResult();
        }

        [HttpDelete("{id:int}")]
        [HasPermission("RequestVoucher", "Delete")]
        public async Task<ActionResult<APIResponse>> Delete(int id)
        {
            var result = await _requestVoucherService.DeleteRequestVoucherAsync(id);
            return result.ToNoContentActionResult();
        }

    }
}
