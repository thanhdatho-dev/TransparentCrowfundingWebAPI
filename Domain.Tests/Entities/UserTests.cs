using Domain.Entities;
using Domain.Events;
using Domain.ValueObjects;
using FluentAssertions;

namespace Domain.Tests.Entities
{
    public class UserTests
    {
        private readonly WalletAddress _wallet = WalletAddress.Create("0x1234567890abcdef1234567890abcdef12345678");
        private readonly Email _email = Email.Create("test@example.com");

        [Fact]
        public void Register_ValidData_CreatesUserWithCorrectProperties()
        {
            var user = User.Register(_wallet, _email);

            user.WalletAddress.Should().Be(_wallet);
            user.Email.Should().Be(_email);
        }

        [Fact]
        public void Register_ValidData_SetsEmailConfirmedTrue()
        {
            var user = User.Register(_wallet, _email);

            user.EmailConfirmed.Should().BeTrue();
        }

        [Fact]
        public void Register_ValidData_SetsCreatedTimestamp()
        {
            var before = DateTime.UtcNow;

            var user = User.Register(_wallet, _email);

            user.Created.Should().BeOnOrAfter(before);
            user.Created.Should().BeOnOrBefore(DateTime.UtcNow);
        }

        [Fact]
        public void Register_ValidData_SetsCreatedByToWalletValue()
        {
            var user = User.Register(_wallet, _email);

            user.CreatedBy.Should().Be(_wallet.Value);
        }

        [Fact]
        public void Register_ValidData_RaisesUserRegisteredEvent()
        {
            var user = User.Register(_wallet, _email);

            user.DomainEvents.Should().ContainSingle()
                .Which.Should().BeOfType<UserRegisteredEvent>()
                .Which.Should().Match<UserRegisteredEvent>(e =>
                    e.UserId == user.Id &&
                    e.Email == _email.Value &&
                    e.WalletAddress == _wallet.Value);
        }
    }
}
