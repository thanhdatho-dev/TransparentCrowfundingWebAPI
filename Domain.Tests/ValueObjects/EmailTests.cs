using Domain.ValueObjects;
using FluentAssertions;

namespace Domain.Tests.ValueObjects
{
    public class EmailTests
    {
        [Fact]
        public void Create_ValidEmail_ReturnsEmail()
        {
            var email = Email.Create("user@example.com");

            email.Should().NotBeNull();
            email.Value.Should().Be("user@example.com");
        }

        [Fact]
        public void Create_MixedCaseEmail_NormalizesToLowerCase()
        {
            var email = Email.Create("User@EXAMPLE.Com");

            email.Value.Should().Be("user@example.com");
        }

        [Fact]
        public void Create_ValidEmail_ToStringReturnsValue()
        {
            var email = Email.Create("test@test.com");

            email.ToString().Should().Be("test@test.com");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Create_NullOrEmpty_ThrowsArgumentException(string? value)
        {
            var act = () => Email.Create(value!);

            act.Should().Throw<ArgumentException>()
                .WithMessage("*cannot be empty*");
        }

        [Theory]
        [InlineData("notanemail")]
        [InlineData("missing@domain")]
        [InlineData("@nodomain.com")]
        public void Create_InvalidFormat_ThrowsArgumentException(string value)
        {
            var act = () => Email.Create(value);

            act.Should().Throw<ArgumentException>()
                .WithMessage("*Invalid email format*");
        }

        [Theory]
        [InlineData("a@b.co")]
        [InlineData("complex+tag@sub.domain.org")]
        public void Create_EdgeCaseValidEmails_Succeeds(string value)
        {
            var email = Email.Create(value);

            email.Value.Should().Be(value.ToLowerInvariant());
        }
    }
}
