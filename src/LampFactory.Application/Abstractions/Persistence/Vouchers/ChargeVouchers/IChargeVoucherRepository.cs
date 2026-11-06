

namespace LampFactory.Application.Abstractions.Persistence.Vouchers.ChargeVouchers;

public interface IChargeVoucherRepository
{
    Task<int> CreateWithProductsAsync(ChargeVoucher voucher, List<ChargeVoucherProduct> products);
    Task<(ChargeVoucher? Voucher, List<ChargeVoucherProduct> Products)> GetByIdWithProductsAsync(int voucherId);
    Task<List<(ChargeVoucher Voucher, List<ChargeVoucherProduct> Products)>> GetAllWithProductsAsync();
    Task<bool> RemoveWithProductsAsync(int voucherId);
    Task<bool> UpdateWithProductsAsync(ChargeVoucher voucher, List<ChargeVoucherProduct> products);
}
