using Domain.Entities;
using Domain.ValueObjects;
using Infrastructure.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Mappers
{
    public static class UserMappers
    {
        public static User FromAppUserToUserEntity(this AppUser appUser)
        {
            var walletAddress = WalletAddress.Create(appUser.UserName!);
            var email = Email.Create(appUser.Email!);
            var user = User.Register(walletAddress, email);
            user.Id = Guid.Parse(appUser.Id);
            user.EmailConfirmed = true;
            return user;
        }

        public static AppUser FromUserToAppUserForCreate(this User user)
            => new()
            {
                UserName = user.WalletAddress.ToString(),
                Email = user.Email.ToString(),
                EmailConfirmed = user.EmailConfirmed
            };

        public static AppUser FromUserToAppUserForRead(this User user)
            => new()
            {
                Id = user.Id.ToString(),
                UserName = user.WalletAddress.ToString(),
                Email = user.Email.ToString(),
                EmailConfirmed = user.EmailConfirmed
            };
    }
}
