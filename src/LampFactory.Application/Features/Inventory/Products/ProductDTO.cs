using LampFactory.Application.Features.Inventory.Products;

namespace LampFactory.Application.Features.Inventory.Products
{
    // Write-side contract for the Product aggregate. The API binds this, not the
    // Domain entity, so the presentation layer never shapes aggregate state.
    public class ProductDTO
    {
        public Product Product { get; set; } = new();
    }
}
