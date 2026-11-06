using LampFactory.Infrastructure.Persistence;
using LampFactory.Application.Abstractions.Persistence.IdentityAccess.Users;
using LampFactory.Domain.IdentityAccess;
using LampFactory.Infrastructure.Security;
using Microsoft.Data.SqlClient;

namespace LampFactory.Infrastructure.Persistence.Repositories.IdentityAccess
{
    public class UserRepository : IUserRepository
{
        private readonly ApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public UserRepository(ApplicationDbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<int> CreateWithRolesAsync(User user, List<int> roleIds)
        {
            using (var conn = _context.CreateConnection())
            {
                await conn.OpenAsync();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        string userQuery = @"INSERT INTO [User] (Username, Userpass, Fullname, Phone, CreatedBy) 
                                            VALUES (@Username, @Userpass, @Fullname, @Phone, @CreatedBy);
                                            SELECT SCOPE_IDENTITY();";

                        int newUserId;
                        using (var cmd = new SqlCommand(userQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@Username", user.Username);
                            cmd.Parameters.AddWithValue("@Userpass", user.Userpass);
                            cmd.Parameters.AddWithValue("@Fullname", (object?)user.Fullname ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@Phone", (object?)user.Phone ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@CreatedBy", _currentUserService.ActorId);

                            var result = await cmd.ExecuteScalarAsync();
                            newUserId = Convert.ToInt32(result);
                        }

                        if (roleIds != null && roleIds.Count > 0)
                        {
                            string roleQuery = "INSERT INTO UserRoles (UserID, RoleID) VALUES (@UserID, @RoleID)";
                            foreach (var roleId in roleIds)
                            {
                                using (var cmd = new SqlCommand(roleQuery, conn, transaction))
                                {
                                    cmd.Parameters.AddWithValue("@UserID", newUserId);
                                    cmd.Parameters.AddWithValue("@RoleID", roleId);
                                    await cmd.ExecuteNonQueryAsync();
                                }
                            }
                        }

                        await transaction.CommitAsync();
                        return newUserId;
                    }
                    catch
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                }
            }
        }

        public async Task<bool> UpdateWithRolesAsync(User user, List<int> roleIds)
        {
            using (var conn = _context.CreateConnection())
            {
                await conn.OpenAsync();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        string updateQuery = @"UPDATE [User] 
                                              SET Username = @Username, Userpass = @Userpass, 
                                                  Fullname = @Fullname, Phone = @Phone,
                                                  ModifiedAt = GETDATE(), ModifiedBy = @ModifiedBy 
                                              WHERE UserID = @UserID";

                        using (var cmd = new SqlCommand(updateQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@UserID", user.UserID);
                            cmd.Parameters.AddWithValue("@Username", user.Username);
                            cmd.Parameters.AddWithValue("@Userpass", user.Userpass);
                            cmd.Parameters.AddWithValue("@Fullname", (object?)user.Fullname ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@Phone", (object?)user.Phone ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@ModifiedBy", _currentUserService.ActorId);
                            await cmd.ExecuteNonQueryAsync();
                        }

                        string deleteRolesQuery = "DELETE FROM UserRoles WHERE UserID = @UserID";
                        using (var cmdDel = new SqlCommand(deleteRolesQuery, conn, transaction))
                        {
                            cmdDel.Parameters.AddWithValue("@UserID", user.UserID);
                            await cmdDel.ExecuteNonQueryAsync();
                        }

                        if (roleIds != null && roleIds.Count > 0)
                        {
                            string insertRoleQuery = "INSERT INTO UserRoles (UserID, RoleID) VALUES (@UserID, @RoleID)";
                            foreach (var roleId in roleIds)
                            {
                                using (var cmdIns = new SqlCommand(insertRoleQuery, conn, transaction))
                                {
                                    cmdIns.Parameters.AddWithValue("@UserID", user.UserID);
                                    cmdIns.Parameters.AddWithValue("@RoleID", roleId);
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

        public async Task<List<User>> GetAllAsync()
        {
            var users = new List<User>();
            string query = "SELECT UserID, Username, Userpass, Fullname, Phone, CreatedAt, CreatedBy, ModifiedAt, ModifiedBy FROM [User]";

            using (var conn = _context.CreateConnection())
            using (var cmd = new SqlCommand(query, conn))
            {
                await conn.OpenAsync();
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        users.Add(new User
                        {
                            UserID = reader.GetInt32(0),
                            Username = reader.GetString(1),
                            Userpass = reader.GetString(2),
                            Fullname = reader.IsDBNull(3) ? null : reader.GetString(3),
                            Phone = reader.IsDBNull(4) ? null : reader.GetString(4),
                            CreatedAt = reader.IsDBNull(5) ? null : reader.GetDateTime(5),
                            CreatedBy = reader.IsDBNull(6) ? null : reader.GetInt32(6),
                            ModifiedAt = reader.IsDBNull(7) ? null : reader.GetDateTime(7),
                            ModifiedBy = reader.IsDBNull(8) ? null : reader.GetInt32(8)
                        });
                    }
                }
            }
            return users;
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            string query = "SELECT UserID, Username, Userpass, Fullname, Phone, CreatedAt, CreatedBy, ModifiedAt, ModifiedBy FROM [User] WHERE UserID = @Id";

            using (var conn = _context.CreateConnection())
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                await conn.OpenAsync();

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return new User
                        {
                            UserID = reader.GetInt32(0),
                            Username = reader.GetString(1),
                            Userpass = reader.GetString(2),
                            Fullname = reader.IsDBNull(3) ? null : reader.GetString(3),
                            Phone = reader.IsDBNull(4) ? null : reader.GetString(4),
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

        // Lightweight id -> username projection used only to render audit stamps.
        // Deliberately excludes passwords and is reachable by any authenticated
        // user (no [HasPermission]) so non-admin modules can resolve actor names.
        public async Task<List<(int UserId, string Username)>> GetLookupAsync()
        {
            var users = new List<(int, string)>();
            string query = "SELECT UserID, Username FROM [User] ORDER BY UserID";

            using (var conn = _context.CreateConnection())
            using (var cmd = new SqlCommand(query, conn))
            {
                await conn.OpenAsync();
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        users.Add((reader.GetInt32(0), reader.GetString(1)));
                    }
                }
            }
            return users;
        }

        public async Task<List<int>> GetRoleIdsForUserAsync(int userId)
        {
            var roleIds = new List<int>();
            string query = "SELECT RoleID FROM UserRoles WHERE UserID = @UserID";

            using (var conn = _context.CreateConnection())
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@UserID", userId);
                await conn.OpenAsync();
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        roleIds.Add(reader.GetInt32(0));
                    }
                }
            }
            return roleIds;
        }

        public async Task<bool> RemoveAsync(int id)
        {
            string query = "DELETE FROM [User] WHERE UserID = @Id";

            using (var conn = _context.CreateConnection())
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                await conn.OpenAsync();
                return await cmd.ExecuteNonQueryAsync() > 0;
            }
        }
        // IRepository<User> members: the role-linked variants above are the ones
        // the application services prefer; these cover the plain entity cases.
        public async Task<int> CreateAsync(User entity)
        {
            string query = @"INSERT INTO [User] (Username, Userpass, Fullname, Phone, CreatedBy)
                             VALUES (@Username, @Userpass, @Fullname, @Phone, @CreatedBy);
                             SELECT SCOPE_IDENTITY();";

            using var conn = _context.CreateConnection();
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Username", entity.Username);
            cmd.Parameters.AddWithValue("@Userpass", entity.Userpass);
            cmd.Parameters.AddWithValue("@Fullname", (object?)entity.Fullname ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Phone", (object?)entity.Phone ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@CreatedBy", _currentUserService.ActorId);

            await conn.OpenAsync();
            var result = await cmd.ExecuteScalarAsync();
            return result != null ? Convert.ToInt32(result) : 0;
        }

        public async Task<bool> UpdateAsync(User entity)
        {
            string query = @"UPDATE [User]
                             SET Username = @Username, Userpass = @Userpass,
                                 Fullname = @Fullname, Phone = @Phone
                             WHERE UserID = @Id";

            using var conn = _context.CreateConnection();
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Id", entity.UserID);
            cmd.Parameters.AddWithValue("@Username", entity.Username);
            cmd.Parameters.AddWithValue("@Userpass", entity.Userpass);
            cmd.Parameters.AddWithValue("@Fullname", (object?)entity.Fullname ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Phone", (object?)entity.Phone ?? DBNull.Value);

            await conn.OpenAsync();
            return await cmd.ExecuteNonQueryAsync() > 0;
        }
    }
}
