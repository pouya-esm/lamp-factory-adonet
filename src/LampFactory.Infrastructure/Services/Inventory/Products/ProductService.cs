using FluentValidation;
using LampFactory.Application.Common.Results;
using LampFactory.Application.Features.Inventory.Products;
using LampFactory.Application.Abstractions.Persistence.Inventory.Products;
using LampFactory.Domain.Inventory;

namespace LampFactory.Infrastructure.Services.Inventory.Products
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IValidator<ProductDTO> _validator;

        public ProductService(IProductRepository productRepository, IValidator<ProductDTO> validator)
        {
            _productRepository = productRepository;
            _validator = validator;
        }

        public async Task<Result<List<Product>>> GetAllProductsAsync()
        {

            var products = await _productRepository.GetAllAsync();
            return Result<List<Product>>.Success(products);
        }

        public async Task<Result<Product>> GetProductByIdAsync(int id)
        {

            if (id <= 0)
            {
                return Result<Product>.BadRequest("Invalid Product ID.");
            }

            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return Result<Product>.NotFound("Product not found.");
            }

            return Result<Product>.Success(product);
        }

        public async Task<Result<Product>> CreateProductAsync(ProductDTO dto)
        {

            var validationResult = await _validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                return Result<Product>.BadRequest("Validation failed", validationResult.Errors.Select(e => e.ErrorMessage).ToList());
            }

            var newProductId = await _productRepository.CreateAsync(dto.Product);
            if (newProductId <= 0)
            {
                return Result<Product>.BadRequest("Failed to create product.");
            }

            var created = await _productRepository.GetByIdAsync(newProductId);
            if (created == null)
            {
                return Result<Product>.NotFound("Product could not be loaded after creation.");
            }

            return Result<Product>.Success(created);
        }

        public async Task<Result<Product>> UpdateProductAsync(int id, ProductDTO dto)
        {

            if (dto == null || dto.Product == null || id != dto.Product.ProductID)
            {
                return Result<Product>.BadRequest("Product ID mismatch or invalid data.");
            }

            var existingProduct = await _productRepository.GetByIdAsync(id);
            if (existingProduct == null)
            {
                return Result<Product>.NotFound("Product not found for update.");
            }

            var success = await _productRepository.UpdateAsync(dto.Product);
            if (!success)
            {
                return Result<Product>.BadRequest("Failed to update product.");
            }

            var updated = await _productRepository.GetByIdAsync(id);
            if (updated == null)
            {
                return Result<Product>.NotFound("Product could not be loaded after update.");
            }

            return Result<Product>.Success(updated);
        }

        public async Task<Result> DeleteProductAsync(int id)
        {

            var existingProduct = await _productRepository.GetByIdAsync(id);
            if (existingProduct == null)
            {
                return Result.NotFound("Product not found for deletion.");
            }

            var success = await _productRepository.RemoveAsync(id);
            if (!success)
            {
                return Result.BadRequest("Failed to delete product.");
            }

            return Result.Success();
        }
    }
}