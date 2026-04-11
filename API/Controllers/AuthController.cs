using Application.Interfaces.Services;
using Domain.DTOs.Auth;
using Domain.DTOs.Services.MailSender;
using Domain.DTOs.Services.WalletSignature;
using Domain.Entities;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(
        IMailService mailService, 
        UserManager<User> userManager, 
        IConfiguration config,
        IWalletSignatureVerifier signatureVerifier,
        ITokenService tokenService) : ControllerBase
    {
        private readonly IMailService _mailService = mailService;
        private readonly UserManager<User> _userManager = userManager;
        private readonly IConfiguration _config = config;
        private readonly IWalletSignatureVerifier _signatureVerifier = signatureVerifier;
        private readonly ITokenService _tokenService = tokenService;

        [EnableRateLimiting("fixed")]
        [HttpPost("email-verify")]
        public async Task<IActionResult> EmailVerify([FromBody] EmailVerifyDto emailVerifyDto)
        {
            try
            {
                // Signature đã verify ở SignIn Controller, không cần verify lại
                var walletSignature = emailVerifyDto.WalletSignature;
                if (walletSignature == null)
                    return BadRequest("Wallet Signature is missing");
                // Verify Email sử dụng AWS SES làm sender
                string? email = emailVerifyDto.Email;
                if (string.IsNullOrEmpty(email))
                    return BadRequest("Email is missing");

                var user = await _userManager.FindByNameAsync(walletSignature.Address);
                if (user == null)
                {
                    user = new User
                    {
                        UserName = walletSignature.Address,
                        Email = email
                    };
                    var createUser = await _userManager.CreateAsync(user);
                    if (!createUser.Succeeded)
                        return BadRequest(createUser.Errors);
                }

                // Tạo Token xác thực Email
                var emailToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                if (emailToken == null) return NotFound("Không thể tạo token xác thực email");
                var endcodeToken = WebUtility.UrlEncode(emailToken); //Mã hóa

                //Gửi link xác nhận trỏ tới API endpoint
                var confirmUrl = $"{_config["ApiBaseUrl"]}/api/Auth/confirm-email?userId={user.Id}&token={endcodeToken}";

                var mailRequest = new MailRequest
                {
                    ToEmail = email,
                    Subject = "Verify Email Message",
                    Body = $"<p>Nhấn vào <a href='{confirmUrl}'>đây</a> để xác nhận email.</p>"
                };

                await _mailService.SendEmailAsync(mailRequest);
                await _userManager.UpdateAsync(user);
                return Ok(confirmUrl);                
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [EnableRateLimiting("fixed")]
        [HttpPost("sign-in")]
        public async Task<IActionResult> SignIn([FromBody] WalletSignatureDto walletSignatureDto)
        {
            try
            {
                bool signatureVerifyStatus = _signatureVerifier.VerifySignature(walletSignatureDto.Message, walletSignatureDto.Signature, walletSignatureDto.Address);
                if (!signatureVerifyStatus)
                {
                    return BadRequest(new
                    {
                        ErrorCode = "WalletVerificationFailed",
                        Message = "Wallet verification failed",
                        NextAction = "GoHome"
                    });
                }


                var user = await _userManager.Users.FirstOrDefaultAsync(x => x.UserName == walletSignatureDto.Address);
                if (user == null || !user.EmailConfirmed)
                {
                    return BadRequest(new
                    {
                        ErrorCode = "EmailVerificationRequired",
                        Message = "Email verification needed",
                        NextAction = "VerifyEmail"
                    });
                }

                var accesToken = _tokenService.GenerateAccessToken(user);
                var refreshToken = _tokenService.GenerateRefreshToken();

                user.RefreshToken = refreshToken;
                var refreshTokenExpiryTime = user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

                SetRefreshTokenCookie(refreshToken, refreshTokenExpiryTime);

                await _userManager.UpdateAsync(user);

                return Ok(new
                {
                    UserId = user.Id,
                    AccessTokne = accesToken,
                });
            }catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        
        [EnableRateLimiting("fixed")]
        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (userId == null || token == null)
                    return BadRequest("email hoặc token không hợp lệ");

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return BadRequest("Người dùng không tồn tại");

                var result = await _userManager.ConfirmEmailAsync(user, token);
                if (result.Succeeded)
                {
                    var accesToken = _tokenService.GenerateAccessToken(user);
                    var refreshToken = _tokenService.GenerateRefreshToken();

                    user.RefreshToken = refreshToken;
                    var refreshTokenExpiryTime = user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

                    SetRefreshTokenCookie(refreshToken, refreshTokenExpiryTime);
                    await _userManager.UpdateAsync(user);
                    return Ok(new
                    {
                        UserId = user.Id,
                        AccessTokne = accesToken,
                    }); 
                }
                else
                {
                    return BadRequest("Xác thực Email thất bại");
                }
            }
            catch (Exception ex)
            {
                return BadRequest($"Đã có lỗi xảy ra trong quá trình xác thực Email {ex.Message}");
            }
        }

        
        private void SetRefreshTokenCookie(string refreshToken, DateTime? expires)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = expires,
                IsEssential = true,
                Path = "/"
            };

            Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
        }
    }
}
