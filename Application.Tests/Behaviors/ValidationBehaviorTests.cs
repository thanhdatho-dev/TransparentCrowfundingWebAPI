using Application.Behaviors;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;

namespace Application.Tests.Behaviors
{
    public class ValidationBehaviorTests
    {
        // Test request/response types - must be public for validators
        public record TestRequest(string Name) : IRequest<string>;

        // Concrete validator implementations instead of Moq (avoids Castle proxy issues with record types)
        private class AlwaysValidValidator : AbstractValidator<TestRequest>
        {
            // No rules = always valid
        }

        private class AlwaysInvalidValidator : AbstractValidator<TestRequest>
        {
            public AlwaysInvalidValidator()
            {
                RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required");
            }
        }

        [Fact]
        public async Task Handle_NoValidators_CallsNext()
        {
            // Arrange
            var validators = Enumerable.Empty<IValidator<TestRequest>>();
            var behavior = new ValidationBehavior<TestRequest, string>(validators);
            var request = new TestRequest("test");
            var nextCalled = false;

            RequestHandlerDelegate<string> next = (ct) =>
            {
                nextCalled = true;
                return Task.FromResult("result");
            };

            // Act
            var result = await behavior.Handle(request, next, CancellationToken.None);

            // Assert
            nextCalled.Should().BeTrue();
            result.Should().Be("result");
        }

        [Fact]
        public async Task Handle_ValidRequest_CallsNext()
        {
            // Arrange
            var behavior = new ValidationBehavior<TestRequest, string>([new AlwaysValidValidator()]);
            var request = new TestRequest("valid-name");

            RequestHandlerDelegate<string> next = (ct) => Task.FromResult("result");

            // Act
            var result = await behavior.Handle(request, next, CancellationToken.None);

            // Assert
            result.Should().Be("result");
        }

        [Fact]
        public async Task Handle_InvalidRequest_ThrowsValidationException()
        {
            // Arrange
            var behavior = new ValidationBehavior<TestRequest, string>([new AlwaysInvalidValidator()]);
            var request = new TestRequest(""); // empty name triggers validation error

            RequestHandlerDelegate<string> next = (ct) => Task.FromResult("result");

            // Act
            var act = () => behavior.Handle(request, next, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<ValidationException>();
        }
    }
}
