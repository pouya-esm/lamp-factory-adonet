using LampFactory.Infrastructure.Persistence;
using LampFactory.Application.Abstractions.Persistence.Production.ProductionDocuments;
using LampFactory.Domain.Production;
using LampFactory.Infrastructure.Security;
using Microsoft.Data.SqlClient;

namespace LampFactory.Infrastructure.Persistence.Repositories.Production
{
    public class ProductionDocumentRepository : IProductionDocumentRepository
{
        private readonly ApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public ProductionDocumentRepository(ApplicationDbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<int> CreateWithOperatorsAsync(ProductionDocument document, List<ProductionDocumentOperator> operators)
        {
            using (var conn = _context.CreateConnection())
            {
                await conn.OpenAsync();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        string docQuery = @"INSERT INTO ProductionDocument
                                           (ProductionDate, ProducedQuantity, RejectedQuantity, ProductionShift, ProductionPlanID, ProductID, ProductionLineID, CreatedBy)
                                           VALUES
                                           (@ProductionDate, @ProducedQuantity, @RejectedQuantity, @ProductionShift, @ProductionPlanID, @ProductID, @ProductionLineID, @CreatedBy);
                                           SELECT SCOPE_IDENTITY();";

                        int newDocId;
                        using (var cmd = new SqlCommand(docQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@ProductionDate", (object?)document.ProductionDate ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@ProducedQuantity", (object?)document.ProducedQuantity ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@RejectedQuantity", (object?)document.RejectedQuantity ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@ProductionShift", (object?)document.ProductionShift ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@ProductionPlanID", document.ProductionPlanID);
                            cmd.Parameters.AddWithValue("@ProductID", document.ProductID);
                            cmd.Parameters.AddWithValue("@ProductionLineID", document.ProductionLineID);
                            cmd.Parameters.AddWithValue("@CreatedBy", _currentUserService.ActorId);

                            var result = await cmd.ExecuteScalarAsync();
                            newDocId = Convert.ToInt32(result);
                        }

                        if (operators != null && operators.Count > 0)
                        {
                            string opQuery = @"INSERT INTO ProductionDocumentOperator 
                                             (ProductionDocumentID, OperatorID, OperatorRole, AssignedDateFrom, AssignedDateUntil) 
                                             VALUES 
                                             (@ProductionDocumentID, @OperatorID, @OperatorRole, @AssignedDateFrom, @AssignedDateUntil)";

                            foreach (var item in operators)
                            {
                                using (var cmdOp = new SqlCommand(opQuery, conn, transaction))
                                {
                                    cmdOp.Parameters.AddWithValue("@ProductionDocumentID", newDocId);
                                    cmdOp.Parameters.AddWithValue("@OperatorID", item.OperatorID);
                                    cmdOp.Parameters.AddWithValue("@OperatorRole", (object?)item.OperatorRole ?? DBNull.Value);
                                    cmdOp.Parameters.AddWithValue("@AssignedDateFrom", (object?)item.AssignedDateFrom ?? DBNull.Value);
                                    cmdOp.Parameters.AddWithValue("@AssignedDateUntil", (object?)item.AssignedDateUntil ?? DBNull.Value);

                                    await cmdOp.ExecuteNonQueryAsync();
                                }
                            }
                        }

                        await transaction.CommitAsync();
                        return newDocId;
                    }
                    catch
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                }
            }
        }


        public async Task<(ProductionDocument? Document, List<ProductionDocumentOperator> Operators)> GetByIdWithOperatorsAsync(int documentId)
        {
            ProductionDocument? document = null;
            var operators = new List<ProductionDocumentOperator>();

            using (var conn = _context.CreateConnection())
            {
                await conn.OpenAsync();

                string query = @"
            SELECT
                d.ProductionDocumentID,
                d.ProductionDate,
                d.ProducedQuantity,
                d.RejectedQuantity,
                d.ProductionShift,
                d.ProductionPlanID,
                d.ProductID,
                d.ProductionLineID,
                o.OperatorID,
                o.OperatorRole,
                o.AssignedDateFrom,
                o.AssignedDateUntil,
                d.CreatedAt, d.CreatedBy, d.ModifiedAt, d.ModifiedBy
            FROM ProductionDocument d
            LEFT JOIN ProductionDocumentOperator o ON d.ProductionDocumentID = o.ProductionDocumentID
            WHERE d.ProductionDocumentID = @Id";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", documentId);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            if (document == null)
                            {
                                document = new ProductionDocument
                                {
                                    ProductionDocumentID = reader.GetInt32(0),
                                    ProductionDate = reader.IsDBNull(1) ? null : reader.GetDateTime(1),
                                    ProducedQuantity = reader.IsDBNull(2) ? null : reader.GetInt32(2),
                                    RejectedQuantity = reader.IsDBNull(3) ? null : reader.GetInt32(3),
                                    ProductionShift = reader.IsDBNull(4) ? null : reader.GetInt32(4),
                                    ProductionPlanID = reader.GetInt32(5),
                                    ProductID = reader.GetInt32(6),
                                    ProductionLineID = reader.GetInt32(7),
                                    CreatedAt = reader.IsDBNull(12) ? null : reader.GetDateTime(12),
                                    CreatedBy = reader.IsDBNull(13) ? null : reader.GetInt32(13),
                                    ModifiedAt = reader.IsDBNull(14) ? null : reader.GetDateTime(14),
                                    ModifiedBy = reader.IsDBNull(15) ? null : reader.GetInt32(15)
                                };
                            }

                            if (!reader.IsDBNull(8))
                            {
                                operators.Add(new ProductionDocumentOperator
                                {
                                    ProductionDocumentID = reader.GetInt32(0),
                                    OperatorID = reader.GetInt32(8),
                                    OperatorRole = reader.IsDBNull(9) ? null : reader.GetString(9),
                                    AssignedDateFrom = reader.IsDBNull(10) ? null : reader.GetDateTime(10),
                                    AssignedDateUntil = reader.IsDBNull(11) ? null : reader.GetDateTime(11)
                                });
                            }
                        }
                    }
                }
            }

            return (document, operators);
        }

        public async Task<List<(ProductionDocument Document, List<ProductionDocumentOperator> Operators)>> GetAllWithOperatorsAsync()
        {
            var dictionary = new Dictionary<int, (ProductionDocument Document, List<ProductionDocumentOperator> Operators)>();

            using (var conn = _context.CreateConnection())
            {
                await conn.OpenAsync();

                string query = @"
            SELECT
                d.ProductionDocumentID,
                d.ProductionDate,
                d.ProducedQuantity,
                d.RejectedQuantity,
                d.ProductionShift,
                d.ProductionPlanID,
                d.ProductID,
                d.ProductionLineID,
                o.OperatorID,
                o.OperatorRole,
                o.AssignedDateFrom,
                o.AssignedDateUntil,
                d.CreatedAt, d.CreatedBy, d.ModifiedAt, d.ModifiedBy
            FROM ProductionDocument d
            LEFT JOIN ProductionDocumentOperator o ON d.ProductionDocumentID = o.ProductionDocumentID";

                using (var cmd = new SqlCommand(query, conn))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        int currentDocId = reader.GetInt32(0);
                        if (!dictionary.ContainsKey(currentDocId))
                        {
                            var newDocument = new ProductionDocument
                            {
                                ProductionDocumentID = currentDocId,
                                ProductionDate = reader.IsDBNull(1) ? null : reader.GetDateTime(1),
                                ProducedQuantity = reader.IsDBNull(2) ? null : reader.GetInt32(2),
                                RejectedQuantity = reader.IsDBNull(3) ? null : reader.GetInt32(3),
                                ProductionShift = reader.IsDBNull(4) ? null : reader.GetInt32(4),
                                ProductionPlanID = reader.GetInt32(5),
                                ProductID = reader.GetInt32(6),
                                ProductionLineID = reader.GetInt32(7),
                                CreatedAt = reader.IsDBNull(12) ? null : reader.GetDateTime(12),
                                CreatedBy = reader.IsDBNull(13) ? null : reader.GetInt32(13),
                                ModifiedAt = reader.IsDBNull(14) ? null : reader.GetDateTime(14),
                                ModifiedBy = reader.IsDBNull(15) ? null : reader.GetInt32(15)
                            };

                            dictionary[currentDocId] = (newDocument, new List<ProductionDocumentOperator>());
                        }

                        if (!reader.IsDBNull(8))
                        {
                            var docOperator = new ProductionDocumentOperator
                            {
                                ProductionDocumentID = currentDocId,
                                OperatorID = reader.GetInt32(8),
                                OperatorRole = reader.IsDBNull(9) ? null : reader.GetString(9),
                                AssignedDateFrom = reader.IsDBNull(10) ? null : reader.GetDateTime(10),
                                AssignedDateUntil = reader.IsDBNull(11) ? null : reader.GetDateTime(11)
                            };

                            dictionary[currentDocId].Operators.Add(docOperator);
                        }
                    }
                }
            }

            return dictionary.Values.ToList();
        }


        public async Task<bool> UpdateWithOperatorsAsync(ProductionDocument document, List<ProductionDocumentOperator> operators)
        {
            using (var conn = _context.CreateConnection())
            {
                await conn.OpenAsync();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        string updateDocQuery = @"UPDATE ProductionDocument
                                                  SET ProductionDate = @ProductionDate,
                                                      ProducedQuantity = @ProducedQuantity,
                                                      RejectedQuantity = @RejectedQuantity,
                                                      ProductionShift = @ProductionShift,
                                                      ProductionPlanID = @ProductionPlanID,
                                                      ProductID = @ProductID,
                                                      ProductionLineID = @ProductionLineID,
                                                      ModifiedAt = GETDATE(), ModifiedBy = @ModifiedBy
                                                  WHERE ProductionDocumentID = @ProductionDocumentID";

                        using (var cmd = new SqlCommand(updateDocQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@ProductionDocumentID", document.ProductionDocumentID);
                            cmd.Parameters.AddWithValue("@ProductionDate", (object?)document.ProductionDate ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@ProducedQuantity", (object?)document.ProducedQuantity ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@RejectedQuantity", (object?)document.RejectedQuantity ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@ProductionShift", (object?)document.ProductionShift ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@ProductionPlanID", document.ProductionPlanID);
                            cmd.Parameters.AddWithValue("@ProductID", document.ProductID);
                            cmd.Parameters.AddWithValue("@ProductionLineID", document.ProductionLineID);
                            cmd.Parameters.AddWithValue("@ModifiedBy", _currentUserService.ActorId);

                            await cmd.ExecuteNonQueryAsync();
                        }

                        string deleteOpsQuery = "DELETE FROM ProductionDocumentOperator WHERE ProductionDocumentID = @DocId";
                        using (var cmdDel = new SqlCommand(deleteOpsQuery, conn, transaction))
                        {
                            cmdDel.Parameters.AddWithValue("@DocId", document.ProductionDocumentID);
                            await cmdDel.ExecuteNonQueryAsync();
                        }

                        if (operators != null && operators.Count > 0)
                        {
                            string insertOpQuery = @"INSERT INTO ProductionDocumentOperator 
                                                     (ProductionDocumentID, OperatorID, OperatorRole, AssignedDateFrom, AssignedDateUntil) 
                                                     VALUES 
                                                     (@ProductionDocumentID, @OperatorID, @OperatorRole, @AssignedDateFrom, @AssignedDateUntil)";

                            foreach (var item in operators)
                            {
                                using (var cmdIns = new SqlCommand(insertOpQuery, conn, transaction))
                                {
                                    cmdIns.Parameters.AddWithValue("@ProductionDocumentID", document.ProductionDocumentID);
                                    cmdIns.Parameters.AddWithValue("@OperatorID", item.OperatorID);
                                    cmdIns.Parameters.AddWithValue("@OperatorRole", (object?)item.OperatorRole ?? DBNull.Value);
                                    cmdIns.Parameters.AddWithValue("@AssignedDateFrom", (object?)item.AssignedDateFrom ?? DBNull.Value);
                                    cmdIns.Parameters.AddWithValue("@AssignedDateUntil", (object?)item.AssignedDateUntil ?? DBNull.Value);

                                    await cmdIns.ExecuteNonQueryAsync();
                                }
                            }
                        }

                        await transaction.CommitAsync();
                        return true;
                    }
                    catch
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                }
            }
        }

   
        public async Task<bool> RemoveWithOperatorsAsync(int documentId)
        {
            using (var conn = _context.CreateConnection())
            {
                await conn.OpenAsync();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        string deleteOperatorsQuery = "DELETE FROM ProductionDocumentOperator WHERE ProductionDocumentID = @DocId";
                        using (var cmd = new SqlCommand(deleteOperatorsQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@DocId", documentId);
                            await cmd.ExecuteNonQueryAsync();
                        }

                        string deleteDocQuery = "DELETE FROM ProductionDocument WHERE ProductionDocumentID = @DocId";
                        using (var cmd = new SqlCommand(deleteDocQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@DocId", documentId);
                            await cmd.ExecuteNonQueryAsync();
                        }

                        await transaction.CommitAsync();
                        return true;
                    }
                    catch
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                }
            }
        }
    }
}
