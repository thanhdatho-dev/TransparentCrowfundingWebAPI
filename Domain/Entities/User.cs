using Domain.Entities.Common;
using Domain.Events;
using Domain.ValueObjects;

namespace Domain.Entities
{
    public class User : AuditableBaseEntity
    {
        public WalletAddress WalletAddress { get; private set; } = null!;
        public Email Email { get; private set; } = null!;
        public bool EmailConfirmed { get; set; }

        private User(WalletAddress walletAddress, Email email)
        {
            WalletAddress = walletAddress;
            Email = email;
        }

        public static User Register(WalletAddress walletAddress, Email email)
        {
            var user = new User(walletAddress, email)
            {
                EmailConfirmed = true,
                Created = DateTime.UtcNow,
                CreatedBy = walletAddress.Value
            };

            user.RaiseDomainEvent(new UserRegisteredEvent(
                user.Id,
                email.Value,
                walletAddress.Value));

            return user;
        }
    }
}

