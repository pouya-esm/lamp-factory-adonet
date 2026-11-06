using LampFactory.Application.Abstractions.Security;

using LampFactory.Application.Features.IdentityAccess.Authentication;
using LampFactory.Application.Features.IdentityAccess.Users;
using LampFactory.Application.Abstractions.Persistence.IdentityAccess.Authentication;
using LampFactory.Application.Abstractions.Persistence.IdentityAccess.Users;

namespace LampFactory.Infrastructure.Services.IdentityAccess.Authentication;

// Coordinates login and refresh-token rotation. Token mechanics are delegated
// to ITokenService; this service owns the user lookup and token bookkeeping.
public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ITokenService _tokenService;

    public AuthService(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        ITokenService tokenService)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _tokenService = tokenService;
    }

    public async Task<AuthResponseDTO?> LoginAsync(LoginDTO model)
    {
        var users = await _userRepository.GetAllAsync();
        var user = users.FirstOrDefault(u => u.Username == model.Username && u.Userpass == model.Password);

        if (user == null) return null;

        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshTokenString = _tokenService.GenerateRefreshToken();

        var refreshToken = new RefreshToken
        {
            Token = refreshTokenString,
            CreatedAt = DateTime.UtcNow,
            ExpiryDate = DateTime.UtcNow.AddDays(7),
            IsRevoked = false,
            UserID = user.UserID
        };

        await _refreshTokenRepository.AddTokenAsync(refreshToken);

        return new AuthResponseDTO
        {
            AccessToken = accessToken,
            RefreshToken = refreshTokenString
        };
    }

    public async Task<AuthResponseDTO> RefreshTokensAsync(RefreshRequestDTO request)
    {
        var storedToken = await _refreshTokenRepository.GetTokenAsync(request.RefreshToken);

        if (storedToken == null)
            throw new UnauthorizedAccessException("Token not found.");

        if (storedToken.ExpiryDate < DateTime.UtcNow)
            throw new UnauthorizedAccessException("Refresh Token expired. Please login again.");

        // Intrusion detection: a burnt token being presented again means someone
        // reused it. Revoke every session for that user immediately.
        if (storedToken.IsRevoked)
        {
            await _refreshTokenRepository.RevokeAllUserTokensAsync(storedToken.UserID);
            throw new UnauthorizedAccessException("An unauthorized attempt was detected. All your sessions have been terminated.");
        }

        // Rotate the presented token so each refresh is single-use.
        await _refreshTokenRepository.RevokeTokenAsync(storedToken.Token);

        var users = await _userRepository.GetAllAsync();
        var user = users.FirstOrDefault(u => u.UserID == storedToken.UserID);
        if (user == null) throw new UnauthorizedAccessException("User not found.");

        var newAccessToken = _tokenService.GenerateAccessToken(user);
        var newRefreshTokenString = _tokenService.GenerateRefreshToken();

        var newRefreshToken = new RefreshToken
        {
            Token = newRefreshTokenString,
            CreatedAt = DateTime.UtcNow,
            ExpiryDate = DateTime.UtcNow.AddDays(7),
            IsRevoked = false,
            UserID = user.UserID
        };

        await _refreshTokenRepository.AddTokenAsync(newRefreshToken);

        return new AuthResponseDTO
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshTokenString
        };
    }
}
