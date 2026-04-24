namespace API.Models.Requests.Auth
{
    public record VerifyEmailRequest(
        string Email,
        string WalletAddress,
        string Message,
        string Signature);
}
