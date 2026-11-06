using LampFactory.Application.Common.Results;
using LampFactory.Application.Features.Production.LineStops;
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
    public class LineStopController : ControllerBase
    {
        private readonly ILineStopService _lineStopService;

        public LineStopController(ILineStopService lineStopService)
        {
            _lineStopService = lineStopService;
        }

        [HttpGet]
        [HasPermission("LineStop", "Read")]
        public async Task<ActionResult<APIResponse<List<LineStop>>>> GetAll()
        {
            var result = await _lineStopService.GetAllAsync();
            return result.ToActionResult();
        }

        [HttpGet("{id:int}")]
        [HasPermission("LineStop", "Read")]
        public async Task<ActionResult<APIResponse<LineStop>>> GetById(int id)
        {
            var result = await _lineStopService.GetByIdAsync(id);
            return result.ToActionResult();
        }

        [HttpPost]
        [HasPermission("LineStop", "Create")]
        public async Task<ActionResult<APIResponse<LineStop>>> Create([FromBody] LineStopDTO dto)
        {
            var result = await _lineStopService.CreateAsync(dto);
            return result.ToCreatedAtActionResult(this, nameof(GetById), v => new { id = v!.LineStopID });
        }

        [HttpPut("{id:int}")]
        [HasPermission("LineStop", "Update")]
        public async Task<ActionResult<APIResponse<LineStop>>> Update(int id, [FromBody] LineStopDTO dto)
        {
            var result = await _lineStopService.UpdateAsync(id, dto);
            return result.ToActionResult();
        }

        [HttpDelete("{id:int}")]
        [HasPermission("LineStop", "Delete")]
        public async Task<ActionResult<APIResponse>> Delete(int id)
        {
            var result = await _lineStopService.DeleteAsync(id);
            return result.ToNoContentActionResult();
        }

    }
}
