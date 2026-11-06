using LampFactory.Infrastructure.Persistence;
using LampFactory.Application.Abstractions.Persistence.Inventory.Products;
using LampFactory.Domain.Inventory;
using LampFactory.Infrastructure.Security;
using Microsoft.Data.SqlClient;

namespace LampFactory.Infrastructure.Persistence.Repositories.Inventory
{
    public class ProductRepository : IProductRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public ProductRepository(ApplicationDbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<int> CreateAsync(Product entity)
        {
            string query = @"INSERT INTO Product (ProductType, ProductColor, Material, StandardNumber, Weight, Voltage, Power, LampholderType, ConsumptionUnit, CreatedBy)
                            VALUES (@ProductType, @ProductColor, @Material, @StandardNumber, @Weight, @Voltage, @Power, @LampholderType, @ConsumptionUnit, @CreatedBy);
                            SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using (var conn = _context.CreateConnection())
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@ProductType", entity.ProductType.ToString());
                cmd.Parameters.AddWithValue("@ProductColor", (object?)entity.ProductColor ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Material", (object?)entity.Material ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@StandardNumber", (object?)entity.StandardNumber ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Weight", (object?)entity.Weight ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Voltage", (object?)entity.Voltage ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Power", (object?)entity.Power ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@LampholderType", (object?)entity.LampholderType ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ConsumptionUnit", (object?)entity.ConsumptionUnit ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@CreatedBy", _currentUserService.ActorId);

                await conn.OpenAsync();
                var result = await cmd.ExecuteScalarAsync();
                return result != null && result != DBNull.Value ? Convert.ToInt32(result) : 0;
            }
        }

        public async Task<List<Product>> GetAllAsync()
        {
            var products = new List<Product>();
            string query = "SELECT ProductID, ProductType, ProductColor, Material, StandardNumber, Weight, Voltage, Power, LampholderType, ConsumptionUnit, CreatedAt, CreatedBy, ModifiedAt, ModifiedBy FROM Product";

            using (var conn = _context.CreateConnection())
            using (var cmd = new SqlCommand(query, conn))
            {
                await conn.OpenAsync();
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        products.Add(new Product
                        {
                            ProductID = reader.GetInt32(0),
                            ProductType = Enum.Parse<ProductType>(reader.GetString(1)),
                            ProductColor = reader.IsDBNull(2) ? null : reader.GetString(2),
                            Material = reader.IsDBNull(3) ? null : reader.GetString(3),
                            StandardNumber = reader.IsDBNull(4) ? null : reader.GetString(4),
                            Weight = reader.IsDBNull(5) ? null : reader.GetDouble(5),
                            Voltage = reader.IsDBNull(6) ? null : reader.GetDouble(6),
                            Power = reader.IsDBNull(7) ? null : reader.GetDouble(7),
                            LampholderType = reader.IsDBNull(8) ? null : reader.GetString(8),
                            ConsumptionUnit = reader.IsDBNull(9) ? null : reader.GetString(9),
                            CreatedAt = reader.IsDBNull(10) ? null : reader.GetDateTime(10),
                            CreatedBy = reader.IsDBNull(11) ? null : reader.GetInt32(11),
                            ModifiedAt = reader.IsDBNull(12) ? null : reader.GetDateTime(12),
                            ModifiedBy = reader.IsDBNull(13) ? null : reader.GetInt32(13)
                        });
                    }
                }
            }
            return products;
        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            string query = "SELECT ProductID, ProductType, ProductColor, Material, StandardNumber, Weight, Voltage, Power, LampholderType, ConsumptionUnit, CreatedAt, CreatedBy, ModifiedAt, ModifiedBy FROM Product WHERE ProductID = @Id";

            using (var conn = _context.CreateConnection())
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                await conn.OpenAsync();

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return new Product
                        {
                            ProductID = reader.GetInt32(0),
                            ProductType = Enum.Parse<ProductType>(reader.GetString(1)),
                            ProductColor = reader.IsDBNull(2) ? null : reader.GetString(2),
                            Material = reader.IsDBNull(3) ? null : reader.GetString(3),
                            StandardNumber = reader.IsDBNull(4) ? null : reader.GetString(4),
                            Weight = reader.IsDBNull(5) ? null : reader.GetDouble(5),
                            Voltage = reader.IsDBNull(6) ? null : reader.GetDouble(6),
                            Power = reader.IsDBNull(7) ? null : reader.GetDouble(7),
                            LampholderType = reader.IsDBNull(8) ? null : reader.GetString(8),
                            ConsumptionUnit = reader.IsDBNull(9) ? null : reader.GetString(9),
                            CreatedAt = reader.IsDBNull(10) ? null : reader.GetDateTime(10),
                            CreatedBy = reader.IsDBNull(11) ? null : reader.GetInt32(11),
                            ModifiedAt = reader.IsDBNull(12) ? null : reader.GetDateTime(12),
                            ModifiedBy = reader.IsDBNull(13) ? null : reader.GetInt32(13)
                        };
                    }
                }
            }
            return null;
        }

        public async Task<bool> RemoveAsync(int id)
        {
            string query = "DELETE FROM Product WHERE ProductID = @Id";

            using (var conn = _context.CreateConnection())
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                await conn.OpenAsync();
                return await cmd.ExecuteNonQueryAsync() > 0;
            }
        }

        public async Task<bool> UpdateAsync(Product entity)
        {
            string query = @"UPDATE Product 
                            SET ProductType = @ProductType, ProductColor = @ProductColor, Material = @Material, 
                                StandardNumber = @StandardNumber, Weight = @Weight, Voltage = @Voltage, 
                                Power = @Power, LampholderType = @LampholderType, ConsumptionUnit = @ConsumptionUnit,
                                ModifiedAt = GETDATE(), ModifiedBy = @ModifiedBy 
                            WHERE ProductID = @Id";

            using (var conn = _context.CreateConnection())
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Id", entity.ProductID);
                cmd.Parameters.AddWithValue("@ProductType", entity.ProductType.ToString());
                cmd.Parameters.AddWithValue("@ProductColor", (object?)entity.ProductColor ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Material", (object?)entity.Material ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@StandardNumber", (object?)entity.StandardNumber ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Weight", (object?)entity.Weight ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Voltage", (object?)entity.Voltage ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Power", (object?)entity.Power ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@LampholderType", (object?)entity.LampholderType ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ConsumptionUnit", (object?)entity.ConsumptionUnit ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ModifiedBy", _currentUserService.ActorId);

                await conn.OpenAsync();
                return await cmd.ExecuteNonQueryAsync() > 0;
            }
        }
    }
}
