namespace LampFactory.Domain.Vouchers
{
    public class ChargeVoucherProduct
    {
        public int? ChargeQuantity { get; set; }
        public int ChargeVoucherID { get; set; }
        public ChargeVoucher? ChargeVoucher { get; set; }
        public int ProductID { get; set; }
        public Product? Product { get; set; }
    }
}
