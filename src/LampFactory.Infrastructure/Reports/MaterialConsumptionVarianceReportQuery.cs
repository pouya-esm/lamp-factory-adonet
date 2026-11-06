using LampFactory.Application.Features.Reports;
using LampFactory.Application.Abstractions.Persistence.Reports;
using LampFactory.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;

namespace LampFactory.Infrastructure.Reports
{
    public class MaterialConsumptionVarianceReportQuery : IMaterialConsumptionVarianceReportQuery
    {
        private readonly ApplicationDbContext _context;

        public MaterialConsumptionVarianceReportQuery(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<MaterialConsumptionVarianceDTO>> GetMaterialConsumptionVarianceAsync(DateTime? startDate, string? rawMaterialStandardNumber)
        {
            var results = new List<MaterialConsumptionVarianceDTO>();

            string query = @"
                WITH ActualConsumption AS (
                    SELECT 
                        RV.ProductionDocumentID,
                        RVP.ProductID,
                        SUM(RVP.RequestedQuantity) AS TotalRequestedQuantity
                    FROM RequestVoucher RV
                    INNER JOIN RequestVoucherProduct RVP ON RVP.RequestVoucherID = RV.RequestVoucherID
                    GROUP BY RV.ProductionDocumentID, RVP.ProductID
                )
                SELECT 
                    PD.ProductionDocumentID,
                    FP.ProductType + ' - ' + ISNULL(FP.ProductColor, '') + ' (' + FP.StandardNumber + ')' AS FinalProductName,
                    PD.ProductionDate,
                    PD.ProducedQuantity,
                    RM.StandardNumber                              AS RawMaterialStandardNumber,
                    RM.Material                                    AS RawMaterialName,
                    RM.ConsumptionUnit,                             
                    BC.ConsumptionAmount                           AS StandardConsumptionPerUnit,
                    (BC.ConsumptionAmount * PD.ProducedQuantity)   AS StandardTotalConsumption,
                    AC.TotalRequestedQuantity                      AS ActualConsumption,
                    (AC.TotalRequestedQuantity - (BC.ConsumptionAmount * PD.ProducedQuantity)) AS ConsumptionVariance
                FROM ProductionDocument PD
                INNER JOIN BOM B                     ON PD.ProductID = B.ProductID
                INNER JOIN BOMComponent BC           ON B.BOM_ID = BC.BOM_ID
                INNER JOIN Product RM                ON BC.ProductID = RM.ProductID
                INNER JOIN Product FP                ON PD.ProductID = FP.ProductID
                LEFT JOIN ActualConsumption AC       ON AC.ProductionDocumentID = PD.ProductionDocumentID
                                                     AND AC.ProductID = BC.ProductID
                WHERE 1=1 ";

            if (startDate.HasValue)
                query += " AND PD.ProductionDate >= @StartDate ";

            if (!string.IsNullOrWhiteSpace(rawMaterialStandardNumber))
                query += " AND RM.StandardNumber = @RawMaterialStandardNumber ";

            using (var conn = _context.CreateConnection())
            using (var cmd = new SqlCommand(query, conn))
            {
                if (startDate.HasValue)
                    cmd.Parameters.AddWithValue("@StartDate", startDate.Value);

                if (!string.IsNullOrWhiteSpace(rawMaterialStandardNumber))
                    cmd.Parameters.AddWithValue("@RawMaterialStandardNumber", rawMaterialStandardNumber);

                await conn.OpenAsync();
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        results.Add(new MaterialConsumptionVarianceDTO
                        {
                            ProductionDocumentID = reader.GetInt32(0),
                            FinalProductName = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                            ProductionDate = reader.IsDBNull(2) ? null : reader.GetDateTime(2),
                            ProducedQuantity = reader.IsDBNull(3) ? null : reader.GetInt32(3),
                            RawMaterialStandardNumber = reader.IsDBNull(4) ? null : reader.GetString(4),
                            RawMaterialName = reader.IsDBNull(5) ? null : reader.GetString(5),
                            ConsumptionUnit = reader.IsDBNull(6) ? null : reader.GetString(6),
                            StandardConsumptionPerUnit = reader.IsDBNull(7) ? null : reader.GetDouble(7),
                            StandardTotalConsumption = reader.IsDBNull(8) ? null : reader.GetDouble(8),
                            ActualConsumption = reader.IsDBNull(9) ? null : reader.GetInt32(9),
                            ConsumptionVariance = reader.IsDBNull(10) ? null : reader.GetDouble(10)
                        });
                    }
                }
            }

            return results;
        }
    }
}
