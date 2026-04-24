using Domain.Entities;

namespace Domain.Interfaces.Repositories
{
    public interface IUserRepository
    {
        Task<User?> FindByIdAsync(string userId);
        Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default);
        Task<User?> FindByNameAsync(string username);
        Task<bool> CreateAsync(User user);
        Task<bool> UpdateAsync(User user);
    }
}
