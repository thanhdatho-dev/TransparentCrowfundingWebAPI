using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.Auth
{
    public class LoginDto
    {
        public string UserId { get; set; } = null!;
        public string AccessToken { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
        public DateTime ExpiryRefreshTokenTime { get; set; }
    }
}
