

using LampFactory.Application.Features.Purchasing.RequestVouchers;

namespace LampFactory.Application.Features.Purchasing.RequestVouchers
{
    public class RequestVoucherProductDTO
    {
        public RequestVoucher Voucher { get; set; } = new RequestVoucher();
        public List<RequestVoucherProduct> Products { get; set; } = new List<RequestVoucherProduct>();
    }
}
