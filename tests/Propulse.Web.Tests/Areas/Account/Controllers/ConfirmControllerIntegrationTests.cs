using AngleSharp;
using AngleSharp.Html.Dom;
using AwesomeAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Propulse.Web.Entities;
using Propulse.Web.Events;
using Propulse.Web.Services;
using Propulse.Web.Tests.Helpers;
using System.Net;
using System.Text;

namespace Propulse.Web.Tests.Areas.Account.Controllers;

/// <summary>
/// Integration tests for the ConfirmController to verify real-world user scenarios and security threat responses.
/// Tests full HTTP request/response cycles including redirects, form submissions, and email link generation.
/// </summary>
[Collection(SerializedCollection.Name)]
public class ConfirmControllerIntegrationTests
{
    private readonly WebServiceFixture _fixture;
    private readonly IBrowsingContext _browsingContext;

    public ConfirmControllerIntegrationTests(WebServiceFixture fixture)
    {
        _fixture = fixture;
        _browsingContext = BrowsingContext.New(Configuration.Default);
    }

    /// <summary>
    /// Helper method to execute code within a service scope for accessing scoped services like UserManager.
    /// </summary>
    private async Task<T> WithScopeAsync<T>(Func<IServiceProvider, Task<T>> action)
    {
        using var scope = _fixture.Services.CreateScope();
        return await action(scope.ServiceProvider);
    }

    /// <summary>
    /// Helper method to execute code within a service scope for accessing scoped services like UserManager.
    /// </summary>
    private async Task WithScopeAsync(Func<IServiceProvider, Task> action)
    {
        using var scope = _fixture.Services.CreateScope();
        await action(scope.ServiceProvider);
    }

    [Fact]
    public async Task EmailConfirmation_WithValidCode_RedirectsToSuccessPage()
    {
        // Arrange
        using var client = _fixture.CreateClient();
        
        var (code, userId) = await WithScopeAsync(async services =>
        {
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var linkService = services.GetRequiredService<IResponseLinkService>();

            // Create a test user
            var user = new ApplicationUser("test@example.com");
            var createResult = await userManager.CreateAsync(user, "TestPassword123!");
            createResult.Succeeded.Should().BeTrue();

            // Generate confirmation token and code
            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
            var generatedCode = linkService.EncodeResponseCode(user.Id, token);
            
            return (generatedCode, user.Id);
        });

        // Act
        var response = await client.GetAsync($"/Account/Confirm/Email?code={Uri.EscapeDataString(code)}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.RequestMessage!.RequestUri!.LocalPath.Should().Be("/Account/Confirm/Success");
        
        // Verify query parameters for success page
        var query = System.Web.HttpUtility.ParseQueryString(response.RequestMessage.RequestUri.Query);
        query["message"].Should().Be("Thank you for confirming your email address. Your account is now active.");
        query["type"].Should().Be("success");

        // Verify user is now confirmed
        await WithScopeAsync(async services =>
        {
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var updatedUser = await userManager.FindByIdAsync(userId.ToString());
            updatedUser!.EmailConfirmed.Should().BeTrue();
        });
    }

    [Fact]
    public async Task EmailConfirmation_WithNullCode_RedirectsToStatusPageWithError()
    {
        // Arrange
        using var client = _fixture.CreateClient();

        // Act
        var response = await client.GetAsync("/Account/Confirm/Email");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.RequestMessage!.RequestUri!.LocalPath.Should().Be("/Account/Confirm/Status");
        
        // Verify error parameters
        var query = System.Web.HttpUtility.ParseQueryString(response.RequestMessage.RequestUri.Query);
        query["title"].Should().Be("Invalid Confirmation Link");
        query["message"].Should().Be("Invalid confirmation link. Please check your email for the correct link.");
        query["type"].Should().Be("error");
        query["details"].Should().Be("The confirmation link appears to be malformed or incomplete.");
        query["redirectText"].Should().Be("Request a new confirmation email");
    }

    [Fact]
    public async Task EmailConfirmation_WithEmptyCode_RedirectsToStatusPageWithError()
    {
        // Arrange
        using var client = _fixture.CreateClient();

        // Act
        var response = await client.GetAsync("/Account/Confirm/Email?code=");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.RequestMessage!.RequestUri!.LocalPath.Should().Be("/Account/Confirm/Status");
        
        var query = System.Web.HttpUtility.ParseQueryString(response.RequestMessage.RequestUri.Query);
        query["title"].Should().Be("Invalid Confirmation Link");
        query["type"].Should().Be("error");
    }

    [Fact]
    public async Task EmailConfirmation_WithInvalidCode_RedirectsToInvalidCodePage()
    {
        // Arrange
        using var client = _fixture.CreateClient();

        // Act
        var response = await client.GetAsync("/Account/Confirm/Email?code=invalid-code-123");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.RequestMessage!.RequestUri!.LocalPath.Should().Be("/Account/Confirm/InvalidCode");
    }

    [Fact]
    public async Task EmailConfirmation_WithNonExistentUser_RedirectsToAccountNotFound()
    {
        // Arrange
        using var client = _fixture.CreateClient();
        
        var code = await WithScopeAsync(services =>
        {
            var linkService = services.GetRequiredService<IResponseLinkService>();

            // Generate code for non-existent user
            var fakeUserId = Guid.CreateVersion7();
            var fakeToken = "fake-token";
            return Task.FromResult(linkService.EncodeResponseCode(fakeUserId, fakeToken));
        });

        // Act
        var response = await client.GetAsync($"/Account/Confirm/Email?code={Uri.EscapeDataString(code)}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.RequestMessage!.RequestUri!.LocalPath.Should().Be("/Account/Confirm/AccountNotFound");
    }

    [Fact]
    public async Task EmailConfirmation_WithAlreadyConfirmedUser_RedirectsToSuccessWithInfoMessage()
    {
        // Arrange
        using var client = _fixture.CreateClient();
        
        var code = await WithScopeAsync(async services =>
        {
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var linkService = services.GetRequiredService<IResponseLinkService>();

            // Create and confirm a user
            var user = new ApplicationUser("confirmed@example.com");
            var createResult = await userManager.CreateAsync(user, "TestPassword123!");
            createResult.Succeeded.Should().BeTrue();

            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
            var confirmResult = await userManager.ConfirmEmailAsync(user, token);
            confirmResult.Succeeded.Should().BeTrue();

            // Generate new code for already confirmed user
            var newToken = await userManager.GenerateEmailConfirmationTokenAsync(user);
            return linkService.EncodeResponseCode(user.Id, newToken);
        });

        // Act
        var response = await client.GetAsync($"/Account/Confirm/Email?code={Uri.EscapeDataString(code)}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.RequestMessage!.RequestUri!.LocalPath.Should().Be("/Account/Confirm/Success");
        
        var query = System.Web.HttpUtility.ParseQueryString(response.RequestMessage.RequestUri.Query);
        query["message"].Should().Be("Your email address has already been confirmed.");
        query["type"].Should().Be("info");
    }

    [Fact]
    public async Task EmailConfirmation_WithExpiredToken_RedirectsToStatusPageWithError()
    {
        // Arrange
        using var client = _fixture.CreateClient();
        
        var code = await WithScopeAsync(async services =>
        {
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var linkService = services.GetRequiredService<IResponseLinkService>();

            // Create a test user
            var user = new ApplicationUser("expired@example.com");
            var createResult = await userManager.CreateAsync(user, "TestPassword123!");
            createResult.Succeeded.Should().BeTrue();

            // Use an obviously invalid/expired token
            var expiredToken = "expired-token-that-will-fail";
            return linkService.EncodeResponseCode(user.Id, expiredToken);
        });

        // Act
        var response = await client.GetAsync($"/Account/Confirm/Email?code={Uri.EscapeDataString(code)}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.RequestMessage!.RequestUri!.LocalPath.Should().Be("/Account/Confirm/Status");
        
        var query = System.Web.HttpUtility.ParseQueryString(response.RequestMessage.RequestUri.Query);
        query["title"].Should().Be("Email Confirmation Failed");
        query["message"].Should().Be("There was an error confirming your email address. The link may have expired.");
        query["type"].Should().Be("error");
        query["redirectText"].Should().Be("Request a new confirmation email");
    }

    [Fact]
    public async Task ResendConfirmation_GetRequest_ReturnsFormPage()
    {
        // Arrange
        using var client = _fixture.CreateClient();

        // Act
        var response = await client.GetAsync("/Account/Confirm/Resend");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var document = await _browsingContext.OpenAsync(req => req.Content(content));
        
        // Verify form exists
        var form = document.QuerySelector("form") as IHtmlFormElement;
        form.Should().NotBeNull();
        
        // Verify email input exists
        var emailInput = document.QuerySelector("input[name='Email']") as IHtmlInputElement;
        emailInput.Should().NotBeNull();
        emailInput!.Type.Should().Be("email");
        emailInput.IsRequired.Should().BeTrue();
        
        // Verify submit button exists
        var submitButton = document.QuerySelector("button[type='submit'], input[type='submit']");
        submitButton.Should().NotBeNull();
        
        // Verify anti-forgery token exists
        var antiForgeryToken = document.QuerySelector("input[name='__RequestVerificationToken']") as IHtmlInputElement;
        antiForgeryToken.Should().NotBeNull();
    }

    [Fact]
    public async Task ResendConfirmation_WithValidEmail_SendsEmailAndShowsSuccessMessage()
    {
        // Arrange
        using var client = _fixture.CreateClient();
        var emailSender = _fixture.Services.GetRequiredService<IEmailSender<ApplicationUser>>() as NullEmailSender<ApplicationUser>;
        
        await WithScopeAsync(async services =>
        {
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            
            // Create unconfirmed user
            var user = new ApplicationUser("resend@example.com");
            var createResult = await userManager.CreateAsync(user, "TestPassword123!");
            createResult.Succeeded.Should().BeTrue();
        });

        // Set up email capture
        EmailSenderEventArgs? capturedEmailEvent = null;
        emailSender!.EmailSent += (sender, args) => capturedEmailEvent = args;

        // Get the form page first to extract anti-forgery token
        var getResponse = await client.GetAsync("/Account/Confirm/Resend");
        var getContent = await getResponse.Content.ReadAsStringAsync();
        var getDocument = await _browsingContext.OpenAsync(req => req.Content(getContent));
        var antiForgeryToken = (getDocument.QuerySelector("input[name='__RequestVerificationToken']") as IHtmlInputElement)?.Value;

        // Prepare form data
        var formData = new List<KeyValuePair<string, string>>
        {
            new("Email", "resend@example.com"),
            new("__RequestVerificationToken", antiForgeryToken!)
        };

        // Act
        var response = await client.PostAsync("/Account/Confirm/Resend", 
            new FormUrlEncodedContent(formData));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Verify email was sent
        capturedEmailEvent.Should().NotBeNull();
        capturedEmailEvent!.EventType.Should().Be(EmailSenderEventType.AccountConfirmationLink);
        capturedEmailEvent.Email.Should().Be("resend@example.com");
        capturedEmailEvent.LinkOrCode.Should().NotBeEmpty();
        
        // Verify response content shows success message
        var content = await response.Content.ReadAsStringAsync();
        var document = await _browsingContext.OpenAsync(req => req.Content(content));
        
        var statusMessage = document.QuerySelector(".alert-success, .alert.alert-success");
        statusMessage.Should().NotBeNull();
        statusMessage!.TextContent.Should().Contain("confirmation email has been sent");
    }

    [Fact]
    public async Task ResendConfirmation_WithNonExistentEmail_ShowsGenericMessage()
    {
        // Arrange
        using var client = _fixture.CreateClient();
        var emailSender = _fixture.Services.GetRequiredService<IEmailSender<ApplicationUser>>() as NullEmailSender<ApplicationUser>;
        
        // Set up email capture
        EmailSenderEventArgs? capturedEmailEvent = null;
        emailSender!.EmailSent += (sender, args) => capturedEmailEvent = args;

        // Get anti-forgery token
        var getResponse = await client.GetAsync("/Account/Confirm/Resend");
        var getContent = await getResponse.Content.ReadAsStringAsync();
        var getDocument = await _browsingContext.OpenAsync(req => req.Content(getContent));
        var antiForgeryToken = (getDocument.QuerySelector("input[name='__RequestVerificationToken']") as IHtmlInputElement)?.Value;

        // Prepare form data with non-existent email
        var formData = new List<KeyValuePair<string, string>>
        {
            new("Email", "nonexistent@example.com"),
            new("__RequestVerificationToken", antiForgeryToken!)
        };

        // Act
        var response = await client.PostAsync("/Account/Confirm/Resend", 
            new FormUrlEncodedContent(formData));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Verify no email was sent
        capturedEmailEvent.Should().BeNull();
        
        // Verify generic security message is shown
        var content = await response.Content.ReadAsStringAsync();
        var document = await _browsingContext.OpenAsync(req => req.Content(content));
        
        var statusMessage = document.QuerySelector(".alert-info, .alert.alert-info");
        statusMessage.Should().NotBeNull();
        statusMessage!.TextContent.Should().Contain("If an account with that email address exists");
    }

    [Fact]
    public async Task ResendConfirmation_WithAlreadyConfirmedEmail_ShowsInfoMessage()
    {
        // Arrange
        using var client = _fixture.CreateClient();
        var emailSender = _fixture.Services.GetRequiredService<IEmailSender<ApplicationUser>>() as NullEmailSender<ApplicationUser>;

        await WithScopeAsync(async services =>
        {
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

            // Create and confirm user
            var user = new ApplicationUser("alreadyconfirmed@example.com");
            var createResult = await userManager.CreateAsync(user, "TestPassword123!");
            createResult.Succeeded.Should().BeTrue();

            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
            var confirmResult = await userManager.ConfirmEmailAsync(user, token);
            confirmResult.Succeeded.Should().BeTrue();
        });

        // Set up email capture
        EmailSenderEventArgs? capturedEmailEvent = null;
        emailSender!.EmailSent += (sender, args) => capturedEmailEvent = args;

        // Get anti-forgery token
        var getResponse = await client.GetAsync("/Account/Confirm/Resend");
        var getContent = await getResponse.Content.ReadAsStringAsync();
        var getDocument = await _browsingContext.OpenAsync(req => req.Content(getContent));
        var antiForgeryToken = (getDocument.QuerySelector("input[name='__RequestVerificationToken']") as IHtmlInputElement)?.Value;

        // Prepare form data
        var formData = new List<KeyValuePair<string, string>>
        {
            new("Email", "alreadyconfirmed@example.com"),
            new("__RequestVerificationToken", antiForgeryToken!)
        };

        // Act
        var response = await client.PostAsync("/Account/Confirm/Resend", 
            new FormUrlEncodedContent(formData));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Verify no email was sent
        capturedEmailEvent.Should().BeNull();
        
        // Verify already confirmed message
        var content = await response.Content.ReadAsStringAsync();
        var document = await _browsingContext.OpenAsync(req => req.Content(content));
        
        var statusMessage = document.QuerySelector(".alert-info, .alert.alert-info");
        statusMessage.Should().NotBeNull();
        statusMessage!.TextContent.Should().Contain("already been confirmed");
    }

    [Fact]
    public async Task ResendConfirmation_WithInvalidEmail_ShowsValidationErrors()
    {
        // Arrange
        using var client = _fixture.CreateClient();

        // Get anti-forgery token
        var getResponse = await client.GetAsync("/Account/Confirm/Resend");
        var getContent = await getResponse.Content.ReadAsStringAsync();
        var getDocument = await _browsingContext.OpenAsync(req => req.Content(getContent));
        var antiForgeryToken = (getDocument.QuerySelector("input[name='__RequestVerificationToken']") as IHtmlInputElement)?.Value;

        // Test cases for invalid emails
        var invalidEmails = new[] { "", "invalid-email", "test@", "@example.com", "test..test@example.com" };

        foreach (var invalidEmail in invalidEmails)
        {
            // Prepare form data with invalid email
            var formData = new List<KeyValuePair<string, string>>
            {
                new("Email", invalidEmail),
                new("__RequestVerificationToken", antiForgeryToken!)
            };

            // Act
            var response = await client.PostAsync("/Account/Confirm/Resend", 
                new FormUrlEncodedContent(formData));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            // Verify validation error is shown
            var content = await response.Content.ReadAsStringAsync();
            var document = await _browsingContext.OpenAsync(req => req.Content(content));
            
            var validationError = document.QuerySelector(".field-validation-error, .text-danger");
            validationError.Should().NotBeNull($"Expected validation error for email: {invalidEmail}");
        }
    }

    [Fact]
    public async Task ResendConfirmation_WithoutAntiForgeryToken_ReturnsBadRequest()
    {
        // Arrange
        using var client = _fixture.CreateClient();

        // Prepare form data without anti-forgery token
        var formData = new List<KeyValuePair<string, string>>
        {
            new("Email", "test@example.com")
        };

        // Act
        var response = await client.PostAsync("/Account/Confirm/Resend", 
            new FormUrlEncodedContent(formData));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ResendConfirmation_WithInvalidAntiForgeryToken_ReturnsBadRequest()
    {
        // Arrange
        using var client = _fixture.CreateClient();

        // Prepare form data with invalid anti-forgery token
        var formData = new List<KeyValuePair<string, string>>
        {
            new("Email", "test@example.com"),
            new("__RequestVerificationToken", "invalid-token-123")
        };

        // Act
        var response = await client.PostAsync("/Account/Confirm/Resend", 
            new FormUrlEncodedContent(formData));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task StatusPage_WithParameters_DisplaysCorrectContent()
    {
        // Arrange
        using var client = _fixture.CreateClient();
        var query = new StringBuilder();
        query.Append("?title=").Append(Uri.EscapeDataString("Test Title"));
        query.Append("&message=").Append(Uri.EscapeDataString("Test Message"));
        query.Append("&type=").Append(Uri.EscapeDataString("warning"));
        query.Append("&details=").Append(Uri.EscapeDataString("Test Details"));
        query.Append("&redirectUrl=").Append(Uri.EscapeDataString("/test/url"));
        query.Append("&redirectText=").Append(Uri.EscapeDataString("Test Link"));

        // Act
        var response = await client.GetAsync($"/Account/Confirm/Status{query}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Test Title");
        content.Should().Contain("Test Message");
        content.Should().Contain("Test Details");
        content.Should().Contain("Test Link");
        
        // Verify warning styling is applied
        var document = await _browsingContext.OpenAsync(req => req.Content(content));
        var warningElement = document.QuerySelector(".alert-warning, .alert.alert-warning");
        warningElement.Should().NotBeNull();
    }

    [Fact]
    public async Task InvalidCodePage_DisplaysCorrectContent()
    {
        // Arrange
        using var client = _fixture.CreateClient();

        // Act
        var response = await client.GetAsync("/Account/Confirm/InvalidCode");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Invalid Confirmation Code");
        content.Should().Contain("confirmation code is invalid or has expired");
        content.Should().Contain("Request a new confirmation email");
        
        // Verify error styling
        var document = await _browsingContext.OpenAsync(req => req.Content(content));
        var errorElement = document.QuerySelector(".alert-danger, .alert.alert-danger");
        errorElement.Should().NotBeNull();
    }

    [Fact]
    public async Task AccountNotFoundPage_DisplaysCorrectContent()
    {
        // Arrange
        using var client = _fixture.CreateClient();

        // Act
        var response = await client.GetAsync("/Account/Confirm/AccountNotFound");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Account Not Found");
        content.Should().Contain("account associated with this confirmation link could not be found");
        content.Should().Contain("Request a new confirmation email");
        
        // Verify error styling
        var document = await _browsingContext.OpenAsync(req => req.Content(content));
        var errorElement = document.QuerySelector(".alert-danger, .alert.alert-danger");
        errorElement.Should().NotBeNull();
    }

    [Fact]
    public async Task SuccessPage_DisplaysCorrectContent()
    {
        // Arrange
        using var client = _fixture.CreateClient();

        // Act
        var response = await client.GetAsync("/Account/Confirm/Success");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("email address has been confirmed successfully");
        
        // Verify success styling
        var document = await _browsingContext.OpenAsync(req => req.Content(content));
        var successElement = document.QuerySelector(".alert-success, .alert.alert-success");
        successElement.Should().NotBeNull();
    }

    [Theory]
    [InlineData("/Account/Confirm/Email?code=<script>alert('xss')</script>")]
    [InlineData("/Account/Confirm/Status?title=<script>alert('xss')</script>")]
    [InlineData("/Account/Confirm/Status?message=<script>alert('xss')</script>")]
    [InlineData("/Account/Confirm/Success?message=<script>alert('xss')</script>")]
    public async Task XssAttacks_AreProperlyEscaped(string maliciousUrl)
    {
        // Arrange
        using var client = _fixture.CreateClient();

        // Act
        var response = await client.GetAsync(maliciousUrl);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
        
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotContain("<script>");
            content.Should().NotContain("alert('xss')");
        }
    }

    [Fact]
    public async Task SqlInjectionInCode_DoesNotCompromiseDatabase()
    {
        // Arrange
        using var client = _fixture.CreateClient();
        var maliciousCode = "'; DROP TABLE AspNetUsers; --";

        // Act
        var response = await client.GetAsync($"/Account/Confirm/Email?code={Uri.EscapeDataString(maliciousCode)}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.RequestMessage!.RequestUri!.LocalPath.Should().Be("/Account/Confirm/InvalidCode");
        
        // Verify database is still intact
        await WithScopeAsync(async services =>
        {
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var testUser = new ApplicationUser("sqltest@example.com");
            var createResult = await userManager.CreateAsync(testUser, "TestPassword123!");
            createResult.Succeeded.Should().BeTrue("Database should still be functional after SQL injection attempt");
        });
    }

    /// <summary>
    /// End-to-end test that simulates a complete user journey: registration, resend confirmation email, 
    /// and successful email confirmation using the received link.
    /// This test demonstrates the full workflow working together across multiple HTTP clients.
    /// </summary>
    [Fact]
    public async Task EndToEndConfirmationFlow_UserRegistersResetsAndConfirms_CompletesSuccessfully()
    {
        // Arrange
        const string testEmail = "endtoend@example.com";
        const string testPassword = "TestPassword123!";
        var emailSender = _fixture.Services.GetRequiredService<IEmailSender<ApplicationUser>>() as NullEmailSender<ApplicationUser>;
        
        // Track email events throughout the flow
        var emailEvents = new List<EmailSenderEventArgs>();
        emailSender!.EmailSent += (sender, args) => emailEvents.Add(args);

        // Step 1: Create an unconfirmed user account (simulating previous registration)
        var userId = await WithScopeAsync(async services =>
        {
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            
            var user = new ApplicationUser(testEmail);
            var createResult = await userManager.CreateAsync(user, testPassword);
            createResult.Succeeded.Should().BeTrue("User creation should succeed");
            
            // Verify user is unconfirmed
            user.EmailConfirmed.Should().BeFalse("User should start unconfirmed");
            
            return user.Id;
        });

        // Step 2: User goes to resend confirmation page and submits their email
        using var resendClient = _fixture.CreateClient();
        
        // Get the resend form page
        var resendGetResponse = await resendClient.GetAsync("/Account/Confirm/Resend");
        resendGetResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var resendGetContent = await resendGetResponse.Content.ReadAsStringAsync();
        var resendDocument = await _browsingContext.OpenAsync(req => req.Content(resendGetContent));
        
        // Extract anti-forgery token
        var antiForgeryToken = (resendDocument.QuerySelector("input[name='__RequestVerificationToken']") as IHtmlInputElement)?.Value;
        antiForgeryToken.Should().NotBeNullOrEmpty("Anti-forgery token should be present");

        // Submit the resend form
        var resendFormData = new List<KeyValuePair<string, string>>
        {
            new("Email", testEmail),
            new("__RequestVerificationToken", antiForgeryToken!)
        };

        var resendPostResponse = await resendClient.PostAsync("/Account/Confirm/Resend", 
            new FormUrlEncodedContent(resendFormData));
        
        resendPostResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify success message is displayed
        var resendPostContent = await resendPostResponse.Content.ReadAsStringAsync();
        resendPostContent.Should().Contain("confirmation email has been sent", 
            "Success message should be displayed after form submission");

        // Step 3: Verify email was sent and extract confirmation link
        emailEvents.Should().HaveCount(1, "Exactly one email should have been sent");
        var emailEvent = emailEvents.First();
        
        emailEvent.EventType.Should().Be(EmailSenderEventType.AccountConfirmationLink);
        emailEvent.Email.Should().Be(testEmail);
        emailEvent.LinkOrCode.Should().NotBeEmpty("Confirmation code should be provided");

        // Step 4: Simulate user clicking the email link in a different browser/client
        using var confirmClient = _fixture.CreateClient();
        
        // Use the confirmation link directly - it should work as-is
        var confirmResponse = await confirmClient.GetAsync(emailEvent.LinkOrCode);
        
        // Should redirect to success page
        confirmResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        confirmResponse.RequestMessage!.RequestUri!.LocalPath.Should().Be("/Account/Confirm/Success",
            "Should redirect to success page after successful confirmation");

        // Verify success page content
        var confirmContent = await confirmResponse.Content.ReadAsStringAsync();
        var confirmDocument = await _browsingContext.OpenAsync(req => req.Content(confirmContent));
        
        var successAlert = confirmDocument.QuerySelector(".alert-success, .alert.alert-success");
        successAlert.Should().NotBeNull("Success alert should be displayed");
        successAlert!.TextContent.Should().Contain("Thank you for confirming your email address", 
            "Success message should indicate successful confirmation");

        // Verify login and continue buttons are present
        var loginButton = confirmDocument.QuerySelector("a[href*='/Account/Login']");
        loginButton.Should().NotBeNull("Login button should be present");
        loginButton!.TextContent.Should().Contain("Sign In", "Login button should have appropriate text");

        var continueButton = confirmDocument.QuerySelector("a[href='/']");
        continueButton.Should().NotBeNull("Continue to application button should be present");

        // Step 5: Verify user account is now confirmed in the database
        await WithScopeAsync(async services =>
        {
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            
            var confirmedUser = await userManager.FindByIdAsync(userId.ToString());
            confirmedUser.Should().NotBeNull("User should still exist in database");
            confirmedUser!.EmailConfirmed.Should().BeTrue("User should now be confirmed");
            confirmedUser.Email.Should().Be(testEmail, "Email should remain unchanged");
        });

        // Step 6: Verify attempting to use the same confirmation link again shows info message
        var secondConfirmResponse = await confirmClient.GetAsync(emailEvent.LinkOrCode);
        secondConfirmResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        secondConfirmResponse.RequestMessage!.RequestUri!.LocalPath.Should().Be("/Account/Confirm/Success");
        
        // Parse query string to verify it's an info message about already confirmed
        var secondConfirmQuery = System.Web.HttpUtility.ParseQueryString(secondConfirmResponse.RequestMessage.RequestUri.Query);
        secondConfirmQuery["type"].Should().Be("info", "Second confirmation attempt should show info message");
        secondConfirmQuery["message"].Should().Contain("already been confirmed", 
            "Message should indicate email is already confirmed");

        // Step 7: Verify resending email for already confirmed user shows appropriate message
        var secondResendResponse = await resendClient.PostAsync("/Account/Confirm/Resend", 
            new FormUrlEncodedContent(resendFormData));
        
        secondResendResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var secondResendContent = await secondResendResponse.Content.ReadAsStringAsync();
        secondResendContent.Should().Contain("already been confirmed", 
            "Resend attempt for confirmed user should show appropriate message");

        // Verify no additional email was sent
        emailEvents.Should().HaveCount(1, "No additional email should be sent for already confirmed user");
    }
}
