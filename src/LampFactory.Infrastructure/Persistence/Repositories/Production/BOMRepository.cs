using LampFactory.Infrastructure.Persistence;
using LampFactory.Application.Abstractions.Persistence.Production.BillOfMaterials;
using LampFactory.Domain.Production;
using LampFactory.Infrastructure.Security;
using Microsoft.Data.SqlClient;

namespace LampFactory.Infrastructure.Persistence.Repositories.Production
{
    public class BOMRepository : IBOMRepository
{
        private readonly ApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public BOMRepository(ApplicationDbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<int> CreateWithComponentsAsync(BOM bom, List<BOMComponent> components)
        {
            using (var conn = _context.CreateConnection())
            {
                await conn.OpenAsync();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        string bomQuery = @"INSERT INTO BOM (ProductID, CreatedBy)
                                          VALUES (@ProductID, @CreatedBy);
                                          SELECT SCOPE_IDENTITY();";

                        int newBomId;
                        using (var cmd = new SqlCommand(bomQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@ProductID", bom.ProductID);
                            cmd.Parameters.AddWithValue("@CreatedBy", _currentUserService.ActorId);

                            var result = await cmd.ExecuteScalarAsync();
                            newBomId = Convert.ToInt32(result);
                        }

                        if (components != null && components.Count > 0)
                        {
                            string componentQuery = @"INSERT INTO BOMComponent (BOM_ID, ProductID, ConsumptionAmount) 
                                                     VALUES (@BOM_ID, @ProductID, @ConsumptionAmount)";

                            foreach (var item in components)
                            {
                                using (var cmdItem = new SqlCommand(componentQuery, conn, transaction))
                                {
                                    cmdItem.Parameters.AddWithValue("@BOM_ID", newBomId);
                                    cmdItem.Parameters.AddWithValue("@ProductID", item.ProductID);
                                    cmdItem.Parameters.AddWithValue("@ConsumptionAmount", (object?)item.ConsumptionAmount ?? DBNull.Value);

                                    await cmdItem.ExecuteNonQueryAsync();
                                }
                            }
                        }

                        await transaction.CommitAsync();
                        return newBomId;
                    }
                    catch
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                }
            }
        }

        public async Task<(BOM? Bom, List<BOMComponent> Components)> GetByIdWithComponentsAsync(int bomId)
        {
            BOM? bom = null;
            var components = new List<BOMComponent>();

            using (var conn = _context.CreateConnection())
            {
                await conn.OpenAsync();

                string query = @"
            SELECT
                b.BOM_ID,
                b.ProductID AS BomProductID,
                c.ProductID AS ComponentProductID,
                c.ConsumptionAmount,
                b.CreatedAt, b.CreatedBy, b.ModifiedAt, b.ModifiedBy
            FROM BOM b
            LEFT JOIN BOMComponent c ON b.BOM_ID = c.BOM_ID
            WHERE b.BOM_ID = @Id";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", bomId);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            if (bom == null)
                            {
                                bom = new BOM
                                {
                                    BOM_ID = reader.GetInt32(0),
                                    ProductID = reader.GetInt32(1),
                                    CreatedAt = reader.IsDBNull(4) ? null : reader.GetDateTime(4),
                                    CreatedBy = reader.IsDBNull(5) ? null : reader.GetInt32(5),
                                    ModifiedAt = reader.IsDBNull(6) ? null : reader.GetDateTime(6),
                                    ModifiedBy = reader.IsDBNull(7) ? null : reader.GetInt32(7)
                                };
                            }

                            if (!reader.IsDBNull(2))
                            {
                                components.Add(new BOMComponent
                                {
                                    BOM_ID = reader.GetInt32(0),
                                    ProductID = reader.GetInt32(2),
                                    ConsumptionAmount = reader.IsDBNull(3) ? null : reader.GetDouble(3)
                                });
                            }
                        }
                    }
                }
            }

            return (bom, components);
        }

   
        public async Task<List<(BOM Bom, List<BOMComponent> Components)>> GetAllWithComponentsAsync()
        {
            var dictionary = new Dictionary<int, (BOM Bom, List<BOMComponent> Components)>();

            using (var conn = _context.CreateConnection())
            {
                await conn.OpenAsync();

                string query = @"
            SELECT
                b.BOM_ID,
                b.ProductID AS BomProductID,
                c.ProductID AS ComponentProductID,
                c.ConsumptionAmount,
                b.CreatedAt, b.CreatedBy, b.ModifiedAt, b.ModifiedBy
            FROM BOM b
            LEFT JOIN BOMComponent c ON b.BOM_ID = c.BOM_ID";

                using (var cmd = new SqlCommand(query, conn))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        int currentBomId = reader.GetInt32(0);

                        if (!dictionary.ContainsKey(currentBomId))
                        {
                            var newBom = new BOM
                            {
                                BOM_ID = currentBomId,
                                ProductID = reader.GetInt32(1),
                                CreatedAt = reader.IsDBNull(4) ? null : reader.GetDateTime(4),
                                CreatedBy = reader.IsDBNull(5) ? null : reader.GetInt32(5),
                                ModifiedAt = reader.IsDBNull(6) ? null : reader.GetDateTime(6),
                                ModifiedBy = reader.IsDBNull(7) ? null : reader.GetInt32(7)
                            };
                            dictionary[currentBomId] = (newBom, new List<BOMComponent>());
                        }

                        if (!reader.IsDBNull(2))
                        {
                            var component = new BOMComponent
                            {
                                BOM_ID = currentBomId,
                                ProductID = reader.GetInt32(2),
                                ConsumptionAmount = reader.IsDBNull(3) ? null : reader.GetDouble(3)
                            };
                            dictionary[currentBomId].Components.Add(component);
                        }
                    }
                }
            }

            return dictionary.Values.ToList();
        }

        public async Task<bool> RemoveWithComponentsAsync(int bomId)
        {
            using (var conn = _context.CreateConnection())
            {
                await conn.OpenAsync();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        string deleteComponentsQuery = "DELETE FROM BOMComponent WHERE BOM_ID = @BomId";
                        using (var cmd = new SqlCommand(deleteComponentsQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@BomId", bomId);
                            await cmd.ExecuteNonQueryAsync();
                        }

                        string deleteBomQuery = "DELETE FROM BOM WHERE BOM_ID = @BomId";
                        using (var cmd = new SqlCommand(deleteBomQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@BomId", bomId);
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

        public async Task<bool> UpdateWithComponentsAsync(BOM bom, List<BOMComponent> components)
        {
            using (var conn = _context.CreateConnection())
            {
                await conn.OpenAsync();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        string updateBomQuery = @"UPDATE BOM
                                          SET ProductID = @ProductID,
                                              ModifiedAt = GETDATE(), ModifiedBy = @ModifiedBy
                                          WHERE BOM_ID = @BOM_ID";

                        using (var cmd = new SqlCommand(updateBomQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@BOM_ID", bom.BOM_ID);
                            cmd.Parameters.AddWithValue("@ProductID", bom.ProductID);
                            cmd.Parameters.AddWithValue("@ModifiedBy", _currentUserService.ActorId);

                            await cmd.ExecuteNonQueryAsync();
                        }

                        string deleteComponentsQuery = "DELETE FROM BOMComponent WHERE BOM_ID = @BomId";
                        using (var cmdDel = new SqlCommand(deleteComponentsQuery, conn, transaction))
                        {
                            cmdDel.Parameters.AddWithValue("@BomId", bom.BOM_ID);
                            await cmdDel.ExecuteNonQueryAsync();
                        }

                        if (components != null && components.Count > 0)
                        {
                            string insertComponentQuery = @"INSERT INTO BOMComponent (BOM_ID, ProductID, ConsumptionAmount) 
                                                 VALUES (@BOM_ID, @ProductID, @ConsumptionAmount)";

                            foreach (var item in components)
                            {
                                using (var cmdIns = new SqlCommand(insertComponentQuery, conn, transaction))
                                {
                                    cmdIns.Parameters.AddWithValue("@BOM_ID", bom.BOM_ID);
                                    cmdIns.Parameters.AddWithValue("@ProductID", item.ProductID);
                                    cmdIns.Parameters.AddWithValue("@ConsumptionAmount", (object?)item.ConsumptionAmount ?? DBNull.Value);

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
