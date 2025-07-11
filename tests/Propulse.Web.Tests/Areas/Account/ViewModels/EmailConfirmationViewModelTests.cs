using AwesomeAssertions;
using Propulse.Web.Areas.Account.InputModels;
using Propulse.Web.Areas.Account.ViewModels;

namespace Propulse.Web.Tests.Areas.Account.ViewModels;

public class EmailConfirmationViewModelTests
{
    [Fact]
    public void DefaultConstructor_SetsDefaultValues()
    {
        // Act
        var viewModel = new EmailConfirmationViewModel();

        // Assert
        viewModel.Code.Should().BeEmpty();
        viewModel.StatusMessage.Should().BeNull();
        viewModel.StatusType.Should().Be("info");
    }

    [Fact]
    public void ConstructorWithInputModel_CopiesInputModelProperties()
    {
        // Arrange
        var inputModel = new EmailConfirmationInputModel
        {
            Code = "test-confirmation-code"
        };

        // Act
        var viewModel = new EmailConfirmationViewModel(inputModel);

        // Assert
        viewModel.Code.Should().Be(inputModel.Code);
        viewModel.StatusMessage.Should().BeNull();
        viewModel.StatusType.Should().Be("info");
    }

    [Fact]
    public void ConstructorWithInputModel_WithNullInputModel_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new EmailConfirmationViewModel(null!));
    }

    [Theory]
    [InlineData("Email confirmed successfully")]
    [InlineData("Confirmation failed")]
    [InlineData(null)]
    public void StatusMessage_Property_CanBeSetAndRetrieved(string? statusMessage)
    {
        // Arrange
        var viewModel = new EmailConfirmationViewModel();

        // Act
        viewModel.StatusMessage = statusMessage;

        // Assert
        viewModel.StatusMessage.Should().Be(statusMessage);
    }

    [Theory]
    [InlineData("success")]
    [InlineData("error")]
    [InlineData("warning")]
    [InlineData("info")]
    public void StatusType_Property_CanBeSetAndRetrieved(string statusType)
    {
        // Arrange
        var viewModel = new EmailConfirmationViewModel();

        // Act
        viewModel.StatusType = statusType;

        // Assert
        viewModel.StatusType.Should().Be(statusType);
    }

    [Fact]
    public void InheritsFromEmailConfirmationInputModel()
    {
        // Arrange & Act
        var viewModel = new EmailConfirmationViewModel();

        // Assert
        viewModel.Should().BeAssignableTo<EmailConfirmationInputModel>();
    }

    [Fact]
    public void ConstructorWithInputModel_ModifyingViewModel_DoesNotAffectInputModel()
    {
        // Arrange
        var inputModel = new EmailConfirmationInputModel { Code = "original-code" };
        var viewModel = new EmailConfirmationViewModel(inputModel);

        // Act
        viewModel.Code = "modified-code";
        viewModel.StatusMessage = "Test message";
        viewModel.StatusType = "success";

        // Assert
        inputModel.Code.Should().Be("original-code");
        viewModel.Code.Should().Be("modified-code");
        viewModel.StatusMessage.Should().Be("Test message");
        viewModel.StatusType.Should().Be("success");
    }

    [Fact]
    public void AllProperties_CanBeSetIndependently()
    {
        // Arrange
        var viewModel = new EmailConfirmationViewModel();

        // Act
        viewModel.Code = "test-code-123";
        viewModel.StatusMessage = "Confirmation successful";
        viewModel.StatusType = "success";

        // Assert
        viewModel.Code.Should().Be("test-code-123");
        viewModel.StatusMessage.Should().Be("Confirmation successful");
        viewModel.StatusType.Should().Be("success");
    }

    [Fact]
    public void StatusType_DefaultValue_IsInfo()
    {
        // Act
        var viewModel = new EmailConfirmationViewModel();

        // Assert
        viewModel.StatusType.Should().Be("info");
    }

    [Fact]
    public void CreateFromInputModel_PreservesValidationBehavior()
    {
        // Arrange
        var inputModel = new EmailConfirmationInputModel { Code = "valid-code" };
        var viewModel = new EmailConfirmationViewModel(inputModel);

        // Act & Assert
        // The view model should inherit validation attributes from the input model
        viewModel.Should().BeAssignableTo<EmailConfirmationInputModel>();
        viewModel.Code.Should().Be("valid-code");
    }
}
