using Application.Features.Auth.Events;
using Application.Interfaces.Services;
using Domain.Events;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Application.Tests.Features.Auth.Events
{
    public class UserRegisteredEventHandlerTests
    {
        private readonly Mock<IEmailService> _emailMock = new();
        private readonly Mock<ILogger<UserRegisteredEventHandler>> _loggerMock = new();
        private readonly UserRegisteredEventHandler _sut;

        public UserRegisteredEventHandlerTests()
        {
            _sut = new UserRegisteredEventHandler(_emailMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_SendsWelcomeEmail()
        {
            // Arrange
            var domainEvent = new UserRegisteredEvent(
                Guid.NewGuid(),
                "test@example.com",
                "0x1234567890abcdef1234567890abcdef12345678");

            // Act
            await _sut.Handle(domainEvent, CancellationToken.None);

            // Assert
            _emailMock.Verify(x => x.SendAsync(
                "test@example.com",
                "Welcome to Transparent Crowdfunding!",
                It.Is<string>(body => body.Contains("0x1234567890abcdef1234567890abcdef12345678")),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_EmailFails_DoesNotThrow()
        {
            // Arrange
            var domainEvent = new UserRegisteredEvent(
                Guid.NewGuid(),
                "test@example.com",
                "0x1234567890abcdef1234567890abcdef12345678");

            _emailMock.Setup(x => x.SendAsync(
                    It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("SMTP down"));

            // Act
            var act = () => _sut.Handle(domainEvent, CancellationToken.None);

            // Assert — should NOT throw, side-effect failures are swallowed
            await act.Should().NotThrowAsync();
        }
    }
}
