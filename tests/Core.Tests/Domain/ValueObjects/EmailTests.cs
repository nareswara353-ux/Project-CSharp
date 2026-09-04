using Domain.ValueObjects;
using FluentAssertions;

namespace Core.Tests.Domain.ValueObjects;

public class EmailTests
{
    [Theory]
    [InlineData("test@example.com")]
    [InlineData("john.doe@company.co.id")]
    [InlineData("user+filter@domain.org")]
    [InlineData("admin@sub.domain.net")]
    public void Create_ShouldReturnValidEmail_WhenFormatIsValid(string emailAddress)
    {
        // Act
        var email = Email.Create(emailAddress);

        // Assert
        email.Value.Should().Be(emailAddress.ToLowerInvariant());
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_ShouldThrowException_WhenEmailEmpty(string emailAddress)
    {
        // Act
        Action act = () => Email.Create(emailAddress);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Email cannot be empty*");
    }

    [Theory]
    [InlineData("plainaddress")]
    [InlineData("missing@domain")]
    [InlineData("@missinglocal.com")]
    [InlineData("user@.com")]
    [InlineData("user@domain.")]
    [InlineData("user@domain,com")]
    [InlineData("user name@domain.com")]
    public void Create_ShouldThrowException_WhenEmailFormatInvalid(string emailAddress)
    {
        // Act
        Action act = () => Email.Create(emailAddress);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Invalid email format*");
    }

    [Fact]
    public void Create_ShouldNormalizeToLowercase_WhenEmailHasUppercase()
    {
        // Arrange
        var emailAddress = "TestUser@Domain.COM";

        // Act
        var email = Email.Create(emailAddress);

        // Assert
        email.Value.Should().Be("testuser@domain.com");
    }

    [Fact]
    public void ImplicitConversion_ShouldReturnStringValue()
    {
        // Arrange
        var email = Email.Create("test@example.com");

        // Act
        string emailString = email;

        // Assert
        emailString.Should().Be("test@example.com");
    }

    [Fact]
    public void ExplicitConversion_ShouldCreateEmailFromString()
    {
        // Act
        var email = (Email)"test@example.com";

        // Assert
        email.Value.Should().Be("test@example.com");
    }

    [Fact]
    public void Equality_ShouldReturnTrue_WhenSameEmailValue()
    {
        // Arrange
        var email1 = Email.Create("test@example.com");
        var email2 = Email.Create("test@example.com");

        // Assert
        email1.Equals(email2).Should().BeTrue();
        (email1 == email2).Should().BeTrue();
    }

    [Fact]
    public void Equality_ShouldReturnFalse_WhenDifferentEmailValue()
    {
        // Arrange
        var email1 = Email.Create("test1@example.com");
        var email2 = Email.Create("test2@example.com");

        // Assert
        email1.Equals(email2).Should().BeFalse();
        (email1 != email2).Should().BeTrue();
    }

    [Fact]
    public void ToString_ShouldReturnEmailValue()
    {
        // Arrange
        var email = Email.Create("test@example.com");

        // Act
        var result = email.ToString();

        // Assert
        result.Should().Be("test@example.com");
    }

    [Fact]
    public void GetHashCode_ShouldBeSame_ForSameEmailValue()
    {
        // Arrange
        var email1 = Email.Create("test@example.com");
        var email2 = Email.Create("test@example.com");

        // Assert
        email1.GetHashCode().Should().Be(email2.GetHashCode());
    }
}
