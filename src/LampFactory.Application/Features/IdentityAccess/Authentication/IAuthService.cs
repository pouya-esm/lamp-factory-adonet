
namespace LampFactory.Application.Features.IdentityAccess.Authentication;

// Application-level auth contract. The implementation coordinates user lookup,
// refresh-token rotation and token issue; the JWT mechanics sit behind
// ITokenService in Infrastructure.
public interface IAuthService
{
    Task<AuthResponseDTO?> LoginAsync(LoginDTO model);
    Task<AuthResponseDTO> RefreshTokensAsync(RefreshRequestDTO request);
}
