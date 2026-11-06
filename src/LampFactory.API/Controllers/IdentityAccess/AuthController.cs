using LampFactory.Application.Features.IdentityAccess.Authentication;
using Microsoft.AspNetCore.Mvc;
using LampFactory.Application.Common.Models;

namespace LampFactory.API.Controllers.IdentityAccess
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO model)
        {
            var result = await _authService.LoginAsync(model);

            if (result == null)
            {
                return Unauthorized(new { message = "Incorrect username or password." });
            }

            return Ok(result);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequestDTO request)
        {
            if (string.IsNullOrEmpty(request.RefreshToken))
            {
                return BadRequest("Refresh Token is not sent.");
            }

            try
            {
                // Returns the new token pair upon success.
                var result = await _authService.RefreshTokensAsync(request);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                // The service throws this exception if the token is invalid, expired, or stolen.
                return Unauthorized(new { message = ex.Message });
            }
        }

    }

}

