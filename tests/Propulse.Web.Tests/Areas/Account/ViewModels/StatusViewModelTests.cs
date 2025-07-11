using AwesomeAssertions;
using Propulse.Web.Areas.Account.ViewModels;

namespace Propulse.Web.Tests.Areas.Account.ViewModels;

public class StatusViewModelTests
{
    [Fact]
    public void DefaultConstructor_SetsDefaultValues()
    {
        // Act
        var viewModel = new StatusViewModel();

        // Assert
        viewModel.Title.Should().Be(string.Empty);
        viewModel.Message.Should().Be(string.Empty);
        viewModel.StatusType.Should().Be("info");
        viewModel.Details.Should().BeNull();
        viewModel.RedirectUrl.Should().BeNull();
        viewModel.RedirectText.Should().BeNull();
    }

    [Theory]
    [InlineData("Account Confirmation")]
    [InlineData("Email Verification")]
    [InlineData("")]
    public void Title_Property_CanBeSetAndRetrieved(string title)
    {
        // Arrange
        var viewModel = new StatusViewModel();

        // Act
        viewModel.Title = title;

        // Assert
        viewModel.Title.Should().Be(title);
    }

    [Theory]
    [InlineData("Account confirmed successfully")]
    [InlineData("Email confirmation failed")]
    [InlineData("")]
    public void Message_Property_CanBeSetAndRetrieved(string message)
    {
        // Arrange
        var viewModel = new StatusViewModel();

        // Act
        viewModel.Message = message;

        // Assert
        viewModel.Message.Should().Be(message);
    }

    [Theory]
    [InlineData("success")]
    [InlineData("error")]
    [InlineData("warning")]
    [InlineData("info")]
    [InlineData("")]
    public void StatusType_Property_CanBeSetAndRetrieved(string statusType)
    {
        // Arrange
        var viewModel = new StatusViewModel();

        // Act
        viewModel.StatusType = statusType;

        // Assert
        viewModel.StatusType.Should().Be(statusType);
    }

    [Theory]
    [InlineData("Please check your email for further instructions")]
    [InlineData("Contact support if the problem persists")]
    [InlineData("")]
    [InlineData(null)]
    public void Details_Property_CanBeSetAndRetrieved(string? details)
    {
        // Arrange
        var viewModel = new StatusViewModel();

        // Act
        viewModel.Details = details;

        // Assert
        viewModel.Details.Should().Be(details);
    }

    [Theory]
    [InlineData("https://example.com/login")]
    [InlineData("/Account/Login")]
    [InlineData("")]
    [InlineData(null)]
    public void RedirectUrl_Property_CanBeSetAndRetrieved(string? redirectUrl)
    {
        // Arrange
        var viewModel = new StatusViewModel();

        // Act
        viewModel.RedirectUrl = redirectUrl;

        // Assert
        viewModel.RedirectUrl.Should().Be(redirectUrl);
    }

    [Theory]
    [InlineData("Go to Login")]
    [InlineData("Return to Home")]
    [InlineData("")]
    [InlineData(null)]
    public void RedirectText_Property_CanBeSetAndRetrieved(string? redirectText)
    {
        // Arrange
        var viewModel = new StatusViewModel();

        // Act
        viewModel.RedirectText = redirectText;

        // Assert
        viewModel.RedirectText.Should().Be(redirectText);
    }

    [Fact]
    public void StatusType_DefaultValue_IsInfo()
    {
        // Act
        var viewModel = new StatusViewModel();

        // Assert
        viewModel.StatusType.Should().Be("info");
    }

    [Fact]
    public void Title_DefaultValue_IsEmptyString()
    {
        // Act
        var viewModel = new StatusViewModel();

        // Assert
        viewModel.Title.Should().Be(string.Empty);
    }

    [Fact]
    public void Message_DefaultValue_IsEmptyString()
    {
        // Act
        var viewModel = new StatusViewModel();

        // Assert
        viewModel.Message.Should().Be(string.Empty);
    }

    [Fact]
    public void NullableProperties_DefaultValue_IsNull()
    {
        // Act
        var viewModel = new StatusViewModel();

        // Assert
        viewModel.Details.Should().BeNull();
        viewModel.RedirectUrl.Should().BeNull();
        viewModel.RedirectText.Should().BeNull();
    }

    [Fact]
    public void AllProperties_CanBeSetIndependently()
    {
        // Arrange
        var viewModel = new StatusViewModel();

        // Act
        viewModel.Title = "Email Confirmation";
        viewModel.Message = "Your account has been confirmed";
        viewModel.StatusType = "success";
        viewModel.Details = "You can now log in to your account";
        viewModel.RedirectUrl = "/Account/Login";
        viewModel.RedirectText = "Go to Login";

        // Assert
        viewModel.Title.Should().Be("Email Confirmation");
        viewModel.Message.Should().Be("Your account has been confirmed");
        viewModel.StatusType.Should().Be("success");
        viewModel.Details.Should().Be("You can now log in to your account");
        viewModel.RedirectUrl.Should().Be("/Account/Login");
        viewModel.RedirectText.Should().Be("Go to Login");
    }

    [Fact]
    public void MultipleInstances_AreIndependent()
    {
        // Arrange
        var viewModel1 = new StatusViewModel();
        var viewModel2 = new StatusViewModel();

        // Act
        viewModel1.Title = "Success";
        viewModel1.Message = "Operation completed";
        viewModel1.StatusType = "success";

        viewModel2.Title = "Error";
        viewModel2.Message = "Operation failed";
        viewModel2.StatusType = "error";

        // Assert
        viewModel1.Title.Should().Be("Success");
        viewModel1.Message.Should().Be("Operation completed");
        viewModel1.StatusType.Should().Be("success");

        viewModel2.Title.Should().Be("Error");
        viewModel2.Message.Should().Be("Operation failed");
        viewModel2.StatusType.Should().Be("error");
    }

    [Fact]
    public void ObjectIsValid_WithValidStatusTypes()
    {
        // Arrange & Act
        var successViewModel = new StatusViewModel { StatusType = "success" };
        var errorViewModel = new StatusViewModel { StatusType = "error" };
        var warningViewModel = new StatusViewModel { StatusType = "warning" };
        var infoViewModel = new StatusViewModel { StatusType = "info" };

        // Assert
        successViewModel.StatusType.Should().Be("success");
        errorViewModel.StatusType.Should().Be("error");
        warningViewModel.StatusType.Should().Be("warning");
        infoViewModel.StatusType.Should().Be("info");
    }

    [Fact]
    public void EmptyStrings_AreHandledCorrectly()
    {
        // Arrange & Act
        var viewModel = new StatusViewModel
        {
            Title = "",
            Message = "",
            StatusType = "",
            Details = "",
            RedirectUrl = "",
            RedirectText = ""
        };

        // Assert
        viewModel.Title.Should().Be("");
        viewModel.Message.Should().Be("");
        viewModel.StatusType.Should().Be("");
        viewModel.Details.Should().Be("");
        viewModel.RedirectUrl.Should().Be("");
        viewModel.RedirectText.Should().Be("");
    }

    [Fact]
    public void LongStrings_AreHandledCorrectly()
    {
        // Arrange
        var longString = new string('x', 1000);

        // Act
        var viewModel = new StatusViewModel
        {
            Title = longString,
            Message = longString,
            Details = longString,
            RedirectUrl = longString,
            RedirectText = longString
        };

        // Assert
        viewModel.Title.Should().Be(longString);
        viewModel.Message.Should().Be(longString);
        viewModel.Details.Should().Be(longString);
        viewModel.RedirectUrl.Should().Be(longString);
        viewModel.RedirectText.Should().Be(longString);
    }

    [Fact]
    public void WhitespaceStrings_ArePreserved()
    {
        // Arrange
        var whitespaceString = "   ";
        var tabString = "\t\t";
        var newlineString = "\n\r\n";

        // Act
        var viewModel = new StatusViewModel
        {
            Title = whitespaceString,
            Message = tabString,
            Details = newlineString
        };

        // Assert
        viewModel.Title.Should().Be(whitespaceString);
        viewModel.Message.Should().Be(tabString);
        viewModel.Details.Should().Be(newlineString);
    }

    [Fact]
    public void CompleteStatusViewModel_CanBeCreated()
    {
        // Act
        var viewModel = new StatusViewModel
        {
            Title = "Account Confirmed",
            Message = "Your email address has been confirmed successfully.",
            StatusType = "success",
            Details = "You can now access all features of your account.",
            RedirectUrl = "/Dashboard",
            RedirectText = "Go to Dashboard"
        };

        // Assert
        viewModel.Title.Should().Be("Account Confirmed");
        viewModel.Message.Should().Be("Your email address has been confirmed successfully.");
        viewModel.StatusType.Should().Be("success");
        viewModel.Details.Should().Be("You can now access all features of your account.");
        viewModel.RedirectUrl.Should().Be("/Dashboard");
        viewModel.RedirectText.Should().Be("Go to Dashboard");
    }
}
