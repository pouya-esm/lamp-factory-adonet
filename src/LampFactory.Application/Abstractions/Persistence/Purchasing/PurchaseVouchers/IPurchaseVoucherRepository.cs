

namespace LampFactory.Application.Abstractions.Persistence.Purchasing.PurchaseVouchers;

public interface IPurchaseVoucherRepository
{
    Task<int> CreateWithProductsAsync(PurchaseVoucher voucher, List<PurchaseVoucherProduct> products);
    Task<(PurchaseVoucher? Voucher, List<PurchaseVoucherProduct> Products)> GetByIdWithProductsAsync(int voucherId);
    Task<List<(PurchaseVoucher Voucher, List<PurchaseVoucherProduct> Products)>> GetAllWithProductsAsync();
    Task<bool> RemoveWithProductsAsync(int voucherId);
    Task<bool> UpdateWithProductsAsync(PurchaseVoucher voucher, List<PurchaseVoucherProduct> products);
}
