using Application.Interfaces.Repositories;
using Domain.Constants;
using StackExchange.Redis;
using System.Security.Cryptography;

namespace Infrastructure.Repositories
{
    public class MailRepository(IConnectionMultiplexer redis) : IMailRepository
    {
        private readonly IDatabase _db = redis.GetDatabase();

        public string GenerateSecureOTP(int length = AuthConstansts.OTPLength)
        {
            return string.Join(string.Empty, RandomNumberGenerator.GetItems("0123456789".ToCharArray(), length));
        }

        public async Task SaveOTPAsync(string userId, string otp)
        {
            await _db.StringSetAsync($"otp:{userId}", otp, TimeSpan.FromMinutes(AuthConstansts.OTPExpiryMinutes));
        }

        public async Task<string?> GetOTPAsync(string userId)
        {
            return await _db.StringGetAsync($"otp:{userId}");
        }

        public async Task DeleteOTPAsync(string userId)
        {
            await _db.KeyDeleteAsync($"otp:{userId}");
        }

    }
}
