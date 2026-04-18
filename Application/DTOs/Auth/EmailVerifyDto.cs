using Application.DTOs.Services.WalletSignature;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Auth
{
    public class EmailVerifyDto
    {
        [EmailAddress]
        public string Email { get; set; } = null!;
        public WalletSignatureDto? WalletSignature { get; set; }
    }
}
