

namespace LampFactory.Application.Abstractions.Persistence.IdentityAccess.Authentication;

public interface IRefreshTokenRepository
{
    Task AddTokenAsync(RefreshToken token);
    Task<RefreshToken?> GetTokenAsync(string token);
    Task RevokeTokenAsync(string token);
    Task RevokeAllUserTokensAsync(int userId);
}
