using Application.Interfaces.Services;
using StackExchange.Redis;

namespace Infrastructure.Services
{
    public class CacheService(IConnectionMultiplexer redis) : ICacheService
    {
        private readonly IDatabase _db = redis.GetDatabase();
        public async Task<string?> GetAsync(string key)
        {
            return await _db.StringGetAsync(key);
        }

        public async Task SetAsync(string key, string value, TimeSpan expiry)
        {
            await _db.StringSetAsync(key, value, expiry);
        }

        public async Task DeleteAsync(string key)
        {
            await _db.KeyDeleteAsync(key);
        }


        public async Task<bool> ExistsAsync(string key)
        {
            return _db.KeyExists(key);
        }
    }
}
