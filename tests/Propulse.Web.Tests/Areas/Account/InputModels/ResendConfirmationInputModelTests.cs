using System.ComponentModel.DataAnnotations;
using AwesomeAssertions;
using Propulse.Core.DataAnnotations;
using Propulse.Web.Areas.Account.InputModels;

namespace Propulse.Web.Tests.Areas.Account.InputModels;

public class ResendConfirmationInputModelTests
{
    [Fact]
    public void DefaultConstructor_SetsDefaultValues()
    {
        // Act
        var model = new ResendConfirmationInputModel();

        // Assert
        model.Email.Should().BeEmpty();
    }

    [Fact]
    public void CopyConstructor_CopiesAllProperties()
    {
        // Arrange
        var original = new ResendConfirmationInputModel
        {
            Email = "test@example.com"
        };

        // Act
        var copy = new ResendConfirmationInputModel(original);

        // Assert
        copy.Email.Should().Be(original.Email);
        copy.Should().NotBeSameAs(original);
    }

    [Fact]
    public void CopyConstructor_WithNullSource_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ResendConfirmationInputModel(null!));
    }

    [Fact]
    public void Email_Property_CanBeSetAndRetrieved()
    {
        // Arrange
        var model = new ResendConfirmationInputModel();
        const string expectedEmail = "user@domain.com";

        // Act
        model.Email = expectedEmail;

        // Assert
        model.Email.Should().Be(expectedEmail);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void ValidationAttributes_RequiredEmail_FailsForEmptyValues(string? invalidEmail)
    {
        // Arrange
        var model = new ResendConfirmationInputModel { Email = invalidEmail! };
        var context = new ValidationContext(model);
        var results = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateObject(model, context, results, true);

        // Assert
        isValid.Should().BeFalse();
        results.Should().HaveCountGreaterThan(0);
        results.Should().Contain(r => r.ErrorMessage == "Email address is required.");
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("@domain.com")]
    [InlineData("user@")]
    [InlineData("user.domain.com")]
    [InlineData("user@domain")]
    public void ValidationAttributes_EmailFormat_FailsForInvalidEmailFormats(string invalidEmail)
    {
        // Arrange
        var model = new ResendConfirmationInputModel { Email = invalidEmail };
        var context = new ValidationContext(model);
        var results = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateObject(model, context, results, true);

        // Assert
        isValid.Should().BeFalse();
        results.Should().Contain(r => r.ErrorMessage == "Please enter a valid email address.");
    }

    [Theory]
    [InlineData("user@example.com")]
    [InlineData("test.email@domain.co.uk")]
    [InlineData("user_tag@subdomain.example.org")]
    [InlineData("firstname.lastname@company.com")]
    public void ValidationAttributes_ValidEmail_PassesValidation(string validEmail)
    {
        // Arrange
        var model = new ResendConfirmationInputModel { Email = validEmail };
        var context = new ValidationContext(model);
        var results = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateObject(model, context, results, true);

        // Assert
        isValid.Should().BeTrue();
        results.Should().BeEmpty();
    }

    [Fact]
    public void ValidationAttributes_RequiredAttribute_HasCorrectErrorMessage()
    {
        // Arrange
        var property = typeof(ResendConfirmationInputModel).GetProperty(nameof(ResendConfirmationInputModel.Email));
        var requiredAttribute = property!.GetCustomAttributes(typeof(RequiredAttribute), false)
            .Cast<RequiredAttribute>()
            .First();

        // Assert
        requiredAttribute.ErrorMessage.Should().Be("Email address is required.");
    }

    [Fact]
    public void ValidationAttributes_PropulseEmailAddressAttribute_HasCorrectErrorMessage()
    {
        // Arrange
        var property = typeof(ResendConfirmationInputModel).GetProperty(nameof(ResendConfirmationInputModel.Email));
        var emailAttribute = property!.GetCustomAttributes(typeof(PropulseEmailAddressAttribute), false)
            .Cast<PropulseEmailAddressAttribute>()
            .First();

        // Assert
        emailAttribute.ErrorMessage.Should().Be("Please enter a valid email address.");
    }

    [Fact]
    public void ValidationAttributes_DisplayAttribute_HasCorrectName()
    {
        // Arrange
        var property = typeof(ResendConfirmationInputModel).GetProperty(nameof(ResendConfirmationInputModel.Email));
        var displayAttribute = property!.GetCustomAttributes(typeof(DisplayAttribute), false)
            .Cast<DisplayAttribute>()
            .First();

        // Assert
        displayAttribute.Name.Should().Be("Email Address");
    }

    [Fact]
    public void CopyConstructor_ModifyingCopy_DoesNotAffectOriginal()
    {
        // Arrange
        var original = new ResendConfirmationInputModel { Email = "original@example.com" };
        var copy = new ResendConfirmationInputModel(original);

        // Act
        copy.Email = "modified@example.com";

        // Assert
        original.Email.Should().Be("original@example.com");
        copy.Email.Should().Be("modified@example.com");
    }
}
