using Application.DTOs.Auth;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Api.Tests.Controllers.AuthControllerTests.ActionTests
{
    public class RefreshTests : AuthControllerTests
    {
        [Fact]
        public async Task Refresh_ShouldReturnOk_WhenRequestValid()
        {
            // Arrange
            string userId = Guid.NewGuid().ToString();
            string jti = Guid.NewGuid().ToString();
            string accessToken = Guid.NewGuid().ToString();

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, userId),
                new(JwtRegisteredClaimNames.Jti, jti)
            };

            var identity = new ClaimsIdentity(claims, "TestAuth");
            _sut.ControllerContext.HttpContext.User = new ClaimsPrincipal(identity);

            _sut.ControllerContext.HttpContext.Request.Headers.Authorization = $"Bearer {accessToken}";

            var dictionary = new Dictionary<string, string>
            {
                { "refreshToken", "refresh-token-value" }
            };
            _sut.ControllerContext.HttpContext.Request.Cookies = new MockRequestCookieCollection(dictionary);

            var refreshDto = _fixture.Create<RefreshTokenDto>();

            _serviceMock.Setup(x => x.IsTokenBlacklistedAsync(jti)).ReturnsAsync(false);
            _serviceMock.Setup(x => x.GetRefreshTokenAsync(userId)).ReturnsAsync("refresh-token-value");
            _serviceMock.Setup(x => x.RefreshTokenAsync(userId, accessToken)).ReturnsAsync(refreshDto);
            // Act
            var result = await _sut.Refresh();

            //
            result.Should().NotBeNull();
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Refresh_ShouldReturnUnauthorized_WhenRefreshTokenInCookiesInvalid()
        {
            // Arrange
            string userId = Guid.NewGuid().ToString();
            string jti = Guid.NewGuid().ToString();
            string accessToken = Guid.NewGuid().ToString();

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, userId),
                new(JwtRegisteredClaimNames.Jti, jti)
            };

            var identity = new ClaimsIdentity(claims, "TestAuth");
            _sut.ControllerContext.HttpContext.User = new ClaimsPrincipal(identity);

            _sut.ControllerContext.HttpContext.Request.Headers.Authorization = $"Bearer {accessToken}";

            var refreshDto = _fixture.Create<RefreshTokenDto>();

            _serviceMock.Setup(x => x.IsTokenBlacklistedAsync(jti)).ReturnsAsync(false);
            _serviceMock.Setup(x => x.GetRefreshTokenAsync(userId)).ReturnsAsync("value");
            _serviceMock.Setup(x => x.RefreshTokenAsync(userId, accessToken)).ReturnsAsync(refreshDto);
            // Act
            var result = await _sut.Refresh();

            // Assert
            _serviceMock.Verify(x => x.IsTokenBlacklistedAsync(It.IsAny<string>()), Times.Never);
            _serviceMock.Verify(x => x.GetRefreshTokenAsync(It.IsAny<string>()), Times.Never);
            _serviceMock.Verify(x => x.RefreshTokenAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            result.Should().NotBeNull();
            result.Should().BeOfType<UnauthorizedResult>();
        }

        [Fact]
        public async Task Refresh_ShouldReturnUnauthorized_WhenJtiInvalid()
        {
            // Arrange
            string userId = Guid.NewGuid().ToString();
            string jti = "";
            string accessToken = Guid.NewGuid().ToString();

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, userId),
                new(JwtRegisteredClaimNames.Jti, jti)
            };

            var identity = new ClaimsIdentity(claims, "TestAuth");
            _sut.ControllerContext.HttpContext.User = new ClaimsPrincipal(identity);

            _sut.ControllerContext.HttpContext.Request.Headers.Authorization = $"Bearer {accessToken}";

            var dictionary = new Dictionary<string, string>
            {
                { "refreshToken", "refresh-token" }
            };
            _sut.ControllerContext.HttpContext.Request.Cookies = new MockRequestCookieCollection(dictionary);

            var refreshDto = _fixture.Create<RefreshTokenDto>();

            _serviceMock.Setup(x => x.IsTokenBlacklistedAsync(jti)).ReturnsAsync(false);
            _serviceMock.Setup(x => x.GetRefreshTokenAsync(userId)).ReturnsAsync("refresh-token");
            _serviceMock.Setup(x => x.RefreshTokenAsync(userId, accessToken)).ReturnsAsync(refreshDto);
            // Act
            var result = await _sut.Refresh();

            // Assert
            _serviceMock.Verify(x => x.IsTokenBlacklistedAsync(It.IsAny<string>()), Times.Never);
            _serviceMock.Verify(x => x.GetRefreshTokenAsync(It.IsAny<string>()), Times.Never);
            _serviceMock.Verify(x => x.RefreshTokenAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            result.Should().NotBeNull();
            result.Should().BeOfType<UnauthorizedResult>();
        }

        [Fact]
        public async Task Refresh_ShouldReturnUnauthorized_WhenAccessTokenInvalid()
        {
            // Arrange
            string userId = Guid.NewGuid().ToString();
            string jti = Guid.NewGuid().ToString();
            string accessToken = Guid.NewGuid().ToString();

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, userId),
                new(JwtRegisteredClaimNames.Jti, jti)
            };

            var identity = new ClaimsIdentity(claims, "TestAuth");
            _sut.ControllerContext.HttpContext.User = new ClaimsPrincipal(identity);

            _sut.ControllerContext.HttpContext.Request.Headers.Authorization = $"Bearer {accessToken}";

            var dictionary = new Dictionary<string, string>
            {
                { "refreshToken", "refresh-token" }
            };
            _sut.ControllerContext.HttpContext.Request.Cookies = new MockRequestCookieCollection(dictionary);

            var refreshDto = _fixture.Create<RefreshTokenDto>();

            _serviceMock.Setup(x => x.IsTokenBlacklistedAsync(jti)).ReturnsAsync(true);
            _serviceMock.Setup(x => x.GetRefreshTokenAsync(userId)).ReturnsAsync("refresh-token");
            _serviceMock.Setup(x => x.RefreshTokenAsync(userId, accessToken)).ReturnsAsync(refreshDto);
            // Act
            var result = await _sut.Refresh();

            // Assert
            _serviceMock.Verify(x => x.IsTokenBlacklistedAsync(It.IsAny<string>()), Times.Once);
            _serviceMock.Verify(x => x.GetRefreshTokenAsync(It.IsAny<string>()), Times.Never);
            _serviceMock.Verify(x => x.RefreshTokenAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            result.Should().NotBeNull();
            result.Should().BeOfType<UnauthorizedResult>();
        }

        [Fact]
        public async Task Refresh_ShouldReturnUnauthorized_WhenIndentifierInvalid()
        {
            // Arrange
            string userId = string.Empty;
            string jti = Guid.NewGuid().ToString();
            string accessToken = Guid.NewGuid().ToString();

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, userId),
                new(JwtRegisteredClaimNames.Jti, jti)
            };

            var identity = new ClaimsIdentity(claims, "TestAuth");
            _sut.ControllerContext.HttpContext.User = new ClaimsPrincipal(identity);

            _sut.ControllerContext.HttpContext.Request.Headers.Authorization = $"Bearer {accessToken}";

            var dictionary = new Dictionary<string, string>
            {
                { "refreshToken", "refresh-token" }
            };
            _sut.ControllerContext.HttpContext.Request.Cookies = new MockRequestCookieCollection(dictionary);

            var refreshDto = _fixture.Create<RefreshTokenDto>();

            _serviceMock.Setup(x => x.IsTokenBlacklistedAsync(jti)).ReturnsAsync(false);
            _serviceMock.Setup(x => x.GetRefreshTokenAsync(userId)).ReturnsAsync("refresh-token");
            _serviceMock.Setup(x => x.RefreshTokenAsync(userId, accessToken)).ReturnsAsync(refreshDto);
            // Act
            var result = await _sut.Refresh();

            // Assert
            _serviceMock.Verify(x => x.IsTokenBlacklistedAsync(It.IsAny<string>()), Times.Once);
            _serviceMock.Verify(x => x.GetRefreshTokenAsync(It.IsAny<string>()), Times.Never);
            _serviceMock.Verify(x => x.RefreshTokenAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            result.Should().NotBeNull();
            result.Should().BeOfType<UnauthorizedResult>();
        }

        [Fact]
        public async Task Refresh_ShouldReturnUnauthorized_WhenRefreshTokenNotInDB()
        {
            // Arrange
            string userId = Guid.NewGuid().ToString();
            string jti = Guid.NewGuid().ToString();
            string accessToken = Guid.NewGuid().ToString();

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, userId),
                new(JwtRegisteredClaimNames.Jti, jti)
            };

            var identity = new ClaimsIdentity(claims, "TestAuth");
            _sut.ControllerContext.HttpContext.User = new ClaimsPrincipal(identity);

            _sut.ControllerContext.HttpContext.Request.Headers.Authorization = $"Bearer {accessToken}";

            var dictionary = new Dictionary<string, string>
            {
                { "refreshToken", "refresh-token" }
            };
            _sut.ControllerContext.HttpContext.Request.Cookies = new MockRequestCookieCollection(dictionary);

            var refreshDto = _fixture.Create<RefreshTokenDto>();

            _serviceMock.Setup(x => x.IsTokenBlacklistedAsync(jti)).ReturnsAsync(false);
            _serviceMock.Setup(x => x.GetRefreshTokenAsync(userId)).ReturnsAsync(string.Empty);
            _serviceMock.Setup(x => x.RefreshTokenAsync(userId, accessToken)).ReturnsAsync(refreshDto);
            // Act
            var result = await _sut.Refresh();

            // Assert
            _serviceMock.Verify(x => x.IsTokenBlacklistedAsync(It.IsAny<string>()), Times.Once);
            _serviceMock.Verify(x => x.GetRefreshTokenAsync(It.IsAny<string>()), Times.Once);
            _serviceMock.Verify(x => x.RefreshTokenAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            result.Should().NotBeNull();
            result.Should().BeOfType<UnauthorizedResult>();
        }

        [Fact]
        public async Task Refresh_ShouldReturnUnauthorized_WhenRefreshTokenNotMatch()
        {
            // Arrange
            string userId = Guid.NewGuid().ToString();
            string jti = Guid.NewGuid().ToString();
            string accessToken = Guid.NewGuid().ToString();

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, userId),
                new(JwtRegisteredClaimNames.Jti, jti)
            };

            var identity = new ClaimsIdentity(claims, "TestAuth");
            _sut.ControllerContext.HttpContext.User = new ClaimsPrincipal(identity);

            _sut.ControllerContext.HttpContext.Request.Headers.Authorization = $"Bearer {accessToken}";

            var dictionary = new Dictionary<string, string>
            {
                { "refreshToken", "refresh-token" }
            };
            _sut.ControllerContext.HttpContext.Request.Cookies = new MockRequestCookieCollection(dictionary);

            var refreshDto = _fixture.Create<RefreshTokenDto>();

            _serviceMock.Setup(x => x.IsTokenBlacklistedAsync(jti)).ReturnsAsync(false);
            _serviceMock.Setup(x => x.GetRefreshTokenAsync(userId)).ReturnsAsync("diff-refresh-token");
            _serviceMock.Setup(x => x.RefreshTokenAsync(userId, accessToken)).ReturnsAsync(refreshDto);
            // Act
            var result = await _sut.Refresh();

            // Assert
            _serviceMock.Verify(x => x.IsTokenBlacklistedAsync(It.IsAny<string>()), Times.Once);
            _serviceMock.Verify(x => x.GetRefreshTokenAsync(It.IsAny<string>()), Times.Once);
            _serviceMock.Verify(x => x.RefreshTokenAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            result.Should().NotBeNull();
            result.Should().BeOfType<UnauthorizedResult>();
        }

        [Fact]
        public async Task Refresh_ShouldReturnUnauthorized_WhenAccessInValid()
        {
            // Arrange
            string userId = Guid.NewGuid().ToString();
            string jti = Guid.NewGuid().ToString();
            string accessToken = string.Empty;

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, userId),
                new(JwtRegisteredClaimNames.Jti, jti)
            };

            var identity = new ClaimsIdentity(claims, "TestAuth");
            _sut.ControllerContext.HttpContext.User = new ClaimsPrincipal(identity);

            var dictionary = new Dictionary<string, string>
            {
                { "refreshToken", "refresh-token" }
            };
            _sut.ControllerContext.HttpContext.Request.Cookies = new MockRequestCookieCollection(dictionary);

            var refreshDto = _fixture.Create<RefreshTokenDto>();

            _serviceMock.Setup(x => x.IsTokenBlacklistedAsync(jti)).ReturnsAsync(false);
            _serviceMock.Setup(x => x.GetRefreshTokenAsync(userId)).ReturnsAsync("refresh-token");
            _serviceMock.Setup(x => x.RefreshTokenAsync(userId, accessToken)).ReturnsAsync(refreshDto);
            // Act
            var result = await _sut.Refresh();

            // Assert
            _serviceMock.Verify(x => x.IsTokenBlacklistedAsync(It.IsAny<string>()), Times.Once);
            _serviceMock.Verify(x => x.GetRefreshTokenAsync(It.IsAny<string>()), Times.Once);
            _serviceMock.Verify(x => x.RefreshTokenAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            result.Should().NotBeNull();
            result.Should().BeOfType<UnauthorizedResult>();
        }
    }
}
