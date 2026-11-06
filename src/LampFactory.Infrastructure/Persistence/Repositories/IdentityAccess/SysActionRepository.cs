using LampFactory.Infrastructure.Persistence;
using LampFactory.Application.Abstractions.Persistence.IdentityAccess.SysActions;
using LampFactory.Domain.IdentityAccess;
using LampFactory.Infrastructure.Security;
using Microsoft.Data.SqlClient;

namespace LampFactory.Infrastructure.Persistence.Repositories.IdentityAccess
{
    public class SysActionRepository : ISysActionRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public SysActionRepository(ApplicationDbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<List<SysAction>> GetAllAsync()
        {
            var actions = new List<SysAction>();
            string query = "SELECT ActionID, Name, CreatedAt, CreatedBy, ModifiedAt, ModifiedBy FROM [Action]";

            using (var conn = _context.CreateConnection())
            using (var cmd = new SqlCommand(query, conn))
            {
                await conn.OpenAsync();
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        actions.Add(new SysAction
                        {
                            ActionID = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            CreatedAt = reader.IsDBNull(2) ? null : reader.GetDateTime(2),
                            CreatedBy = reader.IsDBNull(3) ? null : reader.GetInt32(3),
                            ModifiedAt = reader.IsDBNull(4) ? null : reader.GetDateTime(4),
                            ModifiedBy = reader.IsDBNull(5) ? null : reader.GetInt32(5)
                        });
                    }
                }
            }
            return actions;
        }

        public async Task<SysAction?> GetByIdAsync(int id)
        {
            string query = "SELECT ActionID, Name, CreatedAt, CreatedBy, ModifiedAt, ModifiedBy FROM [Action] WHERE ActionID = @Id";

            using (var conn = _context.CreateConnection())
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                await conn.OpenAsync();

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return new SysAction
                        {
                            ActionID = reader.GetInt32(0),
                            Name = reader.GetString(1),
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

        public async Task<int> CreateAsync(SysAction sysAction)
        {
            string query = "INSERT INTO [Action] (Name, CreatedBy) VALUES (@Name, @CreatedBy); SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using (var conn = _context.CreateConnection())
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Name", sysAction.Name);
                cmd.Parameters.AddWithValue("@CreatedBy", _currentUserService.ActorId);
                await conn.OpenAsync();
                var result = await cmd.ExecuteScalarAsync();
                return result != null && result != DBNull.Value ? Convert.ToInt32(result) : 0;
            }
        }

        public async Task<bool> UpdateAsync(SysAction sysAction)
        {
            string query = "UPDATE [Action] SET Name = @Name, ModifiedAt = GETDATE(), ModifiedBy = @ModifiedBy WHERE ActionID = @ActionID";

            using (var conn = _context.CreateConnection())
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@ActionID", sysAction.ActionID);
                cmd.Parameters.AddWithValue("@Name", sysAction.Name);
                cmd.Parameters.AddWithValue("@ModifiedBy", _currentUserService.ActorId);
                await conn.OpenAsync();
                return await cmd.ExecuteNonQueryAsync() > 0;
            }
        }

        public async Task<bool> RemoveAsync(int id)
        {
            string query = "DELETE FROM [Action] WHERE ActionID = @Id";

            using (var conn = _context.CreateConnection())
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                await conn.OpenAsync();
                return await cmd.ExecuteNonQueryAsync() > 0;
            }
        }
    }

}
