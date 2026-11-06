using LampFactory.Infrastructure.Persistence;
using LampFactory.Application.Abstractions.Persistence.Vouchers.ChargeVouchers;
using LampFactory.Domain.Inventory;
using LampFactory.Domain.Vouchers;
using LampFactory.Infrastructure.Security;
using Microsoft.Data.SqlClient;

namespace LampFactory.Infrastructure.Persistence.Repositories.Vouchers
{
    public class ChargeVoucherRepository : IChargeVoucherRepository
{
        private readonly ApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public ChargeVoucherRepository(ApplicationDbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<int> CreateWithProductsAsync(ChargeVoucher voucher, List<ChargeVoucherProduct> products)
        {
            using (var conn = _context.CreateConnection())
            {
                await conn.OpenAsync();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        string query = @"INSERT INTO ChargeVoucher (ChargeDate, WarehouseID, ProductionDocumentID, CreatedBy)
                                        VALUES (@ChargeDate, @WarehouseID, @ProductionDocumentID, @CreatedBy);
                                        SELECT SCOPE_IDENTITY();";

                        int voucherId;
                        using (var cmd = new SqlCommand(query, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@ChargeDate", (object?)voucher.ChargeDate ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@WarehouseID", voucher.WarehouseID);
                            cmd.Parameters.AddWithValue("@ProductionDocumentID", voucher.ProductionDocumentID);
                            cmd.Parameters.AddWithValue("@CreatedBy", _currentUserService.ActorId);

                            var result = await cmd.ExecuteScalarAsync();
                            voucherId = Convert.ToInt32(result);
                        }

                        if (products != null && products.Count > 0)
                        {
                            string itemQuery = @"INSERT INTO ChargeVoucherProduct (ChargeVoucherID, ProductID, ChargeQuantity) 
                                               VALUES (@ChargeVoucherID, @ProductID, @ChargeQuantity)";

                            foreach (var item in products)
                            {
                                using (var cmdItem = new SqlCommand(itemQuery, conn, transaction))
                                {
                                    cmdItem.Parameters.AddWithValue("@ChargeVoucherID", voucherId);
                                    cmdItem.Parameters.AddWithValue("@ProductID", item.ProductID);
                                    cmdItem.Parameters.AddWithValue("@ChargeQuantity", (object?)item.ChargeQuantity ?? DBNull.Value);

                                    await cmdItem.ExecuteNonQueryAsync();
                                }

                                int qty = item.ChargeQuantity ?? 0;
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

        public async Task<(ChargeVoucher? Voucher, List<ChargeVoucherProduct> Products)> GetByIdWithProductsAsync(int voucherId)
        {
            ChargeVoucher? voucher = null;
            var products = new List<ChargeVoucherProduct>();
            using (var conn = _context.CreateConnection())
            {
                await conn.OpenAsync();

                string query = @"
            SELECT
                v.ChargeVoucherID,
                v.ChargeDate,
                v.WarehouseID,
                v.ProductionDocumentID,
                p.ProductID,
                p.ChargeQuantity,
                v.CreatedAt, v.CreatedBy, v.ModifiedAt, v.ModifiedBy
            FROM ChargeVoucher v
            LEFT JOIN ChargeVoucherProduct p ON v.ChargeVoucherID = p.ChargeVoucherID
            WHERE v.ChargeVoucherID = @Id";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", voucherId);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            if (voucher == null)
                            {
                                voucher = new ChargeVoucher
                                {
                                    ChargeVoucherID = reader.GetInt32(0),
                                    ChargeDate = reader.IsDBNull(1) ? null : reader.GetDateTime(1),
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
                                products.Add(new ChargeVoucherProduct
                                {
                                    ChargeVoucherID = reader.GetInt32(0),
                                    ProductID = reader.GetInt32(4),
                                    ChargeQuantity = reader.IsDBNull(5) ? null : reader.GetInt32(5)
                                });
                            }
                        }
                    }
                }
            }

            return (voucher, products);
        }

        public async Task<List<(ChargeVoucher Voucher, List<ChargeVoucherProduct> Products)>> GetAllWithProductsAsync()
        {
            var dictionary = new Dictionary<int, (ChargeVoucher Voucher, List<ChargeVoucherProduct> Products)>();

            using (var conn = _context.CreateConnection())
            {
                await conn.OpenAsync();

                string query = @"
            SELECT
                v.ChargeVoucherID,
                v.ChargeDate,
                v.WarehouseID,
                v.ProductionDocumentID,
                p.ProductID,
                p.ChargeQuantity,
                v.CreatedAt, v.CreatedBy, v.ModifiedAt, v.ModifiedBy
            FROM ChargeVoucher v
            LEFT JOIN ChargeVoucherProduct p ON v.ChargeVoucherID = p.ChargeVoucherID";

                using (var cmd = new SqlCommand(query, conn))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        int currentVoucherId = reader.GetInt32(0);

                        if (!dictionary.ContainsKey(currentVoucherId))
                        {
                            var newVoucher = new ChargeVoucher
                            {
                                ChargeVoucherID = currentVoucherId,
                                ChargeDate = reader.IsDBNull(1) ? null : reader.GetDateTime(1),
                                WarehouseID = reader.GetInt32(2),
                                ProductionDocumentID = reader.GetInt32(3),
                                CreatedAt = reader.IsDBNull(6) ? null : reader.GetDateTime(6),
                                CreatedBy = reader.IsDBNull(7) ? null : reader.GetInt32(7),
                                ModifiedAt = reader.IsDBNull(8) ? null : reader.GetDateTime(8),
                                ModifiedBy = reader.IsDBNull(9) ? null : reader.GetInt32(9)
                            };

                            dictionary[currentVoucherId] = (newVoucher, new List<ChargeVoucherProduct>());
                        }

                        if (!reader.IsDBNull(4))
                        {
                            var product = new ChargeVoucherProduct
                            {
                                ChargeVoucherID = currentVoucherId,
                                ProductID = reader.GetInt32(4),
                                ChargeQuantity = reader.IsDBNull(5) ? null : reader.GetInt32(5)
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

                        string deleteItemsQuery = "DELETE FROM ChargeVoucherProduct WHERE ChargeVoucherID = @VoucherId";
                        using (var cmd = new SqlCommand(deleteItemsQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@VoucherId", voucherId);
                            await cmd.ExecuteNonQueryAsync();
                        }

                        string deleteVoucherQuery = "DELETE FROM ChargeVoucher WHERE ChargeVoucherID = @VoucherId";
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

        public async Task<bool> UpdateWithProductsAsync(ChargeVoucher voucher, List<ChargeVoucherProduct> products)
        {
            using (var conn = _context.CreateConnection())
            {
                await conn.OpenAsync();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // ۳-۱. به‌روزرسانی هدر رسید شارژ
                        var existingProducts = await GetExistingVoucherProductsAsync(conn, transaction, voucher.ChargeVoucherID);
                        int oldWarehouseId = await GetWarehouseIdAsync(conn, transaction, voucher.ChargeVoucherID);

                        string updateVoucherQuery = @"UPDATE ChargeVoucher
                                              SET ChargeDate = @ChargeDate,
                                                  WarehouseID = @WarehouseID,
                                                  ProductionDocumentID = @ProductionDocumentID,
                                                  ModifiedAt = GETDATE(), ModifiedBy = @ModifiedBy
                                              WHERE ChargeVoucherID = @ChargeVoucherID";

                        using (var cmd = new SqlCommand(updateVoucherQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@ChargeVoucherID", voucher.ChargeVoucherID);
                            cmd.Parameters.AddWithValue("@ChargeDate", (object?)voucher.ChargeDate ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@WarehouseID", voucher.WarehouseID);
                            cmd.Parameters.AddWithValue("@ProductionDocumentID", voucher.ProductionDocumentID);
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

                        // ۳-۲. حذف اقلام قبلی رسید شارژ
                        string deleteProductsQuery = "DELETE FROM ChargeVoucherProduct WHERE ChargeVoucherID = @VoucherId";
                        using (var cmdDel = new SqlCommand(deleteProductsQuery, conn, transaction))
                        {
                            cmdDel.Parameters.AddWithValue("@VoucherId", voucher.ChargeVoucherID);
                            await cmdDel.ExecuteNonQueryAsync();
                        }

                        // ۳-۳. ثبت مجدد اقلام جدید
                        if (products != null && products.Count > 0)
                        {
                            string insertProductQuery = @"INSERT INTO ChargeVoucherProduct (ChargeVoucherID, ProductID, ChargeQuantity) 
                                                VALUES (@ChargeVoucherID, @ProductID, @ChargeQuantity)";

                            foreach (var item in products)
                            {
                                using (var cmdIns = new SqlCommand(insertProductQuery, conn, transaction))
                                {
                                    cmdIns.Parameters.AddWithValue("@ChargeVoucherID", voucher.ChargeVoucherID);
                                    cmdIns.Parameters.AddWithValue("@ProductID", item.ProductID);
                                    cmdIns.Parameters.AddWithValue("@ChargeQuantity", (object?)item.ChargeQuantity ?? DBNull.Value);

                                    await cmdIns.ExecuteNonQueryAsync();
                                }

                                int qty = item.ChargeQuantity ?? 0;
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
            string query = "SELECT ProductID, ChargeQuantity FROM ChargeVoucherProduct WHERE ChargeVoucherID = @VoucherId";
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
            string query = "SELECT WarehouseID FROM ChargeVoucher WHERE ChargeVoucherID = @VoucherId";
            using var cmd = new SqlCommand(query, conn, transaction);
            cmd.Parameters.AddWithValue("@VoucherId", voucherId);
            var result = await cmd.ExecuteScalarAsync();
            return result != null && result != DBNull.Value ? Convert.ToInt32(result) : 0;
        }
    }
}
