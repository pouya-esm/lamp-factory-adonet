namespace LampFactory.Domain.Purchasing
{
    public class PurchaseVoucherProduct
    {
        public int? PurchasedQuantity { get; set; }
        public int PurchaseVoucherID { get; set; }
        public PurchaseVoucher? PurchaseVoucher { get; set; }
        public int ProductID { get; set; }
        public Product? Product { get; set; }
    }
}
