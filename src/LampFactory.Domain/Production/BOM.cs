
namespace LampFactory.Domain.Production
{
    public class BOM : AuditableEntity
    {
        public int BOM_ID { get; set; }
        public int ProductID { get; set; }
        public Product? Product { get; set; }
    }
}
