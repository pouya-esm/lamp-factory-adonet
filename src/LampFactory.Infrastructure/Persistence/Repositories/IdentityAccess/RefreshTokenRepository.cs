using LampFactory.Infrastructure.Persistence;
using LampFactory.Application.Abstractions.Persistence.IdentityAccess.Authentication;
using LampFactory.Domain.IdentityAccess;
using LampFactory.Infrastructure.Security;
using Microsoft.Data.SqlClient;

namespace LampFactory.Infrastructure.Persistence.Repositories.IdentityAccess
{
    public class RefreshTokenRepository : IRefreshTokenRepository
{
        private readonly ApplicationDbContext _context;

        public RefreshTokenRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddTokenAsync(RefreshToken token)
        {
            string query = @"
                INSERT INTO RefreshToken (Token, CreatedAt, ExpiryDate, IsRevoked, UserID)
                VALUES (@Token, @CreatedAt, @ExpiryDate, @IsRevoked, @UserID)";

            using var conn = _context.CreateConnection();
            using var cmd = new SqlCommand(query, conn);

            cmd.Parameters.AddWithValue("@Token", token.Token);
            cmd.Parameters.AddWithValue("@CreatedAt", token.CreatedAt);
            cmd.Parameters.AddWithValue("@ExpiryDate", token.ExpiryDate);
            cmd.Parameters.AddWithValue("@IsRevoked", token.IsRevoked);
            cmd.Parameters.AddWithValue("@UserID", token.UserID);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<RefreshToken?> GetTokenAsync(string token)
        {
            string query = "SELECT TokenID, Token, CreatedAt, ExpiryDate, IsRevoked, UserID FROM RefreshToken WHERE Token = @Token";

            using var conn = _context.CreateConnection();
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Token", token);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new RefreshToken
                {
                    TokenID = reader.GetInt32(0),
                    Token = reader.GetString(1),
                    CreatedAt = reader.GetDateTime(2),
                    ExpiryDate = reader.GetDateTime(3),
                    IsRevoked = reader.GetBoolean(4),
                    UserID = reader.GetInt32(5)
                };
            }
            return null;
        }

        public async Task RevokeTokenAsync(string token)
        {
            string query = "UPDATE RefreshToken SET IsRevoked = 1 WHERE Token = @Token";
            using var conn = _context.CreateConnection();
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Token", token);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task RevokeAllUserTokensAsync(int userId)
        {
            string query = "UPDATE RefreshToken SET IsRevoked = 1 WHERE UserID = @UserID";
            using var conn = _context.CreateConnection();
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@UserID", userId);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }
    }

}
