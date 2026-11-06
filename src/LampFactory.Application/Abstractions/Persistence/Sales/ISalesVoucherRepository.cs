

namespace LampFactory.Application.Abstractions.Persistence.Sales;

public interface ISalesVoucherRepository
{
    Task<int> CreateWithProductsAsync(SalesVoucher voucher, List<SalesVoucherProduct> products);
    Task<(SalesVoucher? Voucher, List<SalesVoucherProduct> Products)> GetByIdWithProductsAsync(int voucherId);
    Task<List<(SalesVoucher Voucher, List<SalesVoucherProduct> Products)>> GetAllWithProductsAsync();
    Task<bool> RemoveWithProductsAsync(int voucherId);
    Task<bool> UpdateWithProductsAsync(SalesVoucher voucher, List<SalesVoucherProduct> products);
}
