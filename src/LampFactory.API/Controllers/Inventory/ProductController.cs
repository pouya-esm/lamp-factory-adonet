using LampFactory.Application.Common.Results;
using LampFactory.Application.Features.Inventory.Products;
using LampFactory.API.Authorization;
using LampFactory.API.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using LampFactory.Domain.Inventory;
using LampFactory.API.Common;

namespace LampFactory.API.Controllers.Inventory
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        [HasPermission("Product", "Read")]
        public async Task<ActionResult<APIResponse<List<Product>>>> GetAll()
        {
            var result = await _productService.GetAllProductsAsync();
            return result.ToActionResult();
        }

        [HttpGet("{id:int}")]
        [HasPermission("Product", "Read")]
        public async Task<ActionResult<APIResponse<Product>>> GetById(int id)
        {
            var result = await _productService.GetProductByIdAsync(id);
            return result.ToActionResult();
        }

        [HttpPost]
        [HasPermission("Product", "Create")]
        public async Task<ActionResult<APIResponse<Product>>> Create([FromBody] ProductDTO dto)
        {
            var result = await _productService.CreateProductAsync(dto);
            return result.ToCreatedAtActionResult(this, nameof(GetById), v => new { id = v!.ProductID });
        }

        [HttpPut("{id:int}")]
        [HasPermission("Product", "Update")]
        public async Task<ActionResult<APIResponse<Product>>> Update(int id, [FromBody] ProductDTO dto)
        {
            var result = await _productService.UpdateProductAsync(id, dto);
            return result.ToActionResult();
        }

        [HttpDelete("{id:int}")]
        [HasPermission("Product", "Delete")]
        public async Task<ActionResult<APIResponse>> Delete(int id)
        {
            var result = await _productService.DeleteProductAsync(id);
            return result.ToNoContentActionResult();
        }

    }
}
