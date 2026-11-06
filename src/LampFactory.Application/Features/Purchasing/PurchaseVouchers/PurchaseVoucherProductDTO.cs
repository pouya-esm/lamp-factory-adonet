

using LampFactory.Application.Features.Purchasing.PurchaseVouchers;

namespace LampFactory.Application.Features.Purchasing.PurchaseVouchers
{
    public class PurchaseVoucherProductDTO
    {
        public PurchaseVoucher Voucher { get; set; } = new();
        public List<PurchaseVoucherProduct> Products { get; set; } = new();
    }
}
