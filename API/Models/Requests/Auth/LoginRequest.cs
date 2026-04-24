namespace API.Models.Requests.Auth
{
    public record LoginRequest(
        string Message, string Signature, string WalletAddress);
}
