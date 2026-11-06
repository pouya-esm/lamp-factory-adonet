using LampFactory.Infrastructure.Persistence;
using LampFactory.Application.Abstractions.Persistence.Production.Operators;
using LampFactory.Domain.Operators;
using LampFactory.Infrastructure.Security;
using Microsoft.Data.SqlClient;

namespace LampFactory.Infrastructure.Persistence.Repositories.Production
{
    public class OperatorRepository : IOperatorRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        public OperatorRepository(ApplicationDbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<int> CreateAsync(Operator entity)
        {
            string query = "INSERT INTO Operator (NationalID, FirstName, LastName, HiringDate, CreatedBy) VALUES (@NationalID, @FirstName, @LastName, @HiringDate, @CreatedBy); SELECT CAST(SCOPE_IDENTITY() AS INT);";
            using (var conn = _context.CreateConnection())
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@NationalID", entity.NationalID);
                cmd.Parameters.AddWithValue("@FirstName", (object?)entity.FirstName ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@LastName", (object?)entity.LastName ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@HiringDate", (object?)entity.HiringDate ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@CreatedBy", _currentUserService.ActorId);
                await conn.OpenAsync();
                var result = await cmd.ExecuteScalarAsync();
                return result != null && result != DBNull.Value ? Convert.ToInt32(result) : 0;
            }

        }

        public async Task<List<Operator>> GetAllAsync()
        {
            var operators = new List<Operator>();
            string query = "SELECT OperatorID, NationalID, FirstName, LastName, HiringDate, CreatedAt, CreatedBy, ModifiedAt, ModifiedBy FROM Operator";

            using (var conn = _context.CreateConnection())
            using (var cmd = new SqlCommand(query, conn))
            {
                await conn.OpenAsync();
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        operators.Add(new Operator
                        {
                            OperatorID = reader.GetInt32(0),
                            NationalID = reader.GetString(1),
                            FirstName = reader.IsDBNull(2) ? null : reader.GetString(2),
                            LastName = reader.IsDBNull(3) ? null : reader.GetString(3),
                            HiringDate = reader.IsDBNull(4) ? null : reader.GetDateTime(4),
                            CreatedAt = reader.IsDBNull(5) ? null : reader.GetDateTime(5),
                            CreatedBy = reader.IsDBNull(6) ? null : reader.GetInt32(6),
                            ModifiedAt = reader.IsDBNull(7) ? null : reader.GetDateTime(7),
                            ModifiedBy = reader.IsDBNull(8) ? null : reader.GetInt32(8)
                        });
                    }
                }
            }
            return operators;
        }

        public async Task<Operator?> GetByIdAsync(int id)
        {
            string query = "SELECT OperatorID, NationalID, FirstName, LastName, HiringDate, CreatedAt, CreatedBy, ModifiedAt, ModifiedBy FROM Operator WHERE OperatorID = @Id";

            using (var conn = _context.CreateConnection())
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                await conn.OpenAsync();

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return new Operator
                        {
                            OperatorID = reader.GetInt32(0),
                            NationalID = reader.GetString(1),
                            FirstName = reader.IsDBNull(2) ? null : reader.GetString(2),
                            LastName = reader.IsDBNull(3) ? null : reader.GetString(3),
                            HiringDate = reader.IsDBNull(4) ? null : reader.GetDateTime(4),
                            CreatedAt = reader.IsDBNull(5) ? null : reader.GetDateTime(5),
                            CreatedBy = reader.IsDBNull(6) ? null : reader.GetInt32(6),
                            ModifiedAt = reader.IsDBNull(7) ? null : reader.GetDateTime(7),
                            ModifiedBy = reader.IsDBNull(8) ? null : reader.GetInt32(8)
                        };
                    }
                }
            }
            return null;
        }

        public async Task<bool> RemoveAsync(int id)
        {
            string query = "DELETE FROM Operator WHERE OperatorID = @Id";

            using (var conn = _context.CreateConnection())
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                await conn.OpenAsync();
                return await cmd.ExecuteNonQueryAsync() > 0;
            }
        }

        public async Task<bool> UpdateAsync(Operator entity)
        {
            string query = @"UPDATE Operator
                            SET NationalID = @NationalID, FirstName = @FirstName, LastName = @LastName, HiringDate = @HiringDate,
                                ModifiedAt = GETDATE(), ModifiedBy = @ModifiedBy
                            WHERE OperatorID = @Id";

            using (var conn = _context.CreateConnection())
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Id", entity.OperatorID);
                cmd.Parameters.AddWithValue("@NationalID", entity.NationalID);
                cmd.Parameters.AddWithValue("@FirstName", (object?)entity.FirstName ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@LastName", (object?)entity.LastName ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@HiringDate", (object?)entity.HiringDate ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ModifiedBy", _currentUserService.ActorId);

                await conn.OpenAsync();
                return await cmd.ExecuteNonQueryAsync() > 0;
            }
        }
    }
}
