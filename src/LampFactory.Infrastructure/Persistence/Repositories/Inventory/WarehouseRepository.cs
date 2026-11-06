using LampFactory.Infrastructure.Persistence;
using LampFactory.Application.Abstractions.Persistence.Inventory.Warehouses;
using LampFactory.Domain.Inventory;
using LampFactory.Infrastructure.Security;
using Microsoft.Data.SqlClient;

namespace LampFactory.Infrastructure.Persistence.Repositories.Inventory
{
    public class WarehouseRepository : IWarehouseRepository
{
        private readonly ApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public WarehouseRepository(ApplicationDbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<int> CreateWithProductsAsync(Warehouse warehouse, List<WarehouseProduct> products)
        {
            using (var conn = _context.CreateConnection())
            {
                await conn.OpenAsync();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        string docQuery = @"INSERT INTO Warehouse
                                           (WarehouseType, WarehouseName, WarehouseAddress, CreatedBy)
                                           VALUES
                                           (@WarehouseType, @Name, @Address, @CreatedBy);
                                           SELECT SCOPE_IDENTITY();";

                        int newDocId;
                        using (var cmd = new SqlCommand(docQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@WarehouseType", warehouse.WarehouseType.ToString());
                            cmd.Parameters.AddWithValue("@Name", (object?)warehouse.Name ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@Address", (object?)warehouse.Address ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@CreatedBy", _currentUserService.ActorId);

                            var result = await cmd.ExecuteScalarAsync();
                            newDocId = Convert.ToInt32(result);
                        }

                        if (products != null && products.Count > 0)
                        {
                            string opQuery = @"INSERT INTO WarehouseProduct 
                                             (WarehouseID, ProductID, Quantity) 
                                             VALUES 
                                             (@WarehouseID, @ProductID, @Quantity)";

                            foreach (var item in products)
                            {
                                using (var cmdOp = new SqlCommand(opQuery, conn, transaction))
                                {
                                    cmdOp.Parameters.AddWithValue("@WarehouseID", newDocId);
                                    cmdOp.Parameters.AddWithValue("@ProductID", item.ProductID);
                                    cmdOp.Parameters.AddWithValue("@Quantity", (object?)item.Quantity ?? DBNull.Value);

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

        public async Task<List<(Warehouse Warehouse, List<WarehouseProduct> Stock)>> GetAllWithStockAsync()
        {
            var dictionary = new Dictionary<int, (Warehouse Warehouse, List<WarehouseProduct> Stock)>();

            using (var conn = _context.CreateConnection())
            {
                await conn.OpenAsync();

                string query = @"
            SELECT
                w.WarehouseID, w.WarehouseType, w.WarehouseName, w.WarehouseAddress,
                wp.ProductID, wp.Quantity,
                w.CreatedAt, w.CreatedBy, w.ModifiedAt, w.ModifiedBy
            FROM Warehouse w
            LEFT JOIN WarehouseProduct wp ON w.WarehouseID = wp.WarehouseID";

                using (var cmd = new SqlCommand(query, conn))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        int currentWarehouseId = reader.GetInt32(0);

                        if (!dictionary.ContainsKey(currentWarehouseId))
                        {
                            var newWarehouse = new Warehouse
                            {
                                WarehouseID = currentWarehouseId,
                                WarehouseType = Enum.Parse<WarehouseType>(reader.GetString(1)),
                                Name = reader.IsDBNull(2) ? null : reader.GetString(2),
                                Address = reader.IsDBNull(3) ? null : reader.GetString(3),
                                CreatedAt = reader.IsDBNull(6) ? null : reader.GetDateTime(6),
                                CreatedBy = reader.IsDBNull(7) ? null : reader.GetInt32(7),
                                ModifiedAt = reader.IsDBNull(8) ? null : reader.GetDateTime(8),
                                ModifiedBy = reader.IsDBNull(9) ? null : reader.GetInt32(9)
                            };

                            dictionary[currentWarehouseId] = (newWarehouse, new List<WarehouseProduct>());
                        }

                        if (!reader.IsDBNull(4))
                        {
                            var stockItem = new WarehouseProduct
                            {
                                WarehouseID = currentWarehouseId,
                                ProductID = reader.GetInt32(4),
                                Quantity = reader.GetInt32(5)
                            };

                            dictionary[currentWarehouseId].Stock.Add(stockItem);
                        }
                    }
                }
            }
            return dictionary.Values.ToList();
        }

        public async Task<(Warehouse? Warehouse, List<WarehouseProduct> Stock)> GetByIdWithStockAsync(int warehouseId)
        {
            Warehouse? warehouse = null;
            var stock = new List<WarehouseProduct>();

            using (var conn = _context.CreateConnection())
            {
                await conn.OpenAsync();

                string query = @"
            SELECT
                w.WarehouseID, w.WarehouseType, w.WarehouseName, w.WarehouseAddress,
                wp.ProductID, wp.Quantity,
                w.CreatedAt, w.CreatedBy, w.ModifiedAt, w.ModifiedBy
            FROM Warehouse w
            LEFT JOIN WarehouseProduct wp ON w.WarehouseID = wp.WarehouseID
            WHERE w.WarehouseID = @Id";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", warehouseId);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            if (warehouse == null)
                            {
                                warehouse = new Warehouse
                                {
                                    WarehouseID = reader.GetInt32(0),
                                    WarehouseType = Enum.Parse<WarehouseType>(reader.GetString(1)),
                                    Name = reader.IsDBNull(2) ? null : reader.GetString(2),
                                    Address = reader.IsDBNull(3) ? null : reader.GetString(3),
                                    CreatedAt = reader.IsDBNull(6) ? null : reader.GetDateTime(6),
                                    CreatedBy = reader.IsDBNull(7) ? null : reader.GetInt32(7),
                                    ModifiedAt = reader.IsDBNull(8) ? null : reader.GetDateTime(8),
                                    ModifiedBy = reader.IsDBNull(9) ? null : reader.GetInt32(9)
                                };
                            }

                            if (!reader.IsDBNull(4))
                            {
                                stock.Add(new WarehouseProduct
                                {
                                    WarehouseID = warehouse.WarehouseID,
                                    ProductID = reader.GetInt32(4),
                                    Quantity = reader.GetInt32(5)
                                });
                            }
                        }
                    }
                }
            }
            return (warehouse, stock);
        }

        public async Task<bool> RemoveWithProductsAsync(int warehouseId)
        {
            using (var conn = _context.CreateConnection())
            {
                await conn.OpenAsync();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        string deleteProductsQuery = "DELETE FROM WarehouseProduct WHERE WarehouseID = @Id";
                        using (var cmd = new SqlCommand(deleteProductsQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@Id", warehouseId);
                            await cmd.ExecuteNonQueryAsync();
                        }

                        string deleteWarehouseQuery = "DELETE FROM Warehouse WHERE WarehouseID = @Id";
                        using (var cmd = new SqlCommand(deleteWarehouseQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@Id", warehouseId);
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

        public async Task<bool> UpdateWithProductsAsync(Warehouse warehouse, List<WarehouseProduct> products)
        {
            using (var conn = _context.CreateConnection())
            {
                await conn.OpenAsync();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        string updateDocQuery = @"UPDATE Warehouse
                                          SET WarehouseType = @WarehouseType,
                                              WarehouseName = @Name,
                                              WarehouseAddress = @Address,
                                              ModifiedAt = GETDATE(), ModifiedBy = @ModifiedBy
                                          WHERE WarehouseID = @WarehouseID";

                        using (var cmd = new SqlCommand(updateDocQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@WarehouseID", warehouse.WarehouseID);
                            cmd.Parameters.AddWithValue("@WarehouseType", warehouse.WarehouseType.ToString());
                            cmd.Parameters.AddWithValue("@Name", (object?)warehouse.Name ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@Address", (object?)warehouse.Address ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@ModifiedBy", _currentUserService.ActorId);

                            await cmd.ExecuteNonQueryAsync();
                        }

                        string deleteOpsQuery = "DELETE FROM WarehouseProduct WHERE WarehouseID = @WarehouseID";
                        using (var cmdDel = new SqlCommand(deleteOpsQuery, conn, transaction))
                        {
                            cmdDel.Parameters.AddWithValue("@WarehouseID", warehouse.WarehouseID);
                            await cmdDel.ExecuteNonQueryAsync();
                        }

                        if (products != null && products.Count > 0)
                        {
                            string insertOpQuery = @"INSERT INTO WarehouseProduct 
                                             (WarehouseID, ProductID, Quantity) 
                                             VALUES 
                                             (@WarehouseID, @ProductID, @Quantity)";

                            foreach (var item in products)
                            {
                                using (var cmdIns = new SqlCommand(insertOpQuery, conn, transaction))
                                {
                                    cmdIns.Parameters.AddWithValue("@WarehouseID", warehouse.WarehouseID);
                                    cmdIns.Parameters.AddWithValue("@ProductID", item.ProductID);
                                    cmdIns.Parameters.AddWithValue("@Quantity", (object?)item.Quantity ?? DBNull.Value);

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
