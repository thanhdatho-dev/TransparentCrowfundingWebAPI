namespace API.Models.Requests.Auth
{
    public record ConfirmEmailRequest(string Email, string WalletAddress, string OTP);
}
