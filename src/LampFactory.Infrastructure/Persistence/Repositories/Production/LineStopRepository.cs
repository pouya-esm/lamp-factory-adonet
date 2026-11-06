using LampFactory.Infrastructure.Persistence;
using LampFactory.Application.Abstractions.Persistence.Production.LineStops;
using LampFactory.Domain.Production;
using LampFactory.Infrastructure.Security;
using Microsoft.Data.SqlClient;

namespace LampFactory.Infrastructure.Persistence.Repositories.Production
{
    public class LineStopRepository : ILineStopRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public LineStopRepository(ApplicationDbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<int> CreateAsync(LineStop entity)
        {
            string query = @"INSERT INTO LineStop (LineStopDate, Reason, ProductionDocumentID, CreatedBy)
                            VALUES (@LineStopDate, @Reason, @ProductionDocumentID, @CreatedBy);
                            SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using (var conn = _context.CreateConnection())
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@LineStopDate", (object?)entity.LineStopDate ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Reason", (object?)entity.Reason ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ProductionDocumentID", entity.ProductionDocumentID);
                cmd.Parameters.AddWithValue("@CreatedBy", _currentUserService.ActorId);

                await conn.OpenAsync();
                var result = await cmd.ExecuteScalarAsync();
                return result != null && result != DBNull.Value ? Convert.ToInt32(result) : 0;
            }
        }

        public async Task<List<LineStop>> GetAllAsync()
        {
            var lineStops = new List<LineStop>();
            string query = "SELECT LineStopID, LineStopDate, Reason, ProductionDocumentID, CreatedAt, CreatedBy, ModifiedAt, ModifiedBy FROM LineStop";

            using (var conn = _context.CreateConnection())
            using (var cmd = new SqlCommand(query, conn))
            {
                await conn.OpenAsync();
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        lineStops.Add(new LineStop
                        {
                            LineStopID = reader.GetInt32(0),
                            LineStopDate = reader.IsDBNull(1) ? null : reader.GetDateTime(1),
                            Reason = reader.IsDBNull(2) ? null : reader.GetString(2),
                            ProductionDocumentID = reader.GetInt32(3),
                            CreatedAt = reader.IsDBNull(4) ? null : reader.GetDateTime(4),
                            CreatedBy = reader.IsDBNull(5) ? null : reader.GetInt32(5),
                            ModifiedAt = reader.IsDBNull(6) ? null : reader.GetDateTime(6),
                            ModifiedBy = reader.IsDBNull(7) ? null : reader.GetInt32(7)
                        });
                    }
                }
            }
            return lineStops;
        }

        public async Task<LineStop?> GetByIdAsync(int id)
        {
            string query = "SELECT LineStopID, LineStopDate, Reason, ProductionDocumentID, CreatedAt, CreatedBy, ModifiedAt, ModifiedBy FROM LineStop WHERE LineStopID = @Id";

            using (var conn = _context.CreateConnection())
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                await conn.OpenAsync();

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return new LineStop
                        {
                            LineStopID = reader.GetInt32(0),
                            LineStopDate = reader.IsDBNull(1) ? null : reader.GetDateTime(1),
                            Reason = reader.IsDBNull(2) ? null : reader.GetString(2),
                            ProductionDocumentID = reader.GetInt32(3),
                            CreatedAt = reader.IsDBNull(4) ? null : reader.GetDateTime(4),
                            CreatedBy = reader.IsDBNull(5) ? null : reader.GetInt32(5),
                            ModifiedAt = reader.IsDBNull(6) ? null : reader.GetDateTime(6),
                            ModifiedBy = reader.IsDBNull(7) ? null : reader.GetInt32(7)
                        };
                    }
                }
            }
            return null;
        }

        public async Task<bool> RemoveAsync(int id)
        {
            string query = "DELETE FROM LineStop WHERE LineStopID = @Id";

            using (var conn = _context.CreateConnection())
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                await conn.OpenAsync();
                return await cmd.ExecuteNonQueryAsync() > 0;
            }
        }

        public async Task<bool> UpdateAsync(LineStop entity)
        {
            string query = @"UPDATE LineStop
                            SET LineStopDate = @LineStopDate, Reason = @Reason, ProductionDocumentID = @ProductionDocumentID,
                                ModifiedAt = GETDATE(), ModifiedBy = @ModifiedBy
                            WHERE LineStopID = @Id";

            using (var conn = _context.CreateConnection())
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Id", entity.LineStopID);
                cmd.Parameters.AddWithValue("@LineStopDate", (object?)entity.LineStopDate ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Reason", (object?)entity.Reason ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ProductionDocumentID", entity.ProductionDocumentID);
                cmd.Parameters.AddWithValue("@ModifiedBy", _currentUserService.ActorId);

                await conn.OpenAsync();
                return await cmd.ExecuteNonQueryAsync() > 0;
            }
        }
    }
}
