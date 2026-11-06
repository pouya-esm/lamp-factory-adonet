

using LampFactory.Application.Features.Vouchers.ChargeVouchers;

namespace LampFactory.Application.Features.Vouchers.ChargeVouchers
{
    public class ChargeVoucherProductDTO
    {
        public ChargeVoucher Voucher { get; set; } = new();
        public List<ChargeVoucherProduct> Products { get; set; } = new();
    }
}
