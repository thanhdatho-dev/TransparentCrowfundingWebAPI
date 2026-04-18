using API.Controllers;
using Application.Interfaces.Services;
using AutoFixture;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Api.Tests.Controllers.AuthControllerTests
{
    public class AuthControllerTests
    {
        protected readonly IFixture _fixture;
        protected readonly Mock<IAuthService> _serviceMock;
        protected readonly AuthController _sut;

        public AuthControllerTests()
        {
            _fixture = new Fixture();
            _serviceMock = _fixture.Freeze<Mock<IAuthService>>();
            _sut = new AuthController(_serviceMock.Object);

            var httpContext = new DefaultHttpContext();
            _sut.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }
    }
}
