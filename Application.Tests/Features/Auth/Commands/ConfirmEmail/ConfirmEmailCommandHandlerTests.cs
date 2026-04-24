using Application.Features.Auth.Commands.ConfirmEmail;
using Application.Interfaces.Services;
using Domain.Constants;
using Domain.Exceptions.DomainExceptions;
using Domain.Interfaces.Repositories;
using FluentAssertions;
using MediatR;
using Moq;

namespace Application.Tests.Features.Auth.Commands.ConfirmEmail
{
    public class ConfirmEmailCommandHandlerTests
    {
        private readonly Mock<ICacheService> _cacheMock = new();
        private readonly Mock<IUnitOfWork> _uowMock = new();
        private readonly ConfirmEmailCommandHandler _sut;

        private const string ValidWallet = "0x1234567890abcdef1234567890abcdef12345678";
        private const string ValidEmail = "test@example.com";
        private const string ValidOTP = "123456";

        public ConfirmEmailCommandHandlerTests()
        {
            _sut = new ConfirmEmailCommandHandler(_cacheMock.Object, _uowMock.Object);
        }

        [Fact]
        public async Task Handle_ValidOTPAndContext_CreatesUserAndClearsCache()
        {
            // Arrange
            _cacheMock.Setup(x => x.GetAsync($"email-verify:{ValidEmail}"))
                .ReturnsAsync(ValidWallet);
            _cacheMock.Setup(x => x.GetAsync($"otp:{ValidEmail}"))
                .ReturnsAsync(ValidOTP);
            _uowMock.Setup(x => x.Users.CreateAsync(It.IsAny<Domain.Entities.User>()))
                .ReturnsAsync(true);

            var command = new ConfirmEmailCommand(ValidEmail, ValidWallet, ValidOTP);

            // Act
            var result = await _sut.Handle(command, CancellationToken.None);

            // Assert
            result.Should().Be(Unit.Value);
            _cacheMock.Verify(x => x.DeleteAsync($"otp:{ValidEmail}"), Times.Once);
            _cacheMock.Verify(x => x.DeleteAsync($"email-verify:{ValidEmail}"), Times.Once);
            _uowMock.Verify(x => x.Users.CreateAsync(It.Is<Domain.Entities.User>(u =>
                u.Email.Value == ValidEmail &&
                u.WalletAddress.Value == ValidWallet.ToLowerInvariant())), Times.Once);
        }

        [Fact]
        public async Task Handle_InvalidEmailContext_ThrowsBusinessRuleViolation()
        {
            // Arrange
            _cacheMock.Setup(x => x.GetAsync($"email-verify:{ValidEmail}"))
                .ReturnsAsync("different-wallet");

            var command = new ConfirmEmailCommand(ValidEmail, ValidWallet, ValidOTP);

            // Act
            var act = () => _sut.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<BusinessRuleViolationException>()
                .Where(e => e.ErrorCode == ErrorCodes.Forbidden);
        }

        [Fact]
        public async Task Handle_WrongOTP_ThrowsBusinessRuleViolation()
        {
            // Arrange
            _cacheMock.Setup(x => x.GetAsync($"email-verify:{ValidEmail}"))
                .ReturnsAsync(ValidWallet);
            _cacheMock.Setup(x => x.GetAsync($"otp:{ValidEmail}"))
                .ReturnsAsync("999999");

            var command = new ConfirmEmailCommand(ValidEmail, ValidWallet, ValidOTP);

            // Act
            var act = () => _sut.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<BusinessRuleViolationException>()
                .Where(e => e.ErrorCode == ErrorCodes.OTPMismatch);
        }
    }
}
