using Domain.Entities;
using Domain.Interfaces.Repositories;
using Infrastructure.Identity;
using Infrastructure.Mappers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class UserRepository(UserManager<AppUser> userManager) : IUserRepository
    {
        private readonly UserManager<AppUser> _userManager = userManager;

        public async Task<bool> CreateAsync(User user)
        {
            var identity = user.FromUserToAppUser();
            var result = await _userManager.CreateAsync(identity).ConfigureAwait(false);
            return result.Succeeded;
        }

        public async Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default) =>
            await _userManager.FindByEmailAsync(email).ConfigureAwait(false) is not null;

        public async Task<User?> FindByIdAsync(string userId)
        {
            var identity = await _userManager.FindByIdAsync(userId);
            return identity?.FromAppUserToUserEntity();
        }

        public async Task<User?> FindByNameAsync(string username)
        {
            var identity = await _userManager.FindByNameAsync(username);
            return identity?.FromAppUserToUserEntity();
        }

        public async Task<bool> UpdateAsync(User user)
        {
            var identity = user.FromUserToAppUser();
            var result = await _userManager.UpdateAsync(identity).ConfigureAwait(false);
            return result.Succeeded;
        }
    }
}
