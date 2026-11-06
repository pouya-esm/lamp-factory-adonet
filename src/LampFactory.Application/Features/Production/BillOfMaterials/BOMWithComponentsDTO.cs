using System.Text.Json.Serialization;

namespace LampFactory.Application.Features.Production.BillOfMaterials;

// Read-model exposed by BOMService. Domain entities (BOM / BOMComponent) carry
// navigation properties that must never appear in the API payload, so this
// DTO holds exactly the fields the wire contract has today. Property names
// match the previous anonymous-object serialization ({ BOM, Components }):
// BOM_ID serializes as "boM_ID" under the camelCase policy, and the C# member
// is spelled BomId with an explicit JsonPropertyName to (a) satisfy CA1707 and
// (b) keep the JSON key identical to the historical contract.
public class BOMResponseDTO
{
    [JsonPropertyName("boM_ID")]
    public int BomId { get; set; }
    public int ProductID { get; set; }
    public DateTime? CreatedAt { get; set; }
    public int? CreatedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public int? ModifiedBy { get; set; }
}

public class BOMComponentResponseDTO
{
    [JsonPropertyName("boM_ID")]
    public int BomId { get; set; }
    public int ProductID { get; set; }
    public double? ConsumptionAmount { get; set; }
}

// Aggregate response for one bill of materials plus its component lines.
public class BOMWithComponentsDTO
{
    public BOMResponseDTO BOM { get; set; } = new BOMResponseDTO();
    public IReadOnlyList<BOMComponentResponseDTO> Components { get; set; } = new List<BOMComponentResponseDTO>();
}
