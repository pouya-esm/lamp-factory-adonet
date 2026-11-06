using LampFactory.Infrastructure.Persistence;
using LampFactory.Application.Features.Audit;
using LampFactory.Application.Abstractions.Persistence.Audit;
using LampFactory.Domain.IdentityAccess;
using LampFactory.Domain.Inventory;
using LampFactory.Domain.Operators;
using LampFactory.Domain.Production;
using LampFactory.Domain.Purchasing;
using LampFactory.Domain.QualityControl;
using LampFactory.Domain.Sales;
using LampFactory.Domain.Vouchers;
using LampFactory.Infrastructure.Security;
using Microsoft.Data.SqlClient;

namespace LampFactory.Infrastructure.Persistence.Repositories.Audit
{
    // Reads the audit stamps every repository writes. The schema keeps a single
    // audit pair per record, so one row here describes one business record's
    // latest state change (or its creation when it was never modified).
    public class AuditLogRepository : IAuditLogRepository
{
        private readonly ApplicationDbContext _context;

        public AuditLogRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // Every branch joins [User] twice to resolve actor ids into usernames.
        // Because those joined aliases also carry audit columns, each reference
        // below is qualified with its own table alias to stay unambiguous.
        public async Task<List<AuditLogDTO>> GetAllAsync(string? entityName, int? page, int? pageSize)
        {
            var logs = new List<AuditLogDTO>();

            int currentPage = (page.HasValue && page.Value > 0) ? page.Value : 1;
            int currentPageSize = (pageSize.HasValue && pageSize.Value > 0) ? pageSize.Value : 50;

            string query = @"
            SELECT EntityName, EntityID, EntityLabel, LastAction, LastAt, LastBy, ActorName,
                   CreatedAt, CreatedBy, CreatorName
            FROM (
                SELECT 'User' AS EntityName, u.UserID AS EntityID, u.Username AS EntityLabel,
                       CASE WHEN u.ModifiedAt IS NOT NULL THEN 'Modified' ELSE 'Created' END AS LastAction,
                       COALESCE(u.ModifiedAt, u.CreatedAt) AS LastAt,
                       COALESCE(u.ModifiedBy, u.CreatedBy) AS LastBy,
                       COALESCE(mu.Username, cu.Username) AS ActorName,
                       u.CreatedAt, u.CreatedBy, cu.Username AS CreatorName
                FROM [User] u
                LEFT JOIN [User] cu ON cu.UserID = u.CreatedBy
                LEFT JOIN [User] mu ON mu.UserID = u.ModifiedBy

                UNION ALL
                SELECT 'Role', r.RoleID, r.RoleName,
                       CASE WHEN r.ModifiedAt IS NOT NULL THEN 'Modified' ELSE 'Created' END,
                       COALESCE(r.ModifiedAt, r.CreatedAt), COALESCE(r.ModifiedBy, r.CreatedBy),
                       COALESCE(mu.Username, cu.Username), r.CreatedAt, r.CreatedBy, cu.Username
                FROM [Role] r
                LEFT JOIN [User] cu ON cu.UserID = r.CreatedBy
                LEFT JOIN [User] mu ON mu.UserID = r.ModifiedBy

                UNION ALL
                SELECT 'Resource', s.ResourceID, s.Name,
                       CASE WHEN s.ModifiedAt IS NOT NULL THEN 'Modified' ELSE 'Created' END,
                       COALESCE(s.ModifiedAt, s.CreatedAt), COALESCE(s.ModifiedBy, s.CreatedBy),
                       COALESCE(mu.Username, cu.Username), s.CreatedAt, s.CreatedBy, cu.Username
                FROM [Resource] s
                LEFT JOIN [User] cu ON cu.UserID = s.CreatedBy
                LEFT JOIN [User] mu ON mu.UserID = s.ModifiedBy

                UNION ALL
                SELECT 'Action', a.ActionID, a.Name,
                       CASE WHEN a.ModifiedAt IS NOT NULL THEN 'Modified' ELSE 'Created' END,
                       COALESCE(a.ModifiedAt, a.CreatedAt), COALESCE(a.ModifiedBy, a.CreatedBy),
                       COALESCE(mu.Username, cu.Username), a.CreatedAt, a.CreatedBy, cu.Username
                FROM [Action] a
                LEFT JOIN [User] cu ON cu.UserID = a.CreatedBy
                LEFT JOIN [User] mu ON mu.UserID = a.ModifiedBy

                UNION ALL
                SELECT 'Permission', p.PermissionID,
                       res.Name + ' / ' + act.Name,
                       CASE WHEN p.ModifiedAt IS NOT NULL THEN 'Modified' ELSE 'Created' END,
                       COALESCE(p.ModifiedAt, p.CreatedAt), COALESCE(p.ModifiedBy, p.CreatedBy),
                       COALESCE(mu.Username, cu.Username), p.CreatedAt, p.CreatedBy, cu.Username
                FROM Permission p
                INNER JOIN [Resource] res ON res.ResourceID = p.ResourceID
                INNER JOIN [Action] act ON act.ActionID = p.ActionID
                LEFT JOIN [User] cu ON cu.UserID = p.CreatedBy
                LEFT JOIN [User] mu ON mu.UserID = p.ModifiedBy

                UNION ALL
                SELECT 'Product', pr.ProductID,
                       pr.ProductType + ' / ' + ISNULL(pr.ProductColor, '-'),
                       CASE WHEN pr.ModifiedAt IS NOT NULL THEN 'Modified' ELSE 'Created' END,
                       COALESCE(pr.ModifiedAt, pr.CreatedAt), COALESCE(pr.ModifiedBy, pr.CreatedBy),
                       COALESCE(mu.Username, cu.Username), pr.CreatedAt, pr.CreatedBy, cu.Username
                FROM Product pr
                LEFT JOIN [User] cu ON cu.UserID = pr.CreatedBy
                LEFT JOIN [User] mu ON mu.UserID = pr.ModifiedBy

                UNION ALL
                SELECT 'BOM', b.BOM_ID,
                       'BOM for Product ' + CAST(b.ProductID AS NVARCHAR(20)),
                       CASE WHEN b.ModifiedAt IS NOT NULL THEN 'Modified' ELSE 'Created' END,
                       COALESCE(b.ModifiedAt, b.CreatedAt), COALESCE(b.ModifiedBy, b.CreatedBy),
                       COALESCE(mu.Username, cu.Username), b.CreatedAt, b.CreatedBy, cu.Username
                FROM BOM b
                LEFT JOIN [User] cu ON cu.UserID = b.CreatedBy
                LEFT JOIN [User] mu ON mu.UserID = b.ModifiedBy

                UNION ALL
                SELECT 'Warehouse', w.WarehouseID,
                       ISNULL(w.WarehouseName, 'Warehouse ' + CAST(w.WarehouseID AS NVARCHAR(20))),
                       CASE WHEN w.ModifiedAt IS NOT NULL THEN 'Modified' ELSE 'Created' END,
                       COALESCE(w.ModifiedAt, w.CreatedAt), COALESCE(w.ModifiedBy, w.CreatedBy),
                       COALESCE(mu.Username, cu.Username), w.CreatedAt, w.CreatedBy, cu.Username
                FROM Warehouse w
                LEFT JOIN [User] cu ON cu.UserID = w.CreatedBy
                LEFT JOIN [User] mu ON mu.UserID = w.ModifiedBy

                UNION ALL
                SELECT 'ProductionPlan', pp.ProductionPlanID,
                       'Plan issued ' + ISNULL(CONVERT(NVARCHAR(12), pp.IssueDate, 23), '-'),
                       CASE WHEN pp.ModifiedAt IS NOT NULL THEN 'Modified' ELSE 'Created' END,
                       COALESCE(pp.ModifiedAt, pp.CreatedAt), COALESCE(pp.ModifiedBy, pp.CreatedBy),
                       COALESCE(mu.Username, cu.Username), pp.CreatedAt, pp.CreatedBy, cu.Username
                FROM ProductionPlan pp
                LEFT JOIN [User] cu ON cu.UserID = pp.CreatedBy
                LEFT JOIN [User] mu ON mu.UserID = pp.ModifiedBy

                UNION ALL
                SELECT 'ProductionLine', pl.ProductionLineID,
                       'Line ' + CAST(pl.ProductionLineID AS NVARCHAR(20)),
                       CASE WHEN pl.ModifiedAt IS NOT NULL THEN 'Modified' ELSE 'Created' END,
                       COALESCE(pl.ModifiedAt, pl.CreatedAt), COALESCE(pl.ModifiedBy, pl.CreatedBy),
                       COALESCE(mu.Username, cu.Username), pl.CreatedAt, pl.CreatedBy, cu.Username
                FROM ProductionLine pl
                LEFT JOIN [User] cu ON cu.UserID = pl.CreatedBy
                LEFT JOIN [User] mu ON mu.UserID = pl.ModifiedBy

                UNION ALL
                SELECT 'ProductionDocument', pd.ProductionDocumentID,
                       'Doc ' + CAST(pd.ProductionDocumentID AS NVARCHAR(20)),
                       CASE WHEN pd.ModifiedAt IS NOT NULL THEN 'Modified' ELSE 'Created' END,
                       COALESCE(pd.ModifiedAt, pd.CreatedAt), COALESCE(pd.ModifiedBy, pd.CreatedBy),
                       COALESCE(mu.Username, cu.Username), pd.CreatedAt, pd.CreatedBy, cu.Username
                FROM ProductionDocument pd
                LEFT JOIN [User] cu ON cu.UserID = pd.CreatedBy
                LEFT JOIN [User] mu ON mu.UserID = pd.ModifiedBy

                UNION ALL
                SELECT 'Operator', o.OperatorID,
                       ISNULL(o.FirstName + ' ' + o.LastName, o.NationalID),
                       CASE WHEN o.ModifiedAt IS NOT NULL THEN 'Modified' ELSE 'Created' END,
                       COALESCE(o.ModifiedAt, o.CreatedAt), COALESCE(o.ModifiedBy, o.CreatedBy),
                       COALESCE(mu.Username, cu.Username), o.CreatedAt, o.CreatedBy, cu.Username
                FROM Operator o
                LEFT JOIN [User] cu ON cu.UserID = o.CreatedBy
                LEFT JOIN [User] mu ON mu.UserID = o.ModifiedBy

                UNION ALL
                SELECT 'LineStop', ls.LineStopID,
                       ISNULL(LEFT(ls.Reason, 40), 'Stop ' + CAST(ls.LineStopID AS NVARCHAR(20))),
                       CASE WHEN ls.ModifiedAt IS NOT NULL THEN 'Modified' ELSE 'Created' END,
                       COALESCE(ls.ModifiedAt, ls.CreatedAt), COALESCE(ls.ModifiedBy, ls.CreatedBy),
                       COALESCE(mu.Username, cu.Username), ls.CreatedAt, ls.CreatedBy, cu.Username
                FROM LineStop ls
                LEFT JOIN [User] cu ON cu.UserID = ls.CreatedBy
                LEFT JOIN [User] mu ON mu.UserID = ls.ModifiedBy

                UNION ALL
                SELECT 'QualityControl', qc.QualityControlID,
                       ISNULL(qc.BatchNumber, 'QC ' + CAST(qc.QualityControlID AS NVARCHAR(20))),
                       CASE WHEN qc.ModifiedAt IS NOT NULL THEN 'Modified' ELSE 'Created' END,
                       COALESCE(qc.ModifiedAt, qc.CreatedAt), COALESCE(qc.ModifiedBy, qc.CreatedBy),
                       COALESCE(mu.Username, cu.Username), qc.CreatedAt, qc.CreatedBy, cu.Username
                FROM QualityControl qc
                LEFT JOIN [User] cu ON cu.UserID = qc.CreatedBy
                LEFT JOIN [User] mu ON mu.UserID = qc.ModifiedBy

                UNION ALL
                SELECT 'SalesVoucher', sv.SalesVoucherID,
                       'Sale #' + CAST(sv.SalesVoucherID AS NVARCHAR(20)),
                       CASE WHEN sv.ModifiedAt IS NOT NULL THEN 'Modified' ELSE 'Created' END,
                       COALESCE(sv.ModifiedAt, sv.CreatedAt), COALESCE(sv.ModifiedBy, sv.CreatedBy),
                       COALESCE(mu.Username, cu.Username), sv.CreatedAt, sv.CreatedBy, cu.Username
                FROM SalesVoucher sv
                LEFT JOIN [User] cu ON cu.UserID = sv.CreatedBy
                LEFT JOIN [User] mu ON mu.UserID = sv.ModifiedBy

                UNION ALL
                SELECT 'PurchaseVoucher', pv.PurchaseVoucherID,
                       'Purchase #' + CAST(pv.PurchaseVoucherID AS NVARCHAR(20)),
                       CASE WHEN pv.ModifiedAt IS NOT NULL THEN 'Modified' ELSE 'Created' END,
                       COALESCE(pv.ModifiedAt, pv.CreatedAt), COALESCE(pv.ModifiedBy, pv.CreatedBy),
                       COALESCE(mu.Username, cu.Username), pv.CreatedAt, pv.CreatedBy, cu.Username
                FROM PurchaseVoucher pv
                LEFT JOIN [User] cu ON cu.UserID = pv.CreatedBy
                LEFT JOIN [User] mu ON mu.UserID = pv.ModifiedBy

                UNION ALL
                SELECT 'ChargeVoucher', cv.ChargeVoucherID,
                       'Charge #' + CAST(cv.ChargeVoucherID AS NVARCHAR(20)),
                       CASE WHEN cv.ModifiedAt IS NOT NULL THEN 'Modified' ELSE 'Created' END,
                       COALESCE(cv.ModifiedAt, cv.CreatedAt), COALESCE(cv.ModifiedBy, cv.CreatedBy),
                       COALESCE(mu.Username, cu.Username), cv.CreatedAt, cv.CreatedBy, cu.Username
                FROM ChargeVoucher cv
                LEFT JOIN [User] cu ON cu.UserID = cv.CreatedBy
                LEFT JOIN [User] mu ON mu.UserID = cv.ModifiedBy

                UNION ALL
                SELECT 'RequestVoucher', rv.RequestVoucherID,
                       'Request #' + CAST(rv.RequestVoucherID AS NVARCHAR(20)),
                       CASE WHEN rv.ModifiedAt IS NOT NULL THEN 'Modified' ELSE 'Created' END,
                       COALESCE(rv.ModifiedAt, rv.CreatedAt), COALESCE(rv.ModifiedBy, rv.CreatedBy),
                       COALESCE(mu.Username, cu.Username), rv.CreatedAt, rv.CreatedBy, cu.Username
                FROM RequestVoucher rv
                LEFT JOIN [User] cu ON cu.UserID = rv.CreatedBy
                LEFT JOIN [User] mu ON mu.UserID = rv.ModifiedBy
            ) AS AuditRows
            WHERE (@EntityName IS NULL OR EntityName = @EntityName)
            ORDER BY LastAt DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;";

            using (var conn = _context.CreateConnection())
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@EntityName", (object?)entityName ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Offset", (currentPage - 1) * currentPageSize);
                cmd.Parameters.AddWithValue("@PageSize", currentPageSize);

                await conn.OpenAsync();
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        logs.Add(new AuditLogDTO
                        {
                            EntityName = reader.GetString(0),
                            EntityID = reader.GetInt32(1),
                            EntityLabel = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                            LastAction = reader.GetString(3),
                            LastAt = reader.IsDBNull(4) ? null : reader.GetDateTime(4),
                            LastBy = reader.IsDBNull(5) ? null : reader.GetInt32(5),
                            LastByUsername = reader.IsDBNull(6) ? null : reader.GetString(6),
                            CreatedAt = reader.IsDBNull(7) ? null : reader.GetDateTime(7),
                            CreatedBy = reader.IsDBNull(8) ? null : reader.GetInt32(8),
                            CreatedByUsername = reader.IsDBNull(9) ? null : reader.GetString(9)
                        });
                    }
                }
            }
            return logs;
        }

        public async Task<int> CountAsync(string? entityName)
        {
            string query = @"
            SELECT COUNT(*) FROM (
                SELECT 'User' AS EntityName FROM [User]
                UNION ALL SELECT 'Role' FROM [Role]
                UNION ALL SELECT 'Resource' FROM [Resource]
                UNION ALL SELECT 'Action' FROM [Action]
                UNION ALL SELECT 'Permission' FROM Permission
                UNION ALL SELECT 'Product' FROM Product
                UNION ALL SELECT 'BOM' FROM BOM
                UNION ALL SELECT 'Warehouse' FROM Warehouse
                UNION ALL SELECT 'ProductionPlan' FROM ProductionPlan
                UNION ALL SELECT 'ProductionLine' FROM ProductionLine
                UNION ALL SELECT 'ProductionDocument' FROM ProductionDocument
                UNION ALL SELECT 'Operator' FROM Operator
                UNION ALL SELECT 'LineStop' FROM LineStop
                UNION ALL SELECT 'QualityControl' FROM QualityControl
                UNION ALL SELECT 'SalesVoucher' FROM SalesVoucher
                UNION ALL SELECT 'PurchaseVoucher' FROM PurchaseVoucher
                UNION ALL SELECT 'ChargeVoucher' FROM ChargeVoucher
                UNION ALL SELECT 'RequestVoucher' FROM RequestVoucher
            ) AS AuditRows
            WHERE (@EntityName IS NULL OR EntityName = @EntityName);";

            using (var conn = _context.CreateConnection())
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@EntityName", (object?)entityName ?? DBNull.Value);

                await conn.OpenAsync();
                var result = await cmd.ExecuteScalarAsync();
                return result != null && result != DBNull.Value ? Convert.ToInt32(result) : 0;
            }
        }

        public async Task<List<string>> GetEntityNamesAsync()
        {
            var names = new List<string>
            {
                "User", "Role", "Resource", "Action", "Permission", "Product", "BOM",
                "Warehouse", "ProductionPlan", "ProductionLine", "ProductionDocument",
                "Operator", "LineStop", "QualityControl", "SalesVoucher",
                "PurchaseVoucher", "ChargeVoucher", "RequestVoucher"
            };
            return await Task.FromResult(names);
        }
    }
}
