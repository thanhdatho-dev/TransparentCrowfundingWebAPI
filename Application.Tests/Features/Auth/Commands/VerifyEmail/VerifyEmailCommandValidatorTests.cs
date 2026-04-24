using Application.Features.Auth.Commands.VerifyEmail;
using FluentValidation.TestHelper;

namespace Application.Tests.Features.Auth.Commands.VerifyEmail
{
    public class VerifyEmailCommandValidatorTests
    {
        private readonly VerifyEmailCommandValidator _validator = new();

        [Fact]
        public void Validate_ValidCommand_HasNoErrors()
        {
            var command = new VerifyEmailCommand("test@test.com", "0x123", "message", "signature");

            var result = _validator.TestValidate(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Validate_EmptyMessage_HasError(string? message)
        {
            var command = new VerifyEmailCommand("test@test.com", "0x123", message!, "signature");

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.Message);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Validate_EmptySignature_HasError(string? signature)
        {
            var command = new VerifyEmailCommand("test@test.com", "0x123", "message", signature!);

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.Signature);
        }
    }
}
