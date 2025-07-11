using AwesomeAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Propulse.Web.Areas.Account.Controllers;
using Propulse.Web.Entities;
using Propulse.Web.Services;

namespace Propulse.Web.Tests.Areas.Account.Controllers;

/// <summary>
/// Unit tests for ConfirmController focusing on exception handling scenarios.
/// These tests use mocked dependencies to isolate the controller logic and
/// verify proper error handling when dependencies throw exceptions.
/// </summary>
public class ConfirmControllerUnitTests
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailSender<ApplicationUser> _emailSender;
    private readonly IResponseLinkService _linkService;
    private readonly FakeLogger<ConfirmController> _logger;
    private readonly ConfirmController _controller;

    public ConfirmControllerUnitTests()
    {
        // Create mocks for all dependencies
        _userManager = CreateMockUserManager();
        _linkService = Substitute.For<IResponseLinkService>();
        _emailSender = Substitute.For<IEmailSender<ApplicationUser>>();
        _logger = new FakeLogger<ConfirmController>();
        
        // Create controller with mocked dependencies - correct parameter order
        _controller = new ConfirmController(_userManager, _linkService, _emailSender, _logger);
        
        // Set up controller context to avoid NullReferenceException on Url.Action calls
        var urlHelper = Substitute.For<IUrlHelper>();
        urlHelper.Action("Resend", "Confirm", new { area = "Account" }).Returns("http://test.com/resend");
        _controller.Url = urlHelper;
    }

    private static UserManager<ApplicationUser> CreateMockUserManager()
    {
        var store = Substitute.For<IUserStore<ApplicationUser>>();
        var userManager = Substitute.For<UserManager<ApplicationUser>>(
            store, null, null, null, null, null, null, null, null);
        return userManager;
    }

    private static ApplicationUser CreateTestUser() => new()
    {
        Id = Guid.NewGuid(),
        Email = "test@example.com",
        EmailConfirmed = false
    };

    [Fact]
    public async Task Email_WhenLinkServiceThrowsException_ReturnsInvalidCodeRedirect()
    {
        // Arrange
        const string code = "invalid-code";
        _linkService.DecodeResponseCode(code).Throws(new InvalidOperationException("Invalid code format"));

        // Act
        var result = await _controller.Email(code);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var redirectResult = (RedirectToActionResult)result;
        redirectResult.ActionName.Should().Be("InvalidCode");
        
        // Verify error was logged
        _logger.Collector.GetSnapshot().Should().Contain(x => 
            x.Level == LogLevel.Error);
    }

    [Fact]
    public async Task Email_WhenUserManagerFindByIdThrowsException_ReturnsInvalidCodeRedirect()
    {
        // Arrange
        const string code = "valid-code";
        var userId = Guid.NewGuid();
        const string token = "confirmation-token";
        
        _linkService.DecodeResponseCode(code).Returns((userId, token));
        _userManager.FindByIdAsync(userId.ToString()).Throws(new InvalidOperationException("Database connection error"));

        // Act
        var result = await _controller.Email(code);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var redirectResult = (RedirectToActionResult)result;
        redirectResult.ActionName.Should().Be("InvalidCode");
        
        // Verify error was logged
        _logger.Collector.GetSnapshot().Should().Contain(x => 
            x.Level == LogLevel.Error);
    }

    [Fact]
    public async Task Email_WhenConfirmEmailAsyncThrowsException_ReturnsInvalidCodeRedirect()
    {
        // Arrange
        const string code = "valid-code";
        var user = CreateTestUser();
        const string token = "confirmation-token";
        
        _linkService.DecodeResponseCode(code).Returns((user.Id, token));
        _userManager.FindByIdAsync(user.Id.ToString()).Returns(user);
        _userManager.ConfirmEmailAsync(user, token).Throws(new InvalidOperationException("Token validation failed"));

        // Act
        var result = await _controller.Email(code);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var redirectResult = (RedirectToActionResult)result;
        redirectResult.ActionName.Should().Be("InvalidCode");
        
        // Verify error was logged
        _logger.Collector.GetSnapshot().Should().Contain(x => 
            x.Level == LogLevel.Error);
    }

    [Fact]
    public async Task Email_HappyPath_ReturnsSuccessRedirect()
    {
        // Arrange
        const string code = "valid-code";
        var user = CreateTestUser();
        const string token = "confirmation-token";
        
        _linkService.DecodeResponseCode(code).Returns((user.Id, token));
        _userManager.FindByIdAsync(user.Id.ToString()).Returns(user);
        _userManager.ConfirmEmailAsync(user, token).Returns(IdentityResult.Success);

        // Act
        var result = await _controller.Email(code);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var redirectResult = (RedirectToActionResult)result;
        redirectResult.ActionName.Should().Be("Success");
    }

    [Fact]
    public async Task Email_WhenUserManagerThrowsUnexpectedException_LogsErrorAndReturnsInvalidCode()
    {
        // Arrange
        const string code = "test-code";
        var userId = Guid.NewGuid();
        const string token = "test-token";
        
        _linkService.DecodeResponseCode(code).Returns((userId, token));
        _userManager.FindByIdAsync(userId.ToString()).Throws(new OutOfMemoryException("System error"));

        // Act
        var result = await _controller.Email(code);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var redirectResult = (RedirectToActionResult)result;
        redirectResult.ActionName.Should().Be("InvalidCode");
        
        // Verify error logging occurred
        _logger.Collector.GetSnapshot().Should().Contain(x => 
            x.Level == LogLevel.Error);
    }

    [Fact]
    public async Task Email_WhenConfirmEmailReturnsFailure_LogsWarningAndRedirectsToStatus()
    {
        // Arrange
        const string code = "test-code";
        var user = CreateTestUser();
        const string token = "test-token";
        
        _linkService.DecodeResponseCode(code).Returns((user.Id, token));
        _userManager.FindByIdAsync(user.Id.ToString()).Returns(user);
        
        // Create a proper failed result with errors collection
        var identityErrors = new List<IdentityError>
        {
            new IdentityError { Code = "InvalidToken", Description = "Invalid token" }
        };
        var failedResult = IdentityResult.Failed(identityErrors.ToArray());
        _userManager.ConfirmEmailAsync(user, token).Returns(failedResult);

        // Act
        var result = await _controller.Email(code);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var redirectResult = (RedirectToActionResult)result;
        redirectResult.ActionName.Should().Be("Status");
        
        // Verify warning was logged
        _logger.Collector.GetSnapshot().Should().Contain(x => 
            x.Level == LogLevel.Warning);
    }

    [Fact]
    public async Task Email_WhenLinkServiceThrowsArgumentException_LogsErrorAndReturnsInvalidCode()
    {
        // Arrange
        const string code = "malformed-code";
        _linkService.DecodeResponseCode(code).Throws(new ArgumentException("Invalid code format"));

        // Act
        var result = await _controller.Email(code);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var redirectResult = (RedirectToActionResult)result;
        redirectResult.ActionName.Should().Be("InvalidCode");
        
        // Verify error was logged
        _logger.Collector.GetSnapshot().Should().Contain(x => 
            x.Level == LogLevel.Error);
    }

    [Fact]
    public async Task Email_WhenTokenIsNull_LogsWarningAndRedirectsToAccountNotFound()
    {
        // Arrange
        const string code = "test-code";
        var userId = Guid.NewGuid();
        
        _linkService.DecodeResponseCode(code).Returns((userId, (string?)null));
        _userManager.FindByIdAsync(userId.ToString()).Returns((ApplicationUser?)null);

        // Act
        var result = await _controller.Email(code);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var redirectResult = (RedirectToActionResult)result;
        redirectResult.ActionName.Should().Be("AccountNotFound");
        
        // Verify warning was logged
        _logger.Collector.GetSnapshot().Should().Contain(x => 
            x.Level == LogLevel.Warning);
    }
}