using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities
{
    public class User : IdentityUser
    {
        public string? RefreshToken { get; set; } = null!;
        public DateTime? RefreshTokenExpiryTime { get; set; }
    }
}
