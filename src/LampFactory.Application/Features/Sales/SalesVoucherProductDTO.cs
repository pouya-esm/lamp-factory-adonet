

using LampFactory.Application.Features.Sales;

namespace LampFactory.Application.Features.Sales
{
    public class SalesVoucherProductDTO
    {
        public SalesVoucher Voucher { get; set; } = new();
        public List<SalesVoucherProduct> Products { get; set; } = new();
    }
}
