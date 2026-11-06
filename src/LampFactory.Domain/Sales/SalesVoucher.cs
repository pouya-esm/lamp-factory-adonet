namespace LampFactory.Domain.Sales
{
    public class SalesVoucher : AuditableEntity
    {
        public int SalesVoucherID { get; set; }
        public int WarehouseID { get; set; }
        public Warehouse? Warehouse { get; set; }
    }
}
