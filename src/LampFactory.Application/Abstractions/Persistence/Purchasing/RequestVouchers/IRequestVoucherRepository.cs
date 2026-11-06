

namespace LampFactory.Application.Abstractions.Persistence.Purchasing.RequestVouchers;

public interface IRequestVoucherRepository
{
    Task<int> CreateWithProductsAsync(RequestVoucher voucher, List<RequestVoucherProduct> products);
    Task<(RequestVoucher? Voucher, List<RequestVoucherProduct> Products)> GetByIdWithProductsAsync(int voucherId);
    Task<List<(RequestVoucher Voucher, List<RequestVoucherProduct> Products)>> GetAllWithProductsAsync();
    Task<bool> RemoveWithProductsAsync(int voucherId);
    Task<bool> UpdateWithProductsAsync(RequestVoucher voucher, List<RequestVoucherProduct> products);
}
