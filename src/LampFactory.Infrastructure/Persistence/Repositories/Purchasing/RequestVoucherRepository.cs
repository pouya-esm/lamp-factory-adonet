using LampFactory.Infrastructure.Persistence;
using LampFactory.Application.Abstractions.Persistence.Purchasing.RequestVouchers;
using LampFactory.Domain.Inventory;
using LampFactory.Domain.Purchasing;
using LampFactory.Infrastructure.Security;
using Microsoft.Data.SqlClient;

namespace LampFactory.Infrastructure.Persistence.Repositories.Purchasing
{
    public class RequestVoucherRepository : IRequestVoucherRepository
{
        private readonly ApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public RequestVoucherRepository(ApplicationDbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<int> CreateWithProductsAsync(RequestVoucher voucher, List<RequestVoucherProduct> products)
        {
            using (var conn = _context.CreateConnection())
            {
                await conn.OpenAsync();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        string query = @"INSERT INTO RequestVoucher (RequestDate, WarehouseID, ProductionDocumentID, CreatedBy)
                                        VALUES (@RequestDate, @WarehouseID, @ProductionDocumentID, @CreatedBy);
                                        SELECT SCOPE_IDENTITY();";

                        int voucherId;
                        using (var cmd = new SqlCommand(query, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@RequestDate", (object?)voucher.RequestDate ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@WarehouseID", voucher.WarehouseID);
                            cmd.Parameters.AddWithValue("@ProductionDocumentID", voucher.ProductionDocumentID);
                            cmd.Parameters.AddWithValue("@CreatedBy", _currentUserService.ActorId);

                            var result = await cmd.ExecuteScalarAsync();
                            voucherId = Convert.ToInt32(result);
                        }

                        if (products != null && products.Count > 0)
                        {
                            string itemQuery = @"INSERT INTO RequestVoucherProduct (RequestVoucherID, ProductID, RequestedQuantity) 
                                               VALUES (@RequestVoucherID, @ProductID, @RequestedQuantity)";

                            foreach (var item in products)
                            {
                                int qty = item.RequestedQuantity ?? 0;
                                if (qty > 0)
                                {
                                    await ValidateStockAsync(conn, transaction, voucher.WarehouseID, item.ProductID, qty);
                                }

                                using (var cmdItem = new SqlCommand(itemQuery, conn, transaction))
                                {
                                    cmdItem.Parameters.AddWithValue("@RequestVoucherID", voucherId);
                                    cmdItem.Parameters.AddWithValue("@ProductID", item.ProductID);
                                    cmdItem.Parameters.AddWithValue("@RequestedQuantity", (object?)item.RequestedQuantity ?? DBNull.Value);

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

        public async Task<(RequestVoucher? Voucher, List<RequestVoucherProduct> Products)> GetByIdWithProductsAsync(int voucherId)
        {
            RequestVoucher? voucher = null;
            var products = new List<RequestVoucherProduct>();
            using (var conn = _context.CreateConnection())
            {
                await conn.OpenAsync();

                string query = @"
            SELECT
                v.RequestVoucherID,
                v.RequestDate,
                v.WarehouseID,
                v.ProductionDocumentID,
                p.ProductID,
                p.RequestedQuantity,
                v.CreatedAt, v.CreatedBy, v.ModifiedAt, v.ModifiedBy
            FROM RequestVoucher v
            LEFT JOIN RequestVoucherProduct p ON v.RequestVoucherID = p.RequestVoucherID
            WHERE v.RequestVoucherID = @Id";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", voucherId);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            if (voucher == null)
                            {
                                voucher = new RequestVoucher
                                {
                                    RequestVoucherID = reader.GetInt32(0),
                                    RequestDate = reader.IsDBNull(1) ? null : reader.GetDateTime(1),
                                    WarehouseID = reader.GetInt32(2),
                                    ProductionDocumentID = reader.GetInt32(3),
                                    CreatedAt = reader.IsDBNull(6) ? null : reader.GetDateTime(6),
                                    CreatedBy = reader.IsDBNull(7) ? null : reader.GetInt32(7),
                                    ModifiedAt = reader.IsDBNull(8) ? null : reader.GetDateTime(8),
                                    ModifiedBy = reader.IsDBNull(9) ? null : reader.GetInt32(9)
                                };
                            }

                            if (!reader.IsDBNull(4))
                            {
                                products.Add(new RequestVoucherProduct
                                {
                                    RequestVoucherID = reader.GetInt32(0),
                                    ProductID = reader.GetInt32(4),
                                    RequestedQuantity = reader.IsDBNull(5) ? null : reader.GetInt32(5)
                                });
                            }
                        }
                    }
                }
            }

            return (voucher, products);
        }

        public async Task<List<(RequestVoucher Voucher, List<RequestVoucherProduct> Products)>> GetAllWithProductsAsync()
        {
            var dictionary = new Dictionary<int, (RequestVoucher Voucher, List<RequestVoucherProduct> Products)>();
            using (var conn = _context.CreateConnection())
            {
                await conn.OpenAsync();

                string query = @"
            SELECT
                v.RequestVoucherID,
                v.RequestDate,
                v.WarehouseID,
                v.ProductionDocumentID,
                p.ProductID,
                p.RequestedQuantity,
                v.CreatedAt, v.CreatedBy, v.ModifiedAt, v.ModifiedBy
            FROM RequestVoucher v
            LEFT JOIN RequestVoucherProduct p ON v.RequestVoucherID = p.RequestVoucherID";

                using (var cmd = new SqlCommand(query, conn))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        int currentVoucherId = reader.GetInt32(0);
                        if (!dictionary.ContainsKey(currentVoucherId))
                        {
                            var newVoucher = new RequestVoucher
                            {
                                RequestVoucherID = currentVoucherId,
                                RequestDate = reader.IsDBNull(1) ? null : reader.GetDateTime(1),
                                WarehouseID = reader.GetInt32(2),
                                ProductionDocumentID = reader.GetInt32(3),
                                CreatedAt = reader.IsDBNull(6) ? null : reader.GetDateTime(6),
                                CreatedBy = reader.IsDBNull(7) ? null : reader.GetInt32(7),
                                ModifiedAt = reader.IsDBNull(8) ? null : reader.GetDateTime(8),
                                ModifiedBy = reader.IsDBNull(9) ? null : reader.GetInt32(9)
                            };

                            dictionary[currentVoucherId] = (newVoucher, new List<RequestVoucherProduct>());
                        }

                        if (!reader.IsDBNull(4))
                        {
                            var product = new RequestVoucherProduct
                            {
                                RequestVoucherID = currentVoucherId,
                                ProductID = reader.GetInt32(4),
                                RequestedQuantity = reader.IsDBNull(5) ? null : reader.GetInt32(5)
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

                        string deleteItemsQuery = "DELETE FROM RequestVoucherProduct WHERE RequestVoucherID = @VoucherId";
                        using (var cmd = new SqlCommand(deleteItemsQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@VoucherId", voucherId);
                            await cmd.ExecuteNonQueryAsync();
                        }

                        string deleteVoucherQuery = "DELETE FROM RequestVoucher WHERE RequestVoucherID = @VoucherId";
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

        public async Task<bool> UpdateWithProductsAsync(RequestVoucher voucher, List<RequestVoucherProduct> products)
        {
            using (var conn = _context.CreateConnection())
            {
                await conn.OpenAsync();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        var existingProducts = await GetExistingVoucherProductsAsync(conn, transaction, voucher.RequestVoucherID);
                        int oldWarehouseId = await GetWarehouseIdAsync(conn, transaction, voucher.RequestVoucherID);

                        string updateVoucherQuery = @"UPDATE RequestVoucher
                                              SET RequestDate = @RequestDate,
                                                  WarehouseID = @WarehouseID,
                                                  ProductionDocumentID = @ProductionDocumentID,
                                                  ModifiedAt = GETDATE(), ModifiedBy = @ModifiedBy
                                              WHERE RequestVoucherID = @RequestVoucherID";

                        using (var cmd = new SqlCommand(updateVoucherQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@RequestVoucherID", voucher.RequestVoucherID);
                            cmd.Parameters.AddWithValue("@RequestDate", (object?)voucher.RequestDate ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@WarehouseID", voucher.WarehouseID);
                            cmd.Parameters.AddWithValue("@ProductionDocumentID", voucher.ProductionDocumentID);
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

                        string deleteItemsQuery = "DELETE FROM RequestVoucherProduct WHERE RequestVoucherID = @RequestVoucherID";
                        using (var cmdDelete = new SqlCommand(deleteItemsQuery, conn, transaction))
                        {
                            cmdDelete.Parameters.AddWithValue("@RequestVoucherID", voucher.RequestVoucherID);
                            await cmdDelete.ExecuteNonQueryAsync();
                        }

                        if (products != null && products.Count > 0)
                        {
                            string insertItemQuery = @"INSERT INTO RequestVoucherProduct (RequestVoucherID, ProductID, RequestedQuantity) 
                                               VALUES (@RequestVoucherID, @ProductID, @RequestedQuantity)";

                            foreach (var item in products)
                            {
                                int qty = item.RequestedQuantity ?? 0;
                                if (qty > 0)
                                {
                                    await ValidateStockAsync(conn, transaction, voucher.WarehouseID, item.ProductID, qty);
                                }

                                using (var cmdItem = new SqlCommand(insertItemQuery, conn, transaction))
                                {
                                    cmdItem.Parameters.AddWithValue("@RequestVoucherID", voucher.RequestVoucherID);
                                    cmdItem.Parameters.AddWithValue("@ProductID", item.ProductID);
                                    cmdItem.Parameters.AddWithValue("@RequestedQuantity", (object?)item.RequestedQuantity ?? DBNull.Value);

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
            string query = "SELECT ProductID, RequestedQuantity FROM RequestVoucherProduct WHERE RequestVoucherID = @VoucherId";
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
            string query = "SELECT WarehouseID FROM RequestVoucher WHERE RequestVoucherID = @VoucherId";
            using var cmd = new SqlCommand(query, conn, transaction);
            cmd.Parameters.AddWithValue("@VoucherId", voucherId);
            var result = await cmd.ExecuteScalarAsync();
            return result != null && result != DBNull.Value ? Convert.ToInt32(result) : 0;
        }
    }
}
