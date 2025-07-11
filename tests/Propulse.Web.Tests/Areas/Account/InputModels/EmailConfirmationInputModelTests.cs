using System.ComponentModel.DataAnnotations;
using AwesomeAssertions;
using Propulse.Web.Areas.Account.InputModels;

namespace Propulse.Web.Tests.Areas.Account.InputModels;

public class EmailConfirmationInputModelTests
{
    [Fact]
    public void DefaultConstructor_SetsDefaultValues()
    {
        // Act
        var model = new EmailConfirmationInputModel();

        // Assert
        model.Code.Should().BeEmpty();
    }

    [Fact]
    public void CopyConstructor_CopiesAllProperties()
    {
        // Arrange
        var original = new EmailConfirmationInputModel
        {
            Code = "test-confirmation-code-123"
        };

        // Act
        var copy = new EmailConfirmationInputModel(original);

        // Assert
        copy.Code.Should().Be(original.Code);
        copy.Should().NotBeSameAs(original);
    }

    [Fact]
    public void CopyConstructor_WithNullSource_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new EmailConfirmationInputModel(null!));
    }

    [Fact]
    public void Code_Property_CanBeSetAndRetrieved()
    {
        // Arrange
        var model = new EmailConfirmationInputModel();
        const string expectedCode = "abc123def456";

        // Act
        model.Code = expectedCode;

        // Assert
        model.Code.Should().Be(expectedCode);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void ValidationAttributes_RequiredCode_FailsForInvalidValues(string? invalidCode)
    {
        // Arrange
        var model = new EmailConfirmationInputModel { Code = invalidCode! };
        var context = new ValidationContext(model);
        var results = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateObject(model, context, results, true);

        // Assert
        isValid.Should().BeFalse();
        results.Should().HaveCount(1);
        results[0].ErrorMessage.Should().Be("Confirmation code is required.");
        results[0].MemberNames.Should().Contain("Code");
    }

    [Fact]
    public void ValidationAttributes_ValidCode_PassesValidation()
    {
        // Arrange
        var model = new EmailConfirmationInputModel { Code = "valid-code-123" };
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
        var property = typeof(EmailConfirmationInputModel).GetProperty(nameof(EmailConfirmationInputModel.Code));
        var requiredAttribute = property!.GetCustomAttributes(typeof(RequiredAttribute), false)
            .Cast<RequiredAttribute>()
            .First();

        // Assert
        requiredAttribute.ErrorMessage.Should().Be("Confirmation code is required.");
    }

    [Fact]
    public void CopyConstructor_ModifyingCopy_DoesNotAffectOriginal()
    {
        // Arrange
        var original = new EmailConfirmationInputModel { Code = "original-code" };
        var copy = new EmailConfirmationInputModel(original);

        // Act
        copy.Code = "modified-code";

        // Assert
        original.Code.Should().Be("original-code");
        copy.Code.Should().Be("modified-code");
    }
}
