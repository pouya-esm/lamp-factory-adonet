using LampFactory.Infrastructure.Persistence;
using LampFactory.Application.Abstractions.Persistence.IdentityAccess.Roles;
using LampFactory.Domain.IdentityAccess;
using LampFactory.Infrastructure.Security;
using Microsoft.Data.SqlClient;

namespace LampFactory.Infrastructure.Persistence.Repositories.IdentityAccess
{
    public class RoleRepository : IRoleRepository
{
        private readonly ApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public RoleRepository(ApplicationDbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

         public async Task<int> CreateWithPermissionsAsync(string roleName, string? description, List<int> permissionIds)
        {
            using (var conn = _context.CreateConnection())
            {
                await conn.OpenAsync();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        string roleQuery = "INSERT INTO [Role] (RoleName, Description, CreatedBy) VALUES (@RoleName, @Description, @CreatedBy); SELECT SCOPE_IDENTITY();";
                        int newRoleId;

                        using (var cmd = new SqlCommand(roleQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@RoleName", roleName);
                            cmd.Parameters.AddWithValue("@Description", (object?)description ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@CreatedBy", _currentUserService.ActorId);
                            var result = await cmd.ExecuteScalarAsync();
                            newRoleId = Convert.ToInt32(result);
                        }

                        if (permissionIds != null && permissionIds.Count > 0)
                        {
                            string permQuery = "INSERT INTO RolePermissions (RoleID, PermissionID) VALUES (@RoleID, @PermissionID)";
                            foreach (var permId in permissionIds)
                            {
                                using (var cmd = new SqlCommand(permQuery, conn, transaction))
                                {
                                    cmd.Parameters.AddWithValue("@RoleID", newRoleId);
                                    cmd.Parameters.AddWithValue("@PermissionID", permId);
                                    await cmd.ExecuteNonQueryAsync();
                                }
                            }
                        }

                        await transaction.CommitAsync();
                        return newRoleId;
                    }
                    catch
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                }
            }
        }

        public async Task<bool> UpdateWithPermissionsAsync(int roleId, string roleName, string? description, List<int> permissionIds)
        {
            using (var conn = _context.CreateConnection())
            {
                await conn.OpenAsync();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        string updateQuery = "UPDATE [Role] SET RoleName = @RoleName, Description = @Description, ModifiedAt = GETDATE(), ModifiedBy = @ModifiedBy WHERE RoleID = @RoleID";
                        using (var cmd = new SqlCommand(updateQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@RoleID", roleId);
                            cmd.Parameters.AddWithValue("@RoleName", roleName);
                            cmd.Parameters.AddWithValue("@Description", (object?)description ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@ModifiedBy", _currentUserService.ActorId);
                            await cmd.ExecuteNonQueryAsync();
                        }

                        string deletePermsQuery = "DELETE FROM RolePermissions WHERE RoleID = @RoleID";
                        using (var cmdDel = new SqlCommand(deletePermsQuery, conn, transaction))
                        {
                            cmdDel.Parameters.AddWithValue("@RoleID", roleId);
                            await cmdDel.ExecuteNonQueryAsync();
                        }

                        if (permissionIds != null && permissionIds.Count > 0)
                        {
                            string insertPermQuery = "INSERT INTO RolePermissions (RoleID, PermissionID) VALUES (@RoleID, @PermissionID)";
                            foreach (var permId in permissionIds)
                            {
                                using (var cmdIns = new SqlCommand(insertPermQuery, conn, transaction))
                                {
                                    cmdIns.Parameters.AddWithValue("@RoleID", roleId);
                                    cmdIns.Parameters.AddWithValue("@PermissionID", permId);
                                    await cmdIns.ExecuteNonQueryAsync();
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

        public async Task<List<Role>> GetAllAsync()
        {
            var roles = new List<Role>();
            string query = "SELECT RoleID, RoleName, Description, CreatedAt, CreatedBy, ModifiedAt, ModifiedBy FROM [Role]";

            using (var conn = _context.CreateConnection())
            using (var cmd = new SqlCommand(query, conn))
            {
                await conn.OpenAsync();
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        roles.Add(new Role
                        {
                            RoleID = reader.GetInt32(0),
                            RoleName = reader.GetString(1),
                            Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                            CreatedAt = reader.IsDBNull(3) ? null : reader.GetDateTime(3),
                            CreatedBy = reader.IsDBNull(4) ? null : reader.GetInt32(4),
                            ModifiedAt = reader.IsDBNull(5) ? null : reader.GetDateTime(5),
                            ModifiedBy = reader.IsDBNull(6) ? null : reader.GetInt32(6)
                        });
                    }
                }
            }
            return roles;
        }

        public async Task<List<int>> GetPermissionIdsForRoleAsync(int roleId)
        {
            var permIds = new List<int>();
            string query = "SELECT PermissionID FROM RolePermissions WHERE RoleID = @RoleID";

            using (var conn = _context.CreateConnection())
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@RoleID", roleId);
                await conn.OpenAsync();
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        permIds.Add(reader.GetInt32(0));
                    }
                }
            }
            return permIds;
        }

        public async Task<bool> RemoveAsync(int id)
        {
            using (var conn = _context.CreateConnection())
            {
                await conn.OpenAsync();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        string deletePerms = "DELETE FROM RolePermissions WHERE RoleID = @Id";
                        using (var cmd1 = new SqlCommand(deletePerms, conn, transaction))
                        {
                            cmd1.Parameters.AddWithValue("@Id", id);
                            await cmd1.ExecuteNonQueryAsync();
                        }

                        string deleteRole = "DELETE FROM [Role] WHERE RoleID = @Id";
                        using (var cmd2 = new SqlCommand(deleteRole, conn, transaction))
                        {
                            cmd2.Parameters.AddWithValue("@Id", id);
                            int affected = await cmd2.ExecuteNonQueryAsync();

                            await transaction.CommitAsync();
                            return affected > 0;
                        }
                    }
                    catch
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                }
            }
        }
        // IRepository<Role> members: the role-only write paths above handle the
        // permission-link rows, so these cover the plain single-entity cases.
        public async Task<int> CreateAsync(Role entity)
        {
            string query = @"INSERT INTO [Role] (RoleName, Description)
                             VALUES (@RoleName, @Description);
                             SELECT SCOPE_IDENTITY();";

            using var conn = _context.CreateConnection();
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@RoleName", (object?)entity.RoleName ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Description", (object?)entity.Description ?? DBNull.Value);

            await conn.OpenAsync();
            var result = await cmd.ExecuteScalarAsync();
            return result != null ? Convert.ToInt32(result) : 0;
        }

        public async Task<bool> UpdateAsync(Role entity)
        {
            string query = @"UPDATE [Role]
                             SET RoleName = @RoleName, Description = @Description
                             WHERE RoleID = @Id";

            using var conn = _context.CreateConnection();
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Id", entity.RoleID);
            cmd.Parameters.AddWithValue("@RoleName", (object?)entity.RoleName ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Description", (object?)entity.Description ?? DBNull.Value);

            await conn.OpenAsync();
            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        public async Task<Role?> GetByIdAsync(int id)
        {
            string query = "SELECT RoleID, RoleName, Description FROM [Role] WHERE RoleID = @Id";

            using var conn = _context.CreateConnection();
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Id", id);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new Role
                {
                    RoleID = reader.GetInt32(0),
                    RoleName = reader.IsDBNull(1) ? null : reader.GetString(1),
                    Description = reader.IsDBNull(2) ? null : reader.GetString(2)
                };
            }
            return null;
        }
    }
}
