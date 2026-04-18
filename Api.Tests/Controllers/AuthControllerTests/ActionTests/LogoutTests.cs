using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

namespace Api.Tests.Controllers.AuthControllerTests.ActionTests
{
    public class LogoutTests : AuthControllerTests
    {
        [Fact]
        public async Task Logout_ShouldReturnOk_WhenUserAuthorized()
        {
            // Arrange
            string userId = Guid.NewGuid().ToString();
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, userId)
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            _sut.ControllerContext.HttpContext.User = new ClaimsPrincipal(identity);
            _serviceMock.Setup(x => x.RevokeRefreshTokenAsync(userId))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _sut.Logout();

            //Assert
            _serviceMock.Verify(x => x.RevokeRefreshTokenAsync(userId), Times.Once);
            result.Should().NotBeNull();
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Logout_ShouldReturnUnauthorized_WhenUserUnauthorized()
        {
            // Arrange

            // Act
            var result = await _sut.Logout();

            // Assert
            _serviceMock.Verify(x => x.RevokeRefreshTokenAsync(It.IsAny<string>()), Times.Never);
            result.Should().NotBeNull();
            result.Should().BeOfType<UnauthorizedResult>();
        }
    }
}
