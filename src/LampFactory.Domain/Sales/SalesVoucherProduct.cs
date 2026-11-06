namespace LampFactory.Domain.Sales
{
    public class SalesVoucherProduct
    {
        public int? SalesQuantity { get; set; }
        public int SalesVoucherID { get; set; }
        public SalesVoucher? SalesVoucher { get; set; }
        public int ProductID { get; set; }
        public Product? Product { get; set; }
    }
}
