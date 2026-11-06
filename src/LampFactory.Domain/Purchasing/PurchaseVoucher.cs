namespace LampFactory.Domain.Purchasing
{
    public class PurchaseVoucher : AuditableEntity
    {
        public int PurchaseVoucherID { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public int WarehouseID { get; set; }
        public Warehouse? Warehouse { get; set; }
    }
}
