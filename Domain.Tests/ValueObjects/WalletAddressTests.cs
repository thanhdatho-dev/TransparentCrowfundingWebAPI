using Domain.ValueObjects;
using FluentAssertions;

namespace Domain.Tests.ValueObjects
{
    public class WalletAddressTests
    {
        private const string ValidAddress = "0x1234567890abcdef1234567890abcdef12345678";

        [Fact]
        public void Create_ValidAddress_ReturnsWalletAddress()
        {
            var wallet = WalletAddress.Create(ValidAddress);

            wallet.Should().NotBeNull();
            wallet.Value.Should().Be(ValidAddress.ToLowerInvariant());
        }

        [Fact]
        public void Create_MixedCaseAddress_NormalizesToLowerCase()
        {
            var address = "0xABCDEF1234567890ABCDEF1234567890ABCDEF12";

            var wallet = WalletAddress.Create(address);

            wallet.Value.Should().Be(address.ToLowerInvariant());
        }

        [Fact]
        public void Create_ValidAddress_ToStringReturnsValue()
        {
            var wallet = WalletAddress.Create(ValidAddress);

            wallet.ToString().Should().Be(wallet.Value);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Create_NullOrEmpty_ThrowsArgumentException(string? address)
        {
            var act = () => WalletAddress.Create(address!);

            act.Should().Throw<ArgumentException>()
                .WithMessage("*cannot be empty*");
        }

        [Theory]
        [InlineData("1234567890abcdef1234567890abcdef12345678")]    // missing 0x prefix
        [InlineData("0x1234")]                                       // too short
        [InlineData("0x1234567890abcdef1234567890abcdef1234567890")] // too long
        [InlineData("0xGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGG")] // invalid hex chars
        public void Create_InvalidFormat_ThrowsArgumentException(string address)
        {
            var act = () => WalletAddress.Create(address);

            act.Should().Throw<ArgumentException>()
                .WithMessage("*Invalid Ethereum wallet address*");
        }

        [Fact]
        public void TryCreate_ValidAddress_ReturnsTrueWithResult()
        {
            var success = WalletAddress.TryCreate(ValidAddress, out var result);

            success.Should().BeTrue();
            result.Should().NotBeNull();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void TryCreate_NullOrEmpty_ReturnsFalse(string? address)
        {
            var success = WalletAddress.TryCreate(address!, out var result);

            success.Should().BeFalse();
            result.Should().BeNull();
        }
    }
}
