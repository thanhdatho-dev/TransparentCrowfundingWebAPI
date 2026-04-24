using API.Models.Requests.Auth;
using Application.Features.Auth.Commands.ConfirmEmail;
using Application.Features.Auth.Commands.Login;
using Application.Features.Auth.Commands.Logout;
using Application.Features.Auth.Commands.Refresh;
using Application.Features.Auth.Commands.VerifyEmail;
using Application.Features.Auth.Queries.CheckWalletRegistered;
using Application.Features.Auth.Queries.GetCurrentUser;
using Application.Interfaces.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(ISender sender, ICacheService cache) : ControllerBase
    {
        private readonly ISender _sender = sender;
        private readonly ICacheService _cache = cache;

        [EnableRateLimiting("fixed")]
        [HttpPost("email-verify")]
        public async Task<IActionResult> EmailVerify([FromBody] VerifyEmailRequest request, CancellationToken ct)
        {
            var command = new VerifyEmailCommand
            (
                request.Email,
                request.WalletAddress,
                request.Message,
                request.Signature
            );

            await _sender.Send(command, ct);
            return Ok("OTP sent to email");
        }

        [EnableRateLimiting("fixed")]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
        {
            var command = new LoginCommand(request.WalletAddress, request.Message, request.Signature);

            var result = await _sender.Send(command, ct);

            Response.Cookies.Append("refreshToken", result.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = result.ExpiryRefreshTokenTime,
                Path = "/"
            });

            return Ok(new { result.UserId, result.AccessToken });
        }

        [EnableRateLimiting("fixed")]
        [Authorize]
        [HttpDelete("logout")]
        public async Task<IActionResult> Logout(CancellationToken ct)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var jti = User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
            if (string.IsNullOrEmpty(jti))
                return Unauthorized();

            var authHeader = Request.Headers.Authorization.ToString();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                return Unauthorized();

            var accessToken = authHeader["Bearer ".Length..].Trim();

            var command = new LogoutCommand(userId, jti, accessToken);
            await _sender.Send(command, ct);

            Response.Cookies.Delete("refreshToken");
            return Ok("Logged out");
        }

        [EnableRateLimiting("fixed")]
        [HttpPost("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailRequest request, CancellationToken ct)
        {
            var command = new ConfirmEmailCommand(request.Email, request.WalletAddress, request.OTP);

            await _sender.Send(command, ct);

            return Ok("Verify email successfully");
        }

        [EnableRateLimiting("fixed")]
        [Authorize]
        [HttpGet("refresh")]
        public async Task<IActionResult> Refresh(CancellationToken ct)
        {
            if (!Request.Cookies.TryGetValue("refreshToken", out var refreshToken) ||
                string.IsNullOrEmpty(refreshToken))
                return Unauthorized();

            var jti = User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
            if (string.IsNullOrEmpty(jti))
                return Unauthorized();

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var authHeader = Request.Headers.Authorization.ToString();
            if (string.IsNullOrEmpty(authHeader))
                return Unauthorized();
            string oldAccessToken = "";

            if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                oldAccessToken = authHeader["Bearer ".Length..].Trim();
            }

            var command = new RefreshCommand(jti, userId, oldAccessToken, refreshToken);

            var result = await _sender.Send(command, ct);

            Response.Cookies.Append("refreshToken", result.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = result.RefreshTokenExiryTime,
                Path = "/"
            });

            return Ok(new
            {
                result.AccessToken
            });
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser(CancellationToken ct)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var query = new GetCurrentUserQuery(userId);
            var result = await _sender.Send(query, ct);
            return Ok(result);
        }

        [HttpGet("check-wallet/{walletAddress}")]
        public async Task<IActionResult> CheckWalletRegistered(string walletAddress, CancellationToken ct)
        {
            var query = new CheckWalletRegisteredQuery(walletAddress);
            var isRegistered = await _sender.Send(query, ct);
            return Ok(new { IsRegistered = isRegistered });
        }
    }
}
