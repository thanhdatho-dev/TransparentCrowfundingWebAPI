

namespace Application.DTOs.Services.WalletSignature
{
    public class WalletSignatureDto
    {
        public string Message { get; set; } = null!;
        public string Signature { get; set; } = null!;
        public string Address { get; set; } = null!;
    }
}
