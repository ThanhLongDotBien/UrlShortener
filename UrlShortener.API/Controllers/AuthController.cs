using Microsoft.AspNetCore.Mvc;
using UrlShortener.Common.DTOs;
using UrlShortener.Services.Interfaces;

namespace UrlShortener.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequestDto request)
        {
            var result = await _authService.Register(request);

            if (result.Contains("success", StringComparison.OrdinalIgnoreCase))
                return Ok(new { message = result });

            return BadRequest(new { message = result });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequestDto request)
        {
            var result = await _authService.Login(request);

            if (result.Contains("success", StringComparison.OrdinalIgnoreCase))
                return Ok(new { message = result });

            return Unauthorized(new { message = result });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _authService.Logout();
            return Ok(new { message = "Logout success" });
        }
    }
}