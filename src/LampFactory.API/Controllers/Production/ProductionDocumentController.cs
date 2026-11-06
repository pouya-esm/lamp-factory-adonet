using LampFactory.Application.Common.Results;
using LampFactory.Application.Features.Production.ProductionDocuments;
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
    public class ProductionDocumentController : ControllerBase
    {
        private readonly IProductionDocumentService _documentService;

        public ProductionDocumentController(IProductionDocumentService documentService)
        {
            _documentService = documentService;
        }

        [HttpGet]
        [HasPermission("ProductionDocument", "Read")]
        public async Task<ActionResult<APIResponse<List<object>>>> GetAll()
        {
            var result = await _documentService.GetAllDocumentsAsync();
            return result.ToActionResult();
        }

        [HttpGet("{id:int}")]
        [HasPermission("ProductionDocument", "Read")]
        public async Task<ActionResult<APIResponse<object>>> GetById(int id)
        {
            var result = await _documentService.GetDocumentByIdAsync(id);
            return result.ToActionResult();
        }

        [HttpPost]
        [HasPermission("ProductionDocument", "Create")]
        public async Task<ActionResult<APIResponse<object>>> Create([FromBody] ProductionDocumentDTO dto)
        {
            var result = await _documentService.CreateDocumentAsync(dto);
            return result.ToCreatedAtActionResult(this);
        }

        [HttpPut("{id:int}")]
        [HasPermission("ProductionDocument", "Update")]
        public async Task<ActionResult<APIResponse<ProductionDocumentDTO>>> Update(int id, [FromBody] ProductionDocumentDTO dto)
        {
            var result = await _documentService.UpdateDocumentAsync(id, dto);
            return result.ToActionResult();
        }

        [HttpDelete("{id:int}")]
        [HasPermission("ProductionDocument", "Delete")]
        public async Task<ActionResult<APIResponse>> Delete(int id)
        {
            var result = await _documentService.DeleteDocumentAsync(id);
            return result.ToNoContentActionResult();
        }

    }
}
