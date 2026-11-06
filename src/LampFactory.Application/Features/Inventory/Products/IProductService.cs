using LampFactory.Application.Common.Results;
using LampFactory.Application.Features.Inventory.Products;
using LampFactory.Domain.Inventory;

namespace LampFactory.Application.Features.Inventory.Products;

public interface IProductService
{
    Task<Result<List<Product>>> GetAllProductsAsync();
    Task<Result<Product>> GetProductByIdAsync(int id);
    Task<Result<Product>> CreateProductAsync(ProductDTO dto);
    Task<Result<Product>> UpdateProductAsync(int id, ProductDTO dto);
    Task<Result> DeleteProductAsync(int id);
}