namespace LampFactory.Domain.Vouchers
{
    public class ChargeVoucher : AuditableEntity
    {
        public int ChargeVoucherID { get; set; }
        public DateTime? ChargeDate { get; set; }
        public int WarehouseID { get; set; }
        public Warehouse? Warehouse { get; set; }
        public int ProductionDocumentID { get; set; }
        public ProductionDocument? ProductionDocument { get; set; }
    }
}
