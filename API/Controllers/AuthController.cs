using Application.Interfaces.Services;
using Domain.DTOs.Auth;
using Domain.DTOs.Services.MailSender;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Net;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IMailService mailService, UserManager<User> userManager, IConfiguration config) : ControllerBase
    {
        private readonly IMailService _mailService = mailService;
        private readonly UserManager<User> _userManager = userManager;
        private readonly IConfiguration _config = config;

        [EnableRateLimiting("fixed")]
        [HttpPost("email-verify")]
        public async Task<IActionResult> EmailVerify([FromBody] EmailVerifyDto emailVerifyDto)
        {
            try
            {
                // Email đã tồn tại, 
                string? email = emailVerifyDto.Email;
                if (string.IsNullOrEmpty(email))
                    return BadRequest("Email is missing");

                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    user = new User
                    {
                        UserName = email,
                        Email = email
                    };
                    var createUser = await _userManager.CreateAsync(user);
                    if (!createUser.Succeeded)
                        return StatusCode(400, createUser.Errors);
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
                return Ok("Đăng ký thành công, vui lòng xác nhận email.");                
            }
            catch (Exception ex)
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
                    return StatusCode(400, "email hoặc token không hợp lệ");

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return StatusCode(400, "Người dùng không tồn tại");

                var result = await _userManager.ConfirmEmailAsync(user, token);
                if (result.Succeeded)
                {
                    return Ok("Xác thực Email thành công");
                }
                else
                {
                    return StatusCode(400, "Xác thực Email thất bại");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Đã có lỗi xảy ra trong quá trình xác thực Email {ex.Message}");
            }
        }
    }
}
