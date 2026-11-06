

using LampFactory.Application.Features.Production.BillOfMaterials;

namespace LampFactory.Application.Features.Production.BillOfMaterials
{
    public class BOMDTO
    {
        public BOM BOM { get; set; } = new BOM();
        public List<BOMComponent> Components { get; set; } = new List<BOMComponent>();
    }
}
