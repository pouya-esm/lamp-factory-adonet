using LampFactory.Infrastructure.Persistence;
using LampFactory.Application.Abstractions.Persistence.IdentityAccess.Resources;
using LampFactory.Domain.IdentityAccess;
using LampFactory.Infrastructure.Security;
using Microsoft.Data.SqlClient;

namespace LampFactory.Infrastructure.Persistence.Repositories.IdentityAccess
{
    public class ResourceRepository : IResourceRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public ResourceRepository(ApplicationDbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<List<Resource>> GetAllAsync()
        {
            var resources = new List<Resource>();
            string query = "SELECT ResourceID, Name, CreatedAt, CreatedBy, ModifiedAt, ModifiedBy FROM [Resource]";

            using (var conn = _context.CreateConnection())
            using (var cmd = new SqlCommand(query, conn))
            {
                await conn.OpenAsync();
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        resources.Add(new Resource
                        {
                            ResourceID = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            CreatedAt = reader.IsDBNull(2) ? null : reader.GetDateTime(2),
                            CreatedBy = reader.IsDBNull(3) ? null : reader.GetInt32(3),
                            ModifiedAt = reader.IsDBNull(4) ? null : reader.GetDateTime(4),
                            ModifiedBy = reader.IsDBNull(5) ? null : reader.GetInt32(5)
                        });
                    }
                }
            }
            return resources;
        }

        public async Task<Resource?> GetByIdAsync(int id)
        {
            string query = "SELECT ResourceID, Name, CreatedAt, CreatedBy, ModifiedAt, ModifiedBy FROM [Resource] WHERE ResourceID = @Id";

            using (var conn = _context.CreateConnection())
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                await conn.OpenAsync();

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return new Resource
                        {
                            ResourceID = reader.GetInt32(0),
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

        public async Task<int> CreateAsync(Resource resource)
        {
            string query = "INSERT INTO [Resource] (Name, CreatedBy) VALUES (@Name, @CreatedBy); SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using (var conn = _context.CreateConnection())
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Name", resource.Name);
                cmd.Parameters.AddWithValue("@CreatedBy", _currentUserService.ActorId);
                await conn.OpenAsync();
                var result = await cmd.ExecuteScalarAsync();
                return result != null && result != DBNull.Value ? Convert.ToInt32(result) : 0;
            }
        }

        public async Task<bool> UpdateAsync(Resource resource)
        {
            string query = "UPDATE [Resource] SET Name = @Name, ModifiedAt = GETDATE(), ModifiedBy = @ModifiedBy WHERE ResourceID = @ResourceID";

            using (var conn = _context.CreateConnection())
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@ResourceID", resource.ResourceID);
                cmd.Parameters.AddWithValue("@Name", resource.Name);
                cmd.Parameters.AddWithValue("@ModifiedBy", _currentUserService.ActorId);
                await conn.OpenAsync();
                return await cmd.ExecuteNonQueryAsync() > 0;
            }
        }

        public async Task<bool> RemoveAsync(int id)
        {
            string query = "DELETE FROM [Resource] WHERE ResourceID = @Id";

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
