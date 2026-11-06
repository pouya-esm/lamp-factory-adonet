using LampFactory.Application.Features.Reports;
using LampFactory.Application.Abstractions.Persistence.Reports;
using LampFactory.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using System.Data;

namespace LampFactory.Infrastructure.Reports
{
    public class ProductionTraceabilityReportQuery : IProductionTraceabilityReportQuery
    {
        private readonly ApplicationDbContext _context;

        public ProductionTraceabilityReportQuery(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<ProductionTraceabilityReportDTO>> GetProductionTraceabilityAsync(DateTime? startDate, string? standardNumber)
        {
            var results = new List<ProductionTraceabilityReportDTO>();

            string query = @"
                SELECT 
                    PD.ProductionDocumentID,
                    PR.ProductType + ' - ' + ISNULL(PR.ProductColor, '') + ' (' + PR.StandardNumber + ')' AS ProductName,
                    PL.ProductionLineID   AS LineNumber,
                    PL.[Status]           AS LineStatus,
                    O.FirstName + ' ' + O.LastName AS OperatorName,
                    PDO.OperatorRole,
                    PD.ProductionDate,
                    PD.ProductionShift,
                    PD.ProducedQuantity,
                    PD.RejectedQuantity,
                    PP.[Priority]         AS PlanPriority,
                    PP.TargetDate
                FROM ProductionDocument PD
                INNER JOIN ProductionPlan PP             ON PD.ProductionPlanID = PP.ProductionPlanID
                INNER JOIN Product PR                    ON PD.ProductID = PR.ProductID
                INNER JOIN ProductionLine PL              ON PD.ProductionLineID = PL.ProductionLineID
                INNER JOIN ProductionDocumentOperator PDO ON PD.ProductionDocumentID = PDO.ProductionDocumentID
                INNER JOIN Operator O                     ON PDO.OperatorID = O.OperatorID
                WHERE 1=1 ";

            if (startDate.HasValue)
                query += " AND PD.ProductionDate >= @StartDate ";

            if (!string.IsNullOrWhiteSpace(standardNumber))
                query += " AND PR.StandardNumber = @StandardNumber ";

            using (var conn = _context.CreateConnection())
            using (var cmd = new SqlCommand(query, conn))
            {
                if (startDate.HasValue)
                    cmd.Parameters.AddWithValue("@StartDate", startDate.Value);

                if (!string.IsNullOrWhiteSpace(standardNumber))
                    cmd.Parameters.AddWithValue("@StandardNumber", standardNumber);

                await conn.OpenAsync();
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        results.Add(new ProductionTraceabilityReportDTO
                        {
                            ProductionDocumentID = reader.GetInt32(0),
                            ProductName = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                            LineNumber = reader.GetInt32(2),
                            LineStatus = reader.GetBoolean(3),
                            OperatorName = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                            OperatorRole = reader.IsDBNull(5) ? null : reader.GetString(5),
                            ProductionDate = reader.IsDBNull(6) ? null : reader.GetDateTime(6),
                            ProductionShift = reader.IsDBNull(7) ? null : reader.GetInt32(7),
                            ProducedQuantity = reader.IsDBNull(8) ? null : reader.GetInt32(8),
                            RejectedQuantity = reader.IsDBNull(9) ? null : reader.GetInt32(9),
                            PlanPriority = reader.IsDBNull(10) ? null : reader.GetInt32(10),
                            TargetDate = reader.IsDBNull(11) ? null : reader.GetDateTime(11)
                        });
                    }
                }
            }

            return results;
        }
    }
}
