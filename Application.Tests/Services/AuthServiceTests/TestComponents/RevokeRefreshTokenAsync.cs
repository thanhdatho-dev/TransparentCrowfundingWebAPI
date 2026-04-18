using AutoFixture;
using FluentAssertions;
using Moq;

namespace Application.Tests.Services.AuthServiceTests.TestComponents
{
    public class RevokeRefreshTokenAsync : AuthServiceTests
    {
        [Fact]
        public async Task RevokeRefreshTokenAsync_ShouldDeleteRefreshToken_WhenSuccess()
        {
            // Arrange
            var userId = _fixture.Create<string>();

            _tokenRepo
                .Setup(x => x.DeleteRefreshTokenAsync(userId))
                .Returns(Task.CompletedTask);

            // Act
            await _sut.RevokeRefreshTokenAsync(userId);

            // Assert
            _tokenRepo.Verify(x => x.DeleteRefreshTokenAsync(userId), Times.Once);
        }

        [Fact]
        public async Task RevokeRefreshTokenAsync_ShouldThrow_WhenDeleteFails()
        {
            // Arrange
            var userId = _fixture.Create<string>();

            _tokenRepo
                .Setup(x => x.DeleteRefreshTokenAsync(userId))
                .ThrowsAsync(new Exception("Delete failed"));

            // Act
            Func<Task> act = async () => await _sut.RevokeRefreshTokenAsync(userId);

            // Assert
            await act.Should().ThrowAsync<Exception>().WithMessage("Delete failed");
            _tokenRepo.Verify(x => x.DeleteRefreshTokenAsync(userId), Times.Once);
        }
    }
}
