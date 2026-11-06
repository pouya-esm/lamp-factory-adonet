

namespace LampFactory.Application.Abstractions.Security;

// Issues and validates access/refresh tokens. The JWT implementation lives in
// Infrastructure; this interface keeps token mechanics out of the application.
public interface ITokenService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
}
