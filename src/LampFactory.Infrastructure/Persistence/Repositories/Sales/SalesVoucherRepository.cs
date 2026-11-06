using LampFactory.Infrastructure.Persistence;
using LampFactory.Application.Abstractions.Persistence.Sales;
using LampFactory.Domain.Inventory;
using LampFactory.Domain.Sales;
using LampFactory.Infrastructure.Security;
using Microsoft.Data.SqlClient;

namespace LampFactory.Infrastructure.Persistence.Repositories.Sales
{
    public class SalesVoucherRepository : ISalesVoucherRepository
{
        private readonly ApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public SalesVoucherRepository(ApplicationDbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<int> CreateWithProductsAsync(SalesVoucher voucher, List<SalesVoucherProduct> products)
        {
            using (var conn = _context.CreateConnection())
            {
                await conn.OpenAsync();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        string query = @"INSERT INTO SalesVoucher (WarehouseID, CreatedBy)
                                        VALUES (@WarehouseID, @CreatedBy);
                                        SELECT SCOPE_IDENTITY();";

                        int voucherId;
                        using (var cmd = new SqlCommand(query, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@WarehouseID", voucher.WarehouseID);
                            cmd.Parameters.AddWithValue("@CreatedBy", _currentUserService.ActorId);

                            var result = await cmd.ExecuteScalarAsync();
                            voucherId = Convert.ToInt32(result);
                        }

                        if (products != null && products.Count > 0)
                        {
                            string itemQuery = @"INSERT INTO SalesVoucherProduct (SalesVoucherID, ProductID, SalesQuantity) 
                                               VALUES (@SalesVoucherID, @ProductID, @SalesQuantity)";

                            foreach (var item in products)
                            {
                                int qty = item.SalesQuantity ?? 0;
                                if (qty > 0)
                                {
                                    await ValidateStockAsync(conn, transaction, voucher.WarehouseID, item.ProductID, qty);
                                }

                                using (var cmdItem = new SqlCommand(itemQuery, conn, transaction))
                                {
                                    cmdItem.Parameters.AddWithValue("@SalesVoucherID", voucherId);
                                    cmdItem.Parameters.AddWithValue("@ProductID", item.ProductID);
                                    cmdItem.Parameters.AddWithValue("@SalesQuantity", (object?)item.SalesQuantity ?? DBNull.Value);

                                    await cmdItem.ExecuteNonQueryAsync();
                                }

                                if (qty > 0)
                                {
                                    await DecreaseStockAsync(conn, transaction, voucher.WarehouseID, item.ProductID, qty);
                                }
                            }
                        }

                        await transaction.CommitAsync();
                        return voucherId;
                    }
                    catch (InsufficientStockException)
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                    catch
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                }
            }
        }

        public async Task<(SalesVoucher? Voucher, List<SalesVoucherProduct> Products)> GetByIdWithProductsAsync(int voucherId)
        {
            SalesVoucher? voucher = null;
            var products = new List<SalesVoucherProduct>();
            using (var conn = _context.CreateConnection())
            {
                await conn.OpenAsync();

                string query = @"
            SELECT
                v.SalesVoucherID,
                v.WarehouseID,
                p.ProductID,
                p.SalesQuantity,
                v.CreatedAt, v.CreatedBy, v.ModifiedAt, v.ModifiedBy
            FROM SalesVoucher v
            LEFT JOIN SalesVoucherProduct p ON v.SalesVoucherID = p.SalesVoucherID
            WHERE v.SalesVoucherID = @Id";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", voucherId);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            if (voucher == null)
                            {
                                voucher = new SalesVoucher
                                {
                                    SalesVoucherID = reader.GetInt32(0),
                                    WarehouseID = reader.GetInt32(1),
                                    CreatedAt = reader.IsDBNull(4) ? null : reader.GetDateTime(4),
                                    CreatedBy = reader.IsDBNull(5) ? null : reader.GetInt32(5),
                                    ModifiedAt = reader.IsDBNull(6) ? null : reader.GetDateTime(6),
                                    ModifiedBy = reader.IsDBNull(7) ? null : reader.GetInt32(7)
                                };
                            }

                            if (!reader.IsDBNull(2))
                            {
                                products.Add(new SalesVoucherProduct
                                {
                                    SalesVoucherID = reader.GetInt32(0),
                                    ProductID = reader.GetInt32(2),
                                    SalesQuantity = reader.IsDBNull(3) ? null : reader.GetInt32(3)
                                });
                            }
                        }
                    }
                }
            }

            return (voucher, products);
        }

        public async Task<List<(SalesVoucher Voucher, List<SalesVoucherProduct> Products)>> GetAllWithProductsAsync()
        {
            var dictionary = new Dictionary<int, (SalesVoucher Voucher, List<SalesVoucherProduct> Products)>();
            using (var conn = _context.CreateConnection())
            {
                await conn.OpenAsync();

                string query = @"
            SELECT
                v.SalesVoucherID,
                v.WarehouseID,
                p.ProductID,
                p.SalesQuantity,
                v.CreatedAt, v.CreatedBy, v.ModifiedAt, v.ModifiedBy
            FROM SalesVoucher v
            LEFT JOIN SalesVoucherProduct p ON v.SalesVoucherID = p.SalesVoucherID";

                using (var cmd = new SqlCommand(query, conn))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        int currentVoucherId = reader.GetInt32(0);
                        if (!dictionary.ContainsKey(currentVoucherId))
                        {
                            var newVoucher = new SalesVoucher
                            {
                                SalesVoucherID = currentVoucherId,
                                WarehouseID = reader.GetInt32(1),
                                CreatedAt = reader.IsDBNull(4) ? null : reader.GetDateTime(4),
                                CreatedBy = reader.IsDBNull(5) ? null : reader.GetInt32(5),
                                ModifiedAt = reader.IsDBNull(6) ? null : reader.GetDateTime(6),
                                ModifiedBy = reader.IsDBNull(7) ? null : reader.GetInt32(7)
                            };

                            dictionary[currentVoucherId] = (newVoucher, new List<SalesVoucherProduct>());
                        }

                        if (!reader.IsDBNull(2))
                        {
                            var product = new SalesVoucherProduct
                            {
                                SalesVoucherID = currentVoucherId,
                                ProductID = reader.GetInt32(2),
                                SalesQuantity = reader.IsDBNull(3) ? null : reader.GetInt32(3)
                            };

                            dictionary[currentVoucherId].Products.Add(product);
                        }
                    }
                }
            }

            return dictionary.Values.ToList();
        }

        public async Task<bool> RemoveWithProductsAsync(int voucherId)
        {
            using (var conn = _context.CreateConnection())
            {
                await conn.OpenAsync();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        var existingProducts = await GetExistingVoucherProductsAsync(conn, transaction, voucherId);

                        int warehouseId = await GetWarehouseIdAsync(conn, transaction, voucherId);

                        string deleteItemsQuery = "DELETE FROM SalesVoucherProduct WHERE SalesVoucherID = @VoucherId";
                        using (var cmd = new SqlCommand(deleteItemsQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@VoucherId", voucherId);
                            await cmd.ExecuteNonQueryAsync();
                        }

                        string deleteVoucherQuery = "DELETE FROM SalesVoucher WHERE SalesVoucherID = @VoucherId";
                        using (var cmd = new SqlCommand(deleteVoucherQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@VoucherId", voucherId);
                            await cmd.ExecuteNonQueryAsync();
                        }

                        foreach (var (productId, qty) in existingProducts)
                        {
                            if (qty > 0)
                            {
                                await IncreaseStockAsync(conn, transaction, warehouseId, productId, qty);
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

        public async Task<bool> UpdateWithProductsAsync(SalesVoucher voucher, List<SalesVoucherProduct> products)
        {
            using (var conn = _context.CreateConnection())
            {
                await conn.OpenAsync();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        var existingProducts = await GetExistingVoucherProductsAsync(conn, transaction, voucher.SalesVoucherID);
                        int oldWarehouseId = await GetWarehouseIdAsync(conn, transaction, voucher.SalesVoucherID);

                        string updateVoucherQuery = @"UPDATE SalesVoucher
                                              SET WarehouseID = @WarehouseID,
                                                  ModifiedAt = GETDATE(), ModifiedBy = @ModifiedBy
                                              WHERE SalesVoucherID = @SalesVoucherID";

                        using (var cmd = new SqlCommand(updateVoucherQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@SalesVoucherID", voucher.SalesVoucherID);
                            cmd.Parameters.AddWithValue("@WarehouseID", voucher.WarehouseID);
                            cmd.Parameters.AddWithValue("@ModifiedBy", _currentUserService.ActorId);

                            await cmd.ExecuteNonQueryAsync();
                        }

                        foreach (var (productId, qty) in existingProducts)
                        {
                            if (qty > 0)
                            {
                                await IncreaseStockAsync(conn, transaction, oldWarehouseId, productId, qty);
                            }
                        }

                        string deleteItemsQuery = "DELETE FROM SalesVoucherProduct WHERE SalesVoucherID = @SalesVoucherID";
                        using (var cmdDelete = new SqlCommand(deleteItemsQuery, conn, transaction))
                        {
                            cmdDelete.Parameters.AddWithValue("@SalesVoucherID", voucher.SalesVoucherID);
                            await cmdDelete.ExecuteNonQueryAsync();
                        }

                        if (products != null && products.Count > 0)
                        {
                            string insertItemQuery = @"INSERT INTO SalesVoucherProduct (SalesVoucherID, ProductID, SalesQuantity) 
                                               VALUES (@SalesVoucherID, @ProductID, @SalesQuantity)";

                            foreach (var item in products)
                            {
                                int qty = item.SalesQuantity ?? 0;
                                if (qty > 0)
                                {
                                    await ValidateStockAsync(conn, transaction, voucher.WarehouseID, item.ProductID, qty);
                                }

                                using (var cmdItem = new SqlCommand(insertItemQuery, conn, transaction))
                                {
                                    cmdItem.Parameters.AddWithValue("@SalesVoucherID", voucher.SalesVoucherID);
                                    cmdItem.Parameters.AddWithValue("@ProductID", item.ProductID);
                                    cmdItem.Parameters.AddWithValue("@SalesQuantity", (object?)item.SalesQuantity ?? DBNull.Value);

                                    await cmdItem.ExecuteNonQueryAsync();
                                }

                                if (qty > 0)
                                {
                                    await DecreaseStockAsync(conn, transaction, voucher.WarehouseID, item.ProductID, qty);
                                }
                            }
                        }

                        await transaction.CommitAsync();
                        return true;
                    }
                    catch (InsufficientStockException)
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                    catch
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                }
            }
        }

        private async Task<int> GetStockAsync(SqlConnection conn, SqlTransaction transaction, int warehouseId, int productId)
        {
            string query = "SELECT COALESCE(Quantity, 0) FROM WarehouseProduct WHERE WarehouseID = @WarehouseID AND ProductID = @ProductID";
            using var cmd = new SqlCommand(query, conn, transaction);
            cmd.Parameters.AddWithValue("@WarehouseID", warehouseId);
            cmd.Parameters.AddWithValue("@ProductID", productId);
            var result = await cmd.ExecuteScalarAsync();
            return result != null && result != DBNull.Value ? Convert.ToInt32(result) : 0;
        }

        private async Task ValidateStockAsync(SqlConnection conn, SqlTransaction transaction, int warehouseId, int productId, int requestedQuantity)
        {
            if (requestedQuantity <= 0) return;
            int available = await GetStockAsync(conn, transaction, warehouseId, productId);
            if (available < requestedQuantity)
            {
                throw new InsufficientStockException($"Insufficient stock for ProductID {productId} in WarehouseID {warehouseId}. Requested: {requestedQuantity}, Available: {available}.");
            }
        }

        private async Task DecreaseStockAsync(SqlConnection conn, SqlTransaction transaction, int warehouseId, int productId, int quantity)
        {
            string query = @"
                IF EXISTS (SELECT 1 FROM WarehouseProduct WHERE WarehouseID = @WarehouseID AND ProductID = @ProductID)
                    UPDATE WarehouseProduct SET Quantity = Quantity - @Quantity WHERE WarehouseID = @WarehouseID AND ProductID = @ProductID
                ELSE
                    INSERT INTO WarehouseProduct (WarehouseID, ProductID, Quantity) VALUES (@WarehouseID, @ProductID, -@Quantity)";
            using var cmd = new SqlCommand(query, conn, transaction);
            cmd.Parameters.AddWithValue("@WarehouseID", warehouseId);
            cmd.Parameters.AddWithValue("@ProductID", productId);
            cmd.Parameters.AddWithValue("@Quantity", quantity);
            await cmd.ExecuteNonQueryAsync();
        }

        private async Task IncreaseStockAsync(SqlConnection conn, SqlTransaction transaction, int warehouseId, int productId, int quantity)
        {
            string query = @"
                IF EXISTS (SELECT 1 FROM WarehouseProduct WHERE WarehouseID = @WarehouseID AND ProductID = @ProductID)
                    UPDATE WarehouseProduct SET Quantity = Quantity + @Quantity WHERE WarehouseID = @WarehouseID AND ProductID = @ProductID
                ELSE
                    INSERT INTO WarehouseProduct (WarehouseID, ProductID, Quantity) VALUES (@WarehouseID, @ProductID, @Quantity)";
            using var cmd = new SqlCommand(query, conn, transaction);
            cmd.Parameters.AddWithValue("@WarehouseID", warehouseId);
            cmd.Parameters.AddWithValue("@ProductID", productId);
            cmd.Parameters.AddWithValue("@Quantity", quantity);
            await cmd.ExecuteNonQueryAsync();
        }

        private async Task<List<(int ProductID, int Quantity)>> GetExistingVoucherProductsAsync(SqlConnection conn, SqlTransaction transaction, int voucherId)
        {
            var list = new List<(int, int)>();
            string query = "SELECT ProductID, SalesQuantity FROM SalesVoucherProduct WHERE SalesVoucherID = @VoucherId";
            using var cmd = new SqlCommand(query, conn, transaction);
            cmd.Parameters.AddWithValue("@VoucherId", voucherId);
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                int productId = reader.GetInt32(0);
                int quantity = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);
                list.Add((productId, quantity));
            }
            return list;
        }

        private async Task<int> GetWarehouseIdAsync(SqlConnection conn, SqlTransaction transaction, int voucherId)
        {
            string query = "SELECT WarehouseID FROM SalesVoucher WHERE SalesVoucherID = @VoucherId";
            using var cmd = new SqlCommand(query, conn, transaction);
            cmd.Parameters.AddWithValue("@VoucherId", voucherId);
            var result = await cmd.ExecuteScalarAsync();
            return result != null && result != DBNull.Value ? Convert.ToInt32(result) : 0;
        }
    }
}
