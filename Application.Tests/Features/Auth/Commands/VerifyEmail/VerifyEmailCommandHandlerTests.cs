using Application.Features.Auth.Commands.VerifyEmail;
using Application.Interfaces.Services;
using Domain.Constants;
using Domain.Exceptions.DomainExceptions;
using Domain.Interfaces.Repositories;
using FluentAssertions;
using MediatR;
using Moq;

namespace Application.Tests.Features.Auth.Commands.VerifyEmail
{
    public class VerifyEmailCommandHandlerTests
    {
        private readonly Mock<ICacheService> _cacheMock = new();
        private readonly Mock<IEmailService> _emailMock = new();
        private readonly Mock<IWalletSignatureVerifier> _walletVerifierMock = new();
        private readonly Mock<IUnitOfWork> _uowMock = new();
        private readonly VerifyEmailCommandHandler _sut;

        private const string ValidWallet = "0x1234567890abcdef1234567890abcdef12345678";
        private const string ValidEmail = "test@example.com";
        private const string Message = "Sign this";
        private const string Signature = "0xsig";

        public VerifyEmailCommandHandlerTests()
        {
            _sut = new VerifyEmailCommandHandler(
                _uowMock.Object,
                _walletVerifierMock.Object,
                _cacheMock.Object,
                _emailMock.Object);
        }

        [Fact]
        public async Task Handle_ValidRequest_CachesOTPAndSendsEmail()
        {
            // Arrange
            var normalizedWallet = ValidWallet.ToLowerInvariant();
            _walletVerifierMock.Setup(x => x.VerifySignature(Message, Signature, normalizedWallet))
                .Returns(true);
            _uowMock.Setup(x => x.Users.ExistsByEmailAsync(ValidEmail, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var command = new VerifyEmailCommand(ValidEmail, ValidWallet, Message, Signature);

            // Act
            var result = await _sut.Handle(command, CancellationToken.None);

            // Assert
            result.Should().Be(Unit.Value);
            _cacheMock.Verify(x => x.SetAsync(
                $"otp:{ValidEmail}",
                It.Is<string>(s => s.Length == AuthConstants.OTPLength),
                TimeSpan.FromMinutes(AuthConstants.OTPExpiryMinutes)), Times.Once);
            _cacheMock.Verify(x => x.SetAsync(
                $"email-verify:{ValidEmail}",
                normalizedWallet,
                TimeSpan.FromMinutes(AuthConstants.OTPExpiryMinutes)), Times.Once);
        }

        [Fact]
        public async Task Handle_InvalidSignature_ThrowsBusinessRuleViolation()
        {
            // Arrange
            _walletVerifierMock.Setup(x => x.VerifySignature(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(false);

            var command = new VerifyEmailCommand(ValidEmail, ValidWallet, Message, Signature);

            // Act
            var act = () => _sut.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<BusinessRuleViolationException>()
                .Where(e => e.ErrorCode == ErrorCodes.InvalidWallet);
        }

        [Fact]
        public async Task Handle_EmailAlreadyExists_ThrowsExistedEntityException()
        {
            // Arrange
            var normalizedWallet = ValidWallet.ToLowerInvariant();
            _walletVerifierMock.Setup(x => x.VerifySignature(Message, Signature, normalizedWallet))
                .Returns(true);
            _uowMock.Setup(x => x.Users.ExistsByEmailAsync(ValidEmail, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var command = new VerifyEmailCommand(ValidEmail, ValidWallet, Message, Signature);

            // Act
            var act = () => _sut.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<ExistedEntityException>()
                .Where(e => e.ErrorCode == ErrorCodes.EmailAlreadyUsed);
        }
    }
}
