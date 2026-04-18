using Application.DTOs.Auth;
using Application.DTOs.Services.WalletSignature;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        private readonly IAuthService _authService = authService;

        [EnableRateLimiting("fixed")]
        [HttpPost("email-verify")]
        public async Task<IActionResult> EmailVerify([FromBody] EmailVerifyDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (string.IsNullOrEmpty(dto.Email))
                    return BadRequest("Email is required");

                var ws = dto.WalletSignature;
                if (string.IsNullOrEmpty(ws?.Address) ||
                    string.IsNullOrEmpty(ws.Signature) ||
                    string.IsNullOrEmpty(ws.Message)
                   )
                    return BadRequest("Wallet signature information is incomplete.");

                return Ok(new
                {
                    UserId = await _authService.VerifyEmailAsync(dto)
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [EnableRateLimiting("fixed")]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] WalletSignatureDto ws)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (string.IsNullOrEmpty(ws?.Address) ||
                    string.IsNullOrEmpty(ws.Signature) ||
                    string.IsNullOrEmpty(ws.Message))
                {
                    return BadRequest("Wallet signature information is incomplete.");
                }

                var loginDto = await _authService.LoginAsync(ws);

                return Ok(new
                {
                    loginDto.UserId,
                    loginDto.AccessToken,
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [EnableRateLimiting("fixed")]
        [Authorize]
        [HttpDelete("logout")]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                    return Unauthorized();

                await _authService.RevokeRefreshTokenAsync(userId);

                return Ok("Logged out");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [EnableRateLimiting("fixed")]
        [HttpPost("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromBody] EmailConfirmWithOTP client, [FromQuery] string userId)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                await _authService.ConfirmEmailAsync(client, userId);
                return Ok("Xác thực email thành công");
            }
            catch (Exception ex)
            {
                return BadRequest($"Đã có lỗi xảy ra trong quá trình xác thực Email {ex.Message}");
            }
        }

        [EnableRateLimiting("fixed")]
        [Authorize]
        [HttpGet("refresh")]
        public async Task<IActionResult> Refresh()
        {
            try
            {
                if (!Request.Cookies.TryGetValue("refreshToken", out var refreshToken) ||
                string.IsNullOrEmpty(refreshToken))
                    return Unauthorized();

                var jti = User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
                if (string.IsNullOrEmpty(jti))
                    return Unauthorized();

                if (await _authService.IsTokenBlacklistedAsync(jti))
                    return Unauthorized();

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                string? oldRefreshToken = await _authService.GetRefreshTokenAsync(userId);
                if (string.IsNullOrEmpty(oldRefreshToken) || refreshToken != oldRefreshToken)
                    return Unauthorized();

                var authHeader = Request.Headers.Authorization.ToString();
                if (string.IsNullOrEmpty(authHeader))
                    return Unauthorized();
                string oldAccessToken = "";

                if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    oldAccessToken = authHeader["Bearer ".Length..].Trim();
                }

                var response = await _authService.RefreshTokenAsync(userId, oldAccessToken);

                return Ok(new
                {
                    response.AccessToken,
                });
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }
        }
    }
}
