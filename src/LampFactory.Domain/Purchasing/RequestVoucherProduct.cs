namespace LampFactory.Domain.Purchasing
{
    public class RequestVoucherProduct
    {
        public int? RequestedQuantity { get; set; }
        public int RequestVoucherID { get; set; }
        public RequestVoucher? RequestVoucher { get; set; }
        public int ProductID { get; set; }
        public Product? Product { get; set; }
    }
}
