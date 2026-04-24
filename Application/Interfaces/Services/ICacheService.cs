namespace Application.Interfaces.Services
{
    public interface ICacheService
    {
        Task SetAsync(string key, string value, TimeSpan expiry);
        Task<string?> GetAsync(string key);
        Task DeleteAsync(string key);
        Task<bool> ExistsAsync(string key);
    }
}
