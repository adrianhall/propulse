using AwesomeAssertions;
using Propulse.Web.Areas.Account.InputModels;
using Propulse.Web.Areas.Account.ViewModels;

namespace Propulse.Web.Tests.Areas.Account.ViewModels;

public class ResendConfirmationViewModelTests
{
    [Fact]
    public void DefaultConstructor_SetsDefaultValues()
    {
        // Act
        var viewModel = new ResendConfirmationViewModel();

        // Assert
        viewModel.Email.Should().BeEmpty();
        viewModel.StatusMessage.Should().BeNull();
        viewModel.StatusType.Should().Be("info");
    }

    [Fact]
    public void ConstructorWithInputModel_CopiesInputModelProperties()
    {
        // Arrange
        var inputModel = new ResendConfirmationInputModel
        {
            Email = "test@example.com"
        };

        // Act
        var viewModel = new ResendConfirmationViewModel(inputModel);

        // Assert
        viewModel.Email.Should().Be(inputModel.Email);
        viewModel.StatusMessage.Should().BeNull();
        viewModel.StatusType.Should().Be("info");
    }

    [Fact]
    public void ConstructorWithInputModel_WithNullInputModel_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ResendConfirmationViewModel(null!));
    }

    [Theory]
    [InlineData("Email sent successfully")]
    [InlineData("Failed to send email")]
    [InlineData(null)]
    public void StatusMessage_Property_CanBeSetAndRetrieved(string? statusMessage)
    {
        // Arrange
        var viewModel = new ResendConfirmationViewModel();

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
        var viewModel = new ResendConfirmationViewModel();

        // Act
        viewModel.StatusType = statusType;

        // Assert
        viewModel.StatusType.Should().Be(statusType);
    }

    [Fact]
    public void InheritsFromResendConfirmationInputModel()
    {
        // Arrange & Act
        var viewModel = new ResendConfirmationViewModel();

        // Assert
        viewModel.Should().BeAssignableTo<ResendConfirmationInputModel>();
    }

    [Fact]
    public void ConstructorWithInputModel_ModifyingViewModel_DoesNotAffectInputModel()
    {
        // Arrange
        var inputModel = new ResendConfirmationInputModel { Email = "original@example.com" };
        var viewModel = new ResendConfirmationViewModel(inputModel);

        // Act
        viewModel.Email = "modified@example.com";
        viewModel.StatusMessage = "Test message";
        viewModel.StatusType = "success";

        // Assert
        inputModel.Email.Should().Be("original@example.com");
        viewModel.Email.Should().Be("modified@example.com");
        viewModel.StatusMessage.Should().Be("Test message");
        viewModel.StatusType.Should().Be("success");
    }

    [Fact]
    public void AllProperties_CanBeSetIndependently()
    {
        // Arrange
        var viewModel = new ResendConfirmationViewModel();

        // Act
        viewModel.Email = "user@domain.com";
        viewModel.StatusMessage = "Email sent successfully";
        viewModel.StatusType = "success";

        // Assert
        viewModel.Email.Should().Be("user@domain.com");
        viewModel.StatusMessage.Should().Be("Email sent successfully");
        viewModel.StatusType.Should().Be("success");
    }

    [Fact]
    public void StatusType_DefaultValue_IsInfo()
    {
        // Act
        var viewModel = new ResendConfirmationViewModel();

        // Assert
        viewModel.StatusType.Should().Be("info");
    }

    [Fact]
    public void CreateFromInputModel_PreservesValidationBehavior()
    {
        // Arrange
        var inputModel = new ResendConfirmationInputModel { Email = "valid@example.com" };
        var viewModel = new ResendConfirmationViewModel(inputModel);

        // Act & Assert
        // The view model should inherit validation attributes from the input model
        viewModel.Should().BeAssignableTo<ResendConfirmationInputModel>();
        viewModel.Email.Should().Be("valid@example.com");
    }

    [Fact]
    public void Email_Property_InheritsValidationFromBaseClass()
    {
        // Arrange
        var viewModel = new ResendConfirmationViewModel();

        // Act
        viewModel.Email = "test@example.com";

        // Assert
        viewModel.Email.Should().Be("test@example.com");
        // The validation behavior is inherited from ResendConfirmationInputModel
        viewModel.Should().BeAssignableTo<ResendConfirmationInputModel>();
    }

    [Fact]
    public void MultipleInstances_AreIndependent()
    {
        // Arrange
        var viewModel1 = new ResendConfirmationViewModel();
        var viewModel2 = new ResendConfirmationViewModel();

        // Act
        viewModel1.Email = "user1@example.com";
        viewModel1.StatusMessage = "Message 1";
        viewModel1.StatusType = "success";

        viewModel2.Email = "user2@example.com";
        viewModel2.StatusMessage = "Message 2";
        viewModel2.StatusType = "error";

        // Assert
        viewModel1.Email.Should().Be("user1@example.com");
        viewModel1.StatusMessage.Should().Be("Message 1");
        viewModel1.StatusType.Should().Be("success");

        viewModel2.Email.Should().Be("user2@example.com");
        viewModel2.StatusMessage.Should().Be("Message 2");
        viewModel2.StatusType.Should().Be("error");
    }
}
