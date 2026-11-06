

using LampFactory.Application.Features.IdentityAccess.Authentication;

namespace LampFactory.Application.Features.IdentityAccess.Authentication
{
    public class AuthResponseDTO
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }

    public class RefreshRequestDTO
    {
        public string RefreshToken { get; set; } = string.Empty;
    }
}
