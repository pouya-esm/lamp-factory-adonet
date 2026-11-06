namespace LampFactory.Domain.Purchasing
{
    public class RequestVoucher : AuditableEntity
    {
        public int RequestVoucherID { get; set; }
        public DateTime? RequestDate { get; set; }
        public int WarehouseID { get; set; }
        public Warehouse? Warehouse { get; set; }
        public int ProductionDocumentID { get; set; }
        public ProductionDocument? ProductionDocument { get; set; }
    }
}
