using LampFactory.Infrastructure.Persistence;
using LampFactory.Application.Abstractions.Persistence.Purchasing.PurchaseVouchers;
using LampFactory.Domain.Inventory;
using LampFactory.Domain.Purchasing;
using LampFactory.Infrastructure.Security;
using Microsoft.Data.SqlClient;

namespace LampFactory.Infrastructure.Persistence.Repositories.Purchasing
{
    public class PurchaseVoucherRepository : IPurchaseVoucherRepository
{
        private readonly ApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public PurchaseVoucherRepository(ApplicationDbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<int> CreateWithProductsAsync(PurchaseVoucher voucher, List<PurchaseVoucherProduct> products)
        {
            using (var conn = _context.CreateConnection())
            {
                await conn.OpenAsync();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        string query = @"INSERT INTO PurchaseVoucher (PurchaseDate, WarehouseID, CreatedBy)
                                        VALUES (@PurchaseDate, @WarehouseID, @CreatedBy);
                                        SELECT SCOPE_IDENTITY();";

                        int voucherId;
                        using (var cmd = new SqlCommand(query, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@PurchaseDate", (object?)voucher.PurchaseDate ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@WarehouseID", voucher.WarehouseID);
                            cmd.Parameters.AddWithValue("@CreatedBy", _currentUserService.ActorId);

                            var result = await cmd.ExecuteScalarAsync();
                            voucherId = Convert.ToInt32(result);
                        }

                        if (products != null && products.Count > 0)
                        {
                            string itemQuery = @"INSERT INTO PurchaseVoucherProduct (PurchaseVoucherID, ProductID, PurchasedQuantity) 
                                               VALUES (@PurchaseVoucherID, @ProductID, @PurchasedQuantity)";

                            foreach (var item in products)
                            {
                                using (var cmdItem = new SqlCommand(itemQuery, conn, transaction))
                                {
                                    cmdItem.Parameters.AddWithValue("@PurchaseVoucherID", voucherId);
                                    cmdItem.Parameters.AddWithValue("@ProductID", item.ProductID);
                                    cmdItem.Parameters.AddWithValue("@PurchasedQuantity", (object?)item.PurchasedQuantity ?? DBNull.Value);

                                    await cmdItem.ExecuteNonQueryAsync();
                                }

                                int qty = item.PurchasedQuantity ?? 0;
                                if (qty > 0)
                                {
                                    await IncreaseStockAsync(conn, transaction, voucher.WarehouseID, item.ProductID, qty);
                                }
                            }
                        }

                        await transaction.CommitAsync();
                        return voucherId;
                    }
                    catch
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                }
            }
        }

        public async Task<(PurchaseVoucher? Voucher, List<PurchaseVoucherProduct> Products)> GetByIdWithProductsAsync(int voucherId)
        {
            PurchaseVoucher? voucher = null;
            var products = new List<PurchaseVoucherProduct>();

            using (var conn = _context.CreateConnection())
            {
                await conn.OpenAsync();

                string query = @"
            SELECT
                v.PurchaseVoucherID,
                v.PurchaseDate,
                v.WarehouseID,
                p.ProductID,
                p.PurchasedQuantity,
                v.CreatedAt, v.CreatedBy, v.ModifiedAt, v.ModifiedBy
            FROM PurchaseVoucher v
            LEFT JOIN PurchaseVoucherProduct p ON v.PurchaseVoucherID = p.PurchaseVoucherID
            WHERE v.PurchaseVoucherID = @Id";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", voucherId);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            if (voucher == null)
                            {
                                voucher = new PurchaseVoucher
                                {
                                    PurchaseVoucherID = reader.GetInt32(0),
                                    PurchaseDate = reader.IsDBNull(1) ? null : reader.GetDateTime(1),
                                    WarehouseID = reader.GetInt32(2),
                                    CreatedAt = reader.IsDBNull(5) ? null : reader.GetDateTime(5),
                                    CreatedBy = reader.IsDBNull(6) ? null : reader.GetInt32(6),
                                    ModifiedAt = reader.IsDBNull(7) ? null : reader.GetDateTime(7),
                                    ModifiedBy = reader.IsDBNull(8) ? null : reader.GetInt32(8)
                                };
                            }

                            if (!reader.IsDBNull(3))
                            {
                                products.Add(new PurchaseVoucherProduct
                                {
                                    PurchaseVoucherID = reader.GetInt32(0),
                                    ProductID = reader.GetInt32(3),
                                    PurchasedQuantity = reader.IsDBNull(4) ? null : reader.GetInt32(4)
                                });
                            }
                        }
                    }
                }
            }

            return (voucher, products);
        }

        public async Task<List<(PurchaseVoucher Voucher, List<PurchaseVoucherProduct> Products)>> GetAllWithProductsAsync()
        {
            var dictionary = new Dictionary<int, (PurchaseVoucher Voucher, List<PurchaseVoucherProduct> Products)>();
            using (var conn = _context.CreateConnection())
            {
                await conn.OpenAsync();

                string query = @"
            SELECT
                v.PurchaseVoucherID,
                v.PurchaseDate,
                v.WarehouseID,
                p.ProductID,
                p.PurchasedQuantity,
                v.CreatedAt, v.CreatedBy, v.ModifiedAt, v.ModifiedBy
            FROM PurchaseVoucher v
            LEFT JOIN PurchaseVoucherProduct p ON v.PurchaseVoucherID = p.PurchaseVoucherID";

                using (var cmd = new SqlCommand(query, conn))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        int currentVoucherId = reader.GetInt32(0);
                        if (!dictionary.ContainsKey(currentVoucherId))
                        {
                            var newVoucher = new PurchaseVoucher
                            {
                                PurchaseVoucherID = currentVoucherId,
                                PurchaseDate = reader.IsDBNull(1) ? null : reader.GetDateTime(1),
                                WarehouseID = reader.GetInt32(2),
                                CreatedAt = reader.IsDBNull(5) ? null : reader.GetDateTime(5),
                                CreatedBy = reader.IsDBNull(6) ? null : reader.GetInt32(6),
                                ModifiedAt = reader.IsDBNull(7) ? null : reader.GetDateTime(7),
                                ModifiedBy = reader.IsDBNull(8) ? null : reader.GetInt32(8)
                            };

                            dictionary[currentVoucherId] = (newVoucher, new List<PurchaseVoucherProduct>());
                        }

                        if (!reader.IsDBNull(3))
                        {
                            var product = new PurchaseVoucherProduct
                            {
                                PurchaseVoucherID = currentVoucherId,
                                ProductID = reader.GetInt32(3),
                                PurchasedQuantity = reader.IsDBNull(4) ? null : reader.GetInt32(4)
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

                        string deleteItemsQuery = "DELETE FROM PurchaseVoucherProduct WHERE PurchaseVoucherID = @VoucherId";
                        using (var cmd = new SqlCommand(deleteItemsQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@VoucherId", voucherId);
                            await cmd.ExecuteNonQueryAsync();
                        }

                        string deleteVoucherQuery = "DELETE FROM PurchaseVoucher WHERE PurchaseVoucherID = @VoucherId";
                        using (var cmd = new SqlCommand(deleteVoucherQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@VoucherId", voucherId);
                            await cmd.ExecuteNonQueryAsync();
                        }

                        foreach (var (productId, qty) in existingProducts)
                        {
                            if (qty > 0)
                            {
                                await DecreaseStockAsync(conn, transaction, warehouseId, productId, qty);
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

        public async Task<bool> UpdateWithProductsAsync(PurchaseVoucher voucher, List<PurchaseVoucherProduct> products)
        {
            using (var conn = _context.CreateConnection())
            {
                await conn.OpenAsync();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        var existingProducts = await GetExistingVoucherProductsAsync(conn, transaction, voucher.PurchaseVoucherID);
                        int oldWarehouseId = await GetWarehouseIdAsync(conn, transaction, voucher.PurchaseVoucherID);

                        // ۲-۱. به‌روزرسانی هدر رسید خرید
                        string updateVoucherQuery = @"UPDATE PurchaseVoucher
                                              SET PurchaseDate = @PurchaseDate,
                                                  WarehouseID = @WarehouseID,
                                                  ModifiedAt = GETDATE(), ModifiedBy = @ModifiedBy
                                              WHERE PurchaseVoucherID = @PurchaseVoucherID";

                        using (var cmd = new SqlCommand(updateVoucherQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@PurchaseVoucherID", voucher.PurchaseVoucherID);
                            cmd.Parameters.AddWithValue("@PurchaseDate", (object?)voucher.PurchaseDate ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@WarehouseID", voucher.WarehouseID);
                            cmd.Parameters.AddWithValue("@ModifiedBy", _currentUserService.ActorId);

                            await cmd.ExecuteNonQueryAsync();
                        }

                        foreach (var (productId, qty) in existingProducts)
                        {
                            if (qty > 0)
                            {
                                await DecreaseStockAsync(conn, transaction, oldWarehouseId, productId, qty);
                            }
                        }

                        // ۲-۲. حذف اقلام قبلی رسید
                        string deleteProductsQuery = "DELETE FROM PurchaseVoucherProduct WHERE PurchaseVoucherID = @VoucherId";
                        using (var cmdDel = new SqlCommand(deleteProductsQuery, conn, transaction))
                        {
                            cmdDel.Parameters.AddWithValue("@VoucherId", voucher.PurchaseVoucherID);
                            await cmdDel.ExecuteNonQueryAsync();
                        }

                        // ۲-۳. ثبت مجدد اقلام جدید
                        if (products != null && products.Count > 0)
                        {
                            string insertProductQuery = @"INSERT INTO PurchaseVoucherProduct (PurchaseVoucherID, ProductID, PurchasedQuantity) 
                                                VALUES (@PurchaseVoucherID, @ProductID, @PurchasedQuantity)";

                            foreach (var item in products)
                            {
                                using (var cmdIns = new SqlCommand(insertProductQuery, conn, transaction))
                                {
                                    cmdIns.Parameters.AddWithValue("@PurchaseVoucherID", voucher.PurchaseVoucherID);
                                    cmdIns.Parameters.AddWithValue("@ProductID", item.ProductID);
                                    cmdIns.Parameters.AddWithValue("@PurchasedQuantity", (object?)item.PurchasedQuantity ?? DBNull.Value);

                                    await cmdIns.ExecuteNonQueryAsync();
                                }

                                int qty = item.PurchasedQuantity ?? 0;
                                if (qty > 0)
                                {
                                    await IncreaseStockAsync(conn, transaction, voucher.WarehouseID, item.ProductID, qty);
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

        private async Task<List<(int ProductID, int Quantity)>> GetExistingVoucherProductsAsync(SqlConnection conn, SqlTransaction transaction, int voucherId)
        {
            var list = new List<(int, int)>();
            string query = "SELECT ProductID, PurchasedQuantity FROM PurchaseVoucherProduct WHERE PurchaseVoucherID = @VoucherId";
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
            string query = "SELECT WarehouseID FROM PurchaseVoucher WHERE PurchaseVoucherID = @VoucherId";
            using var cmd = new SqlCommand(query, conn, transaction);
            cmd.Parameters.AddWithValue("@VoucherId", voucherId);
            var result = await cmd.ExecuteScalarAsync();
            return result != null && result != DBNull.Value ? Convert.ToInt32(result) : 0;
        }
    }
}
