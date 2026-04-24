using Application.Features.Auth.Commands.Login;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace Application.Tests.Features.Auth.Commands.Login
{
    public class LoginCommandValidatorTests
    {
        private readonly LoginCommandValidator _validator = new();

        [Fact]
        public void Validate_ValidCommand_HasNoErrors()
        {
            var command = new LoginCommand("0x1234", "message", "signature");

            var result = _validator.TestValidate(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Validate_EmptyMessage_HasError(string? message)
        {
            var command = new LoginCommand("0x1234", message!, "signature");

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.Message);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Validate_EmptySignature_HasError(string? signature)
        {
            var command = new LoginCommand("0x1234", "message", signature!);

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.Signature);
        }
    }
}
