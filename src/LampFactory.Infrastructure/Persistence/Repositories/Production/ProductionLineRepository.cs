using LampFactory.Infrastructure.Persistence;
using LampFactory.Application.Abstractions.Persistence.Production.ProductionLines;
using LampFactory.Domain.Production;
using LampFactory.Infrastructure.Security;
using Microsoft.Data.SqlClient;

namespace LampFactory.Infrastructure.Persistence.Repositories.Production
{
    public class ProductionLineRepository : IProductionLineRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public ProductionLineRepository(ApplicationDbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<int> CreateAsync(ProductionLine entity)
        {
            string query = "INSERT INTO ProductionLine ([Status], CreatedBy) VALUES (@Status, @CreatedBy); SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using (var conn = _context.CreateConnection())
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Status", entity.Status);
                cmd.Parameters.AddWithValue("@CreatedBy", _currentUserService.ActorId);

                await conn.OpenAsync();
                var result = await cmd.ExecuteScalarAsync();
                return result != null && result != DBNull.Value ? Convert.ToInt32(result) : 0;
            }
        }

        public async Task<List<ProductionLine>> GetAllAsync()
        {
            var productionLines = new List<ProductionLine>();
            string query = "SELECT ProductionLineID, [Status], CreatedAt, CreatedBy, ModifiedAt, ModifiedBy FROM ProductionLine";

            using (var conn = _context.CreateConnection())
            using (var cmd = new SqlCommand(query, conn))
            {
                await conn.OpenAsync();
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        productionLines.Add(new ProductionLine
                        {
                            ProductionLineID = reader.GetInt32(0),
                            Status = reader.GetBoolean(1),
                            CreatedAt = reader.IsDBNull(2) ? null : reader.GetDateTime(2),
                            CreatedBy = reader.IsDBNull(3) ? null : reader.GetInt32(3),
                            ModifiedAt = reader.IsDBNull(4) ? null : reader.GetDateTime(4),
                            ModifiedBy = reader.IsDBNull(5) ? null : reader.GetInt32(5)
                        });
                    }
                }
            }
            return productionLines;
        }

        public async Task<ProductionLine?> GetByIdAsync(int id)
        {
            string query = "SELECT ProductionLineID, [Status], CreatedAt, CreatedBy, ModifiedAt, ModifiedBy FROM ProductionLine WHERE ProductionLineID = @Id";

            using (var conn = _context.CreateConnection())
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                await conn.OpenAsync();

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return new ProductionLine
                        {
                            ProductionLineID = reader.GetInt32(0),
                            Status = reader.GetBoolean(1),
                            CreatedAt = reader.IsDBNull(2) ? null : reader.GetDateTime(2),
                            CreatedBy = reader.IsDBNull(3) ? null : reader.GetInt32(3),
                            ModifiedAt = reader.IsDBNull(4) ? null : reader.GetDateTime(4),
                            ModifiedBy = reader.IsDBNull(5) ? null : reader.GetInt32(5)
                        };
                    }
                }
            }
            return null;
        }

        public async Task<bool> RemoveAsync(int id)
        {
            string query = "DELETE FROM ProductionLine WHERE ProductionLineID = @Id";

            using (var conn = _context.CreateConnection())
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                await conn.OpenAsync();
                return await cmd.ExecuteNonQueryAsync() > 0;
            }
        }

        public async Task<bool> UpdateAsync(ProductionLine entity)
        {
            string query = "UPDATE ProductionLine SET [Status] = @Status, ModifiedAt = GETDATE(), ModifiedBy = @ModifiedBy WHERE ProductionLineID = @Id";

            using (var conn = _context.CreateConnection())
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Id", entity.ProductionLineID);
                cmd.Parameters.AddWithValue("@Status", entity.Status);
                cmd.Parameters.AddWithValue("@ModifiedBy", _currentUserService.ActorId);

                await conn.OpenAsync();
                return await cmd.ExecuteNonQueryAsync() > 0;
            }
        }
    }
}
