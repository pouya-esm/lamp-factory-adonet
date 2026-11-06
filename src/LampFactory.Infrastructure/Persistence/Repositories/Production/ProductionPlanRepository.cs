using LampFactory.Infrastructure.Persistence;
using LampFactory.Application.Abstractions.Persistence.Production.ProductionPlans;
using LampFactory.Domain.Production;
using LampFactory.Infrastructure.Security;
using Microsoft.Data.SqlClient;

namespace LampFactory.Infrastructure.Persistence.Repositories.Production
{
    public class ProductionPlanRepository : IProductionPlanRepository
{
        private readonly ApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public ProductionPlanRepository(ApplicationDbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<int> CreateWithProductsAsync(ProductionPlan plan, List<ProductionPlanProduct> products)
        {
            using (var conn = _context.CreateConnection())
            {
                await conn.OpenAsync();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        string planQuery = @"INSERT INTO ProductionPlan (IssueDate, TargetDate, Priority, CreatedBy)
                                            VALUES (@IssueDate, @TargetDate, @Priority, @CreatedBy);
                                            SELECT SCOPE_IDENTITY();";

                        int newPlanId;
                        using (var cmd = new SqlCommand(planQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@IssueDate", (object?)plan.IssueDate ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@TargetDate", (object?)plan.TargetDate ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@Priority", (object?)plan.Priority ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@CreatedBy", _currentUserService.ActorId);

                            var result = await cmd.ExecuteScalarAsync();
                            newPlanId = Convert.ToInt32(result);
                        }

                        if (products != null && products.Count > 0)
                        {
                            string productQuery = @"INSERT INTO ProductionPlanProduct (ProductionPlanID, ProductID, MinQuantity, MaxQuantity) 
                                                  VALUES (@ProductionPlanID, @ProductID, @MinQuantity, @MaxQuantity)";

                            foreach (var item in products)
                            {
                                using (var cmdItem = new SqlCommand(productQuery, conn, transaction))
                                {
                                    cmdItem.Parameters.AddWithValue("@ProductionPlanID", newPlanId);
                                    cmdItem.Parameters.AddWithValue("@ProductID", item.ProductID);
                                    cmdItem.Parameters.AddWithValue("@MinQuantity", (object?)item.MinQuantity ?? DBNull.Value);
                                    cmdItem.Parameters.AddWithValue("@MaxQuantity", (object?)item.MaxQuantity ?? DBNull.Value);

                                    await cmdItem.ExecuteNonQueryAsync();
                                }
                            }
                        }

                        await transaction.CommitAsync();
                        return newPlanId;
                    }
                    catch
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                }
            }
        }


        public async Task<(ProductionPlan? Plan, List<ProductionPlanProduct> Products)> GetByIdWithProductsAsync(int id)
        {
            ProductionPlan? plan = null;
            var products = new List<ProductionPlanProduct>();

            string query = @"
        SELECT
            p.ProductionPlanID, p.IssueDate, p.TargetDate, p.Priority,
            pp.ProductID, pp.MinQuantity, pp.MaxQuantity,
            p.CreatedAt, p.CreatedBy, p.ModifiedAt, p.ModifiedBy
        FROM ProductionPlan p
        LEFT JOIN ProductionPlanProduct pp ON p.ProductionPlanID = pp.ProductionPlanID
        WHERE p.ProductionPlanID = @id";

            using (var conn = _context.CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            if (plan == null)
                            {
                                plan = new ProductionPlan
                                {
                                    ProductionPlanId = reader.GetInt32(reader.GetOrdinal("ProductionPlanID")),
                                    IssueDate = reader.IsDBNull(reader.GetOrdinal("IssueDate")) ? null : reader.GetDateTime(reader.GetOrdinal("IssueDate")),
                                    TargetDate = reader.IsDBNull(reader.GetOrdinal("TargetDate")) ? null : reader.GetDateTime(reader.GetOrdinal("TargetDate")),
                                    Priority = reader.IsDBNull(reader.GetOrdinal("Priority")) ? null : reader.GetInt32(reader.GetOrdinal("Priority")),
                                    CreatedAt = reader.IsDBNull(reader.GetOrdinal("CreatedAt")) ? null : reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                                    CreatedBy = reader.IsDBNull(reader.GetOrdinal("CreatedBy")) ? null : reader.GetInt32(reader.GetOrdinal("CreatedBy")),
                                    ModifiedAt = reader.IsDBNull(reader.GetOrdinal("ModifiedAt")) ? null : reader.GetDateTime(reader.GetOrdinal("ModifiedAt")),
                                    ModifiedBy = reader.IsDBNull(reader.GetOrdinal("ModifiedBy")) ? null : reader.GetInt32(reader.GetOrdinal("ModifiedBy"))
                                };
                            }

                            if (!reader.IsDBNull(reader.GetOrdinal("ProductID")))
                            {
                                products.Add(new ProductionPlanProduct
                                {
                                    ProductionPlanID = id,
                                    ProductID = reader.GetInt32(reader.GetOrdinal("ProductID")),
                                    MinQuantity = reader.IsDBNull(reader.GetOrdinal("MinQuantity")) ? null : reader.GetInt32(reader.GetOrdinal("MinQuantity")),
                                    MaxQuantity = reader.IsDBNull(reader.GetOrdinal("MaxQuantity")) ? null : reader.GetInt32(reader.GetOrdinal("MaxQuantity"))
                                });
                            }
                        }
                    }
                }
            }

            // Matches the codebase convention: the tuple itself is never null,
            // a missing aggregate is signalled by a null root.
            return (plan, products);
        }


        public async Task<List<(ProductionPlan Plan, List<ProductionPlanProduct> Products)>> GetAllWithProductsAsync()
        {
            var planMap = new Dictionary<int, (ProductionPlan Plan, List<ProductionPlanProduct> Products)>();

            string query = @"
        SELECT
            p.ProductionPlanID, p.IssueDate, p.TargetDate, p.Priority,
            pp.ProductID, pp.MinQuantity, pp.MaxQuantity,
            p.CreatedAt, p.CreatedBy, p.ModifiedAt, p.ModifiedBy
        FROM ProductionPlan p
        LEFT JOIN ProductionPlanProduct pp ON p.ProductionPlanID = pp.ProductionPlanID
        ORDER BY p.ProductionPlanID";

            using (var conn = _context.CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = new SqlCommand(query, conn))
                {
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            int planId = reader.GetInt32(reader.GetOrdinal("ProductionPlanID"));

                            if (!planMap.TryGetValue(planId, out var planTuple))
                            {
                                var plan = new ProductionPlan
                                {
                                    ProductionPlanId = planId,
                                    IssueDate = reader.IsDBNull(reader.GetOrdinal("IssueDate")) ? null : reader.GetDateTime(reader.GetOrdinal("IssueDate")),
                                    TargetDate = reader.IsDBNull(reader.GetOrdinal("TargetDate")) ? null : reader.GetDateTime(reader.GetOrdinal("TargetDate")),
                                    Priority = reader.IsDBNull(reader.GetOrdinal("Priority")) ? null : reader.GetInt32(reader.GetOrdinal("Priority")),
                                    CreatedAt = reader.IsDBNull(reader.GetOrdinal("CreatedAt")) ? null : reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                                    CreatedBy = reader.IsDBNull(reader.GetOrdinal("CreatedBy")) ? null : reader.GetInt32(reader.GetOrdinal("CreatedBy")),
                                    ModifiedAt = reader.IsDBNull(reader.GetOrdinal("ModifiedAt")) ? null : reader.GetDateTime(reader.GetOrdinal("ModifiedAt")),
                                    ModifiedBy = reader.IsDBNull(reader.GetOrdinal("ModifiedBy")) ? null : reader.GetInt32(reader.GetOrdinal("ModifiedBy"))
                                };

                                planTuple = (Plan: plan, Products: new List<ProductionPlanProduct>());
                                planMap[planId] = planTuple;
                            }

                            if (!reader.IsDBNull(reader.GetOrdinal("ProductID")))
                            {
                                var product = new ProductionPlanProduct
                                {
                                    ProductionPlanID = planId,
                                    ProductID = reader.GetInt32(reader.GetOrdinal("ProductID")),
                                    MinQuantity = reader.IsDBNull(reader.GetOrdinal("MinQuantity")) ? null : reader.GetInt32(reader.GetOrdinal("MinQuantity")),
                                    MaxQuantity = reader.IsDBNull(reader.GetOrdinal("MaxQuantity")) ? null : reader.GetInt32(reader.GetOrdinal("MaxQuantity"))
                                };

                                planTuple.Products.Add(product);
                            }
                        }
                    }
                }
            }

            return planMap.Values.ToList();
        }

        public async Task<bool> RemoveWithProductsAsync(int planId)
        {
            using (var conn = _context.CreateConnection())
            {
                await conn.OpenAsync();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        string deleteProductsQuery = "DELETE FROM ProductionPlanProduct WHERE ProductionPlanID = @PlanId";
                        using (var cmd = new SqlCommand(deleteProductsQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@PlanId", planId);
                            await cmd.ExecuteNonQueryAsync();
                        }

                        string deletePlanQuery = "DELETE FROM ProductionPlan WHERE ProductionPlanID = @PlanId";
                        using (var cmd = new SqlCommand(deletePlanQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@PlanId", planId);
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

        public async Task<bool> UpdateWithProductsAsync(ProductionPlan plan, List<ProductionPlanProduct> products)
        {
            using (var conn = _context.CreateConnection())
            {
                await conn.OpenAsync();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        string updatePlanQuery = @"UPDATE ProductionPlan
                                          SET IssueDate = @IssueDate,
                                              TargetDate = @TargetDate,
                                              Priority = @Priority,
                                              ModifiedAt = GETDATE(), ModifiedBy = @ModifiedBy
                                          WHERE ProductionPlanID = @ProductionPlanID";

                        using (var cmd = new SqlCommand(updatePlanQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@ProductionPlanID", plan.ProductionPlanId);
                            cmd.Parameters.AddWithValue("@IssueDate", (object?)plan.IssueDate ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@TargetDate", (object?)plan.TargetDate ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@Priority", (object?)plan.Priority ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@ModifiedBy", _currentUserService.ActorId);

                            await cmd.ExecuteNonQueryAsync();
                        }

                        string deleteProductsQuery = "DELETE FROM ProductionPlanProduct WHERE ProductionPlanID = @PlanId";
                        using (var cmdDel = new SqlCommand(deleteProductsQuery, conn, transaction))
                        {
                            cmdDel.Parameters.AddWithValue("@PlanId", plan.ProductionPlanId);
                            await cmdDel.ExecuteNonQueryAsync();
                        }

                        if (products != null && products.Count > 0)
                        {
                            string insertProductQuery = @"INSERT INTO ProductionPlanProduct (ProductionPlanID, ProductID, MinQuantity, MaxQuantity) 
                                                 VALUES (@ProductionPlanID, @ProductID, @MinQuantity, @MaxQuantity)";

                            foreach (var item in products)
                            {
                                using (var cmdIns = new SqlCommand(insertProductQuery, conn, transaction))
                                {
                                    cmdIns.Parameters.AddWithValue("@ProductionPlanID", plan.ProductionPlanId);
                                    cmdIns.Parameters.AddWithValue("@ProductID", item.ProductID);
                                    cmdIns.Parameters.AddWithValue("@MinQuantity", (object?)item.MinQuantity ?? DBNull.Value);
                                    cmdIns.Parameters.AddWithValue("@MaxQuantity", (object?)item.MaxQuantity ?? DBNull.Value);

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

    }
}
