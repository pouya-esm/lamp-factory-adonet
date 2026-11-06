using LampFactory.Application.Common.Results;
using LampFactory.Application.Features.QualityControl;
using LampFactory.API.Authorization;
using LampFactory.API.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using QualityControlEntity = LampFactory.Domain.QualityControl.QualityControl;
using LampFactory.API.Common;

namespace LampFactory.API.Controllers.QualityControl
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class QualityControlController : ControllerBase
    {
        private readonly IQualityControlService _qcService;

        public QualityControlController(IQualityControlService qcService)
        {
            _qcService = qcService;
        }

        [HttpGet]
        [HasPermission("QualityControl", "Read")]
        public async Task<ActionResult<APIResponse<List<QualityControlEntity>>>> GetAll()
        {
            var result = await _qcService.GetAllAsync();
            return result.ToActionResult();
        }

        [HttpGet("{id:int}")]
        [HasPermission("QualityControl", "Read")]
        public async Task<ActionResult<APIResponse<QualityControlEntity>>> GetById(int id)
        {
            var result = await _qcService.GetByIdAsync(id);
            return result.ToActionResult();
        }

        [HttpPost]
        [HasPermission("QualityControl", "Create")]
        public async Task<ActionResult<APIResponse<QualityControlEntity>>> Create([FromBody] QualityControlDTO dto)
        {
            var result = await _qcService.CreateAsync(dto);
            return result.ToCreatedAtActionResult(this, nameof(GetById), v => new { id = v!.QualityControlID });
        }

        [HttpPut("{id:int}")]
        [HasPermission("QualityControl", "Update")]
        public async Task<ActionResult<APIResponse<QualityControlEntity>>> Update(int id, [FromBody] QualityControlDTO dto)
        {
            var result = await _qcService.UpdateAsync(id, dto);
            return result.ToActionResult();
        }

        [HttpDelete("{id:int}")]
        [HasPermission("QualityControl", "Delete")]
        public async Task<ActionResult<APIResponse>> Delete(int id)
        {
            var result = await _qcService.DeleteAsync(id);
            return result.ToNoContentActionResult();
        }

    }
}
