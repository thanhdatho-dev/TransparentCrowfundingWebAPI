namespace Application.Interfaces.Repositories
{
    public interface IMailRepository
    {
        string GenerateSecureOTP(int length = 6);
        Task SaveOTPAsync(string userId, string otp);
        Task<string?> GetOTPAsync(string userId);
        Task DeleteOTPAsync(string userId);
    }
}
