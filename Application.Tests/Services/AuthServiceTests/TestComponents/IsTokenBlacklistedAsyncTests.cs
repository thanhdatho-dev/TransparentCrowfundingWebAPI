using AutoFixture;
using FluentAssertions;
using Moq;

namespace Application.Tests.Services.AuthServiceTests.TestComponents
{
    public class IsTokenBlacklistedAsyncTests : AuthServiceTests
    {
        [Fact]
        public async Task IsTokenBlacklistedAsync_ShouldReturnTrue_WhenTokenIsBlacklisted()
        {
            // Arrange
            var jti = _fixture.Create<string>();

            _tokenRepo
                .Setup(x => x.IsTokenBlacklistedAsync(jti))
                .ReturnsAsync(true);

            // Act
            var result = await _sut.IsTokenBlacklistedAsync(jti);

            // Assert
            result.Should().BeTrue();
            _tokenRepo.Verify(x => x.IsTokenBlacklistedAsync(jti), Times.Once);
        }

        [Fact]
        public async Task IsTokenBlacklistedAsync_ShouldReturnFalse_WhenTokenIsNotBlacklisted()
        {
            // Arrange
            var jti = _fixture.Create<string>();

            _tokenRepo
                .Setup(x => x.IsTokenBlacklistedAsync(jti))
                .ReturnsAsync(false);

            // Act
            var result = await _sut.IsTokenBlacklistedAsync(jti);

            // Assert
            result.Should().BeFalse();
            _tokenRepo.Verify(x => x.IsTokenBlacklistedAsync(jti), Times.Once);
        }
    }
}
