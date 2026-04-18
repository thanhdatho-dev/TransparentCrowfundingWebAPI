using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.Services.JWTs
{
    public class TokenDto
    {
        public string AccessToken { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
    }
}
