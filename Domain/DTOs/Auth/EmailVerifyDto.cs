using Domain.DTOs.Services.WalletSignature;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Domain.DTOs.Auth
{
    public class EmailVerifyDto
    {
        [EmailAddress]
        public string Email { get; set; } = null!;
        public WalletSignatureDto? WalletSignature { get; set; }
    }
}
