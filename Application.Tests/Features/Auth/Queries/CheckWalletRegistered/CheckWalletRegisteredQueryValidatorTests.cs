using Application.Features.Auth.Queries.CheckWalletRegistered;
using FluentValidation.TestHelper;

namespace Application.Tests.Features.Auth.Queries.CheckWalletRegistered
{
    public class CheckWalletRegisteredQueryValidatorTests
    {
        private readonly CheckWalletRegisteredQueryValidator _validator = new();

        [Fact]
        public void Validate_ValidWalletAddress_HasNoErrors()
        {
            var query = new CheckWalletRegisteredQuery("0x1234567890abcdef1234567890abcdef12345678");

            var result = _validator.TestValidate(query);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Validate_EmptyWalletAddress_HasError(string? address)
        {
            var query = new CheckWalletRegisteredQuery(address!);

            var result = _validator.TestValidate(query);

            result.ShouldHaveValidationErrorFor(x => x.WalletAddress);
        }

        [Fact]
        public void Validate_InvalidWalletFormat_HasError()
        {
            var query = new CheckWalletRegisteredQuery("not-a-wallet");

            var result = _validator.TestValidate(query);

            result.ShouldHaveValidationErrorFor(x => x.WalletAddress)
                .WithErrorMessage("Invalid Ethereum wallet address");
        }
    }
}
