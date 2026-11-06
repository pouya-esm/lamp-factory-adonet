using LampFactory.Infrastructure.Persistence;
using LampFactory.Application.Abstractions.Persistence.IdentityAccess.Permissions;
using LampFactory.Domain.IdentityAccess;
using LampFactory.Infrastructure.Security;
using Microsoft.Data.SqlClient;

namespace LampFactory.Infrastructure.Persistence.Repositories.IdentityAccess
{
    public class PermissionRepository : IPermissionRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public PermissionRepository(ApplicationDbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        private int _actorId => _currentUserService.ActorId;

        // Returns the distinct resources the user can reach through any of their
        // roles' permissions. Drives the client sidebar, so it must stay dynamic:
        // changing a user's roles is reflected on their next login / refresh.
        public async Task<List<Resource>> GetAccessibleResourcesAsync(int userId)
        {
            var resources = new List<Resource>();
            string query = @"
                SELECT DISTINCT r.ResourceID, r.Name
                FROM UserRoles ur
                INNER JOIN RolePermissions rp ON ur.RoleID = rp.RoleID
                INNER JOIN [Permission] p ON rp.PermissionID = p.PermissionID
                INNER JOIN [Resource] r ON p.ResourceID = r.ResourceID
                WHERE ur.UserID = @UserID";

            using (var conn = _context.CreateConnection())
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@UserID", userId);

                await conn.OpenAsync();
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        resources.Add(new Resource
                        {
                            ResourceID = reader.GetInt32(0),
                            Name = reader.GetString(1)
                        });
                    }
                }
            }
            return resources;
        }

        public async Task<bool> CheckUserPermissionAsync(int userId, string resourceName, string actionName)
        {
            string query = @"
        SELECT TOP 1 1
        FROM UserRoles ur
        INNER JOIN RolePermissions rp ON ur.RoleID = rp.RoleID
        INNER JOIN [Permission] p ON rp.PermissionID = p.PermissionID
        INNER JOIN [Resource] r ON p.ResourceID = r.ResourceID
        INNER JOIN [Action] a ON p.ActionID = a.ActionID
        WHERE ur.UserID = @UserID 
          AND r.Name = @ResourceName 
          AND a.Name = @ActionName";

            using (var conn = _context.CreateConnection())
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@UserID", userId);
                cmd.Parameters.AddWithValue("@ResourceName", resourceName);
                cmd.Parameters.AddWithValue("@ActionName", actionName);

                await conn.OpenAsync();
                var result = await cmd.ExecuteScalarAsync();

                return result != null;
            }
        }

        public async Task<List<Permission>> GetAllAsync()
        {
            var permissions = new List<Permission>();
            string query = @"
                SELECT p.PermissionID, p.ResourceID, r.Name AS ResourceName, p.ActionID, a.Name AS ActionName,
                       p.CreatedAt, p.CreatedBy, p.ModifiedAt, p.ModifiedBy
                FROM [Permission] p
                INNER JOIN [Resource] r ON p.ResourceID = r.ResourceID
                INNER JOIN [Action] a ON p.ActionID = a.ActionID";

            using (var conn = _context.CreateConnection())
            using (var cmd = new SqlCommand(query, conn))
            {
                await conn.OpenAsync();
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        permissions.Add(new Permission
                        {
                            PermissionID = reader.GetInt32(0),
                            ResourceID = reader.GetInt32(1),
                            Resource = new Resource
                            {
                                ResourceID = reader.GetInt32(1),
                                Name = reader.GetString(2)
                            },
                            ActionID = reader.GetInt32(3),
                            Action = new SysAction
                            {
                                ActionID = reader.GetInt32(3),
                                Name = reader.GetString(4)
                            },
                            CreatedAt = reader.IsDBNull(5) ? null : reader.GetDateTime(5),
                            CreatedBy = reader.IsDBNull(6) ? null : reader.GetInt32(6),
                            ModifiedAt = reader.IsDBNull(7) ? null : reader.GetDateTime(7),
                            ModifiedBy = reader.IsDBNull(8) ? null : reader.GetInt32(8)
                        });
                    }
                }
            }
            return permissions;
        }

        public async Task<Permission?> GetByIdAsync(int id)
        {
            string query = @"
        SELECT p.PermissionID, p.ResourceID, r.Name AS ResourceName, p.ActionID, a.Name AS ActionName,
               p.CreatedAt, p.CreatedBy, p.ModifiedAt, p.ModifiedBy
        FROM [Permission] p
        INNER JOIN [Resource] r ON p.ResourceID = r.ResourceID
        INNER JOIN [Action] a ON p.ActionID = a.ActionID
        WHERE p.PermissionID = @Id";

            using (var conn = _context.CreateConnection())
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                await conn.OpenAsync();

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return new Permission
                        {
                            PermissionID = reader.GetInt32(0),
                            ResourceID = reader.GetInt32(1),
                            Resource = new Resource
                            {
                                ResourceID = reader.GetInt32(1),
                                Name = reader.GetString(2)
                            },
                            ActionID = reader.GetInt32(3),
                            Action = new SysAction
                            {
                                ActionID = reader.GetInt32(3),
                                Name = reader.GetString(4)
                            },
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

        public async Task<int> CreateAsync(Permission permission)
        {
            string query = "INSERT INTO [Permission] (ResourceID, ActionID, CreatedBy) VALUES (@ResourceID, @ActionID, @CreatedBy); SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using (var conn = _context.CreateConnection())
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@ResourceID", permission.ResourceID);
                cmd.Parameters.AddWithValue("@ActionID", permission.ActionID);
                cmd.Parameters.AddWithValue("@CreatedBy", _actorId);
                await conn.OpenAsync();
                var result = await cmd.ExecuteScalarAsync();
                return result != null && result != DBNull.Value ? Convert.ToInt32(result) : 0;
            }
        }

        public async Task<bool> UpdateAsync(Permission permission)
        {
            string query = "UPDATE [Permission] SET ResourceID = @ResourceID, ActionID = @ActionID, ModifiedAt = GETDATE(), ModifiedBy = @ModifiedBy WHERE PermissionID = @PermissionID";

            using (var conn = _context.CreateConnection())
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@PermissionID", permission.PermissionID);
                cmd.Parameters.AddWithValue("@ResourceID", permission.ResourceID);
                cmd.Parameters.AddWithValue("@ActionID", permission.ActionID);
                cmd.Parameters.AddWithValue("@ModifiedBy", _actorId);
                await conn.OpenAsync();
                return await cmd.ExecuteNonQueryAsync() > 0;
            }
        }

        public async Task<bool> RemoveAsync(int id)
        {
            string query = "DELETE FROM [Permission] WHERE PermissionID = @Id";

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
