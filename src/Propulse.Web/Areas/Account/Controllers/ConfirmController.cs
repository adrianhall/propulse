using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Propulse.Web.Areas.Account.InputModels;
using Propulse.Web.Areas.Account.ViewModels;
using Propulse.Web.Entities;
using Propulse.Web.Services;

namespace Propulse.Web.Areas.Account.Controllers;

/// <summary>
/// Controller responsible for handling account confirmation operations.
/// Provides functionality for email confirmation and resending confirmation links.
/// </summary>
[Area("Account")]
[Route("Account/[controller]")]
public class ConfirmController(
    UserManager<ApplicationUser> userManager,
    IResponseLinkService linkService,
    IEmailSender<ApplicationUser> emailSender,
    ILogger<ConfirmController> logger) : Controller
{
    /// <summary>
    /// Handles email confirmation using the provided confirmation code.
    /// Redirects to appropriate status pages to prevent duplicate submissions.
    /// </summary>
    /// <param name="code">The confirmation code from the email link.</param>
    /// <returns>Redirect to success or status page based on confirmation result.</returns>
    [HttpGet("Email")]
    public async Task<IActionResult> Email([FromQuery] string? code)
    {
        if (string.IsNullOrEmpty(code))
        {
            logger.LogWarning("Email confirmation attempted with empty or null code");
            return RedirectToAction(nameof(Status), new { 
                title = "Invalid Confirmation Link",
                message = "Invalid confirmation link. Please check your email for the correct link.",
                type = "error",
                details = "The confirmation link appears to be malformed or incomplete.",
                redirectUrl = Url.Action(nameof(Resend), "Confirm", new { area = "Account" }),
                redirectText = "Request a new confirmation email"
            });
        }

        try
        {
            // Decode the response code to get user ID and token
            var (userId, token) = linkService.DecodeResponseCode(code);
            
            // Find the user
            var user = await userManager.FindByIdAsync(userId.ToString());
            if (user is null)
            {
                logger.LogWarning("Email confirmation attempted for non-existent user ID: {UserId}", userId);
                return RedirectToAction(nameof(AccountNotFound));
            }

            // Check if already confirmed - treat as success since confirmation is not required
            if (user.EmailConfirmed)
            {
                logger.LogInformation("Email confirmation attempted for already confirmed user: {UserId}", userId);
                return RedirectToAction(nameof(Success), new {
                    message = "Your email address has already been confirmed.",
                    type = "info"
                });
            }

            // Confirm the email
            var result = await userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                logger.LogInformation("Email confirmed successfully for user: {UserId}", userId);
                return RedirectToAction(nameof(Success), new {
                    message = "Thank you for confirming your email address. Your account is now active.",
                    type = "success"
                });
            }
            else
            {
                logger.LogWarning("Email confirmation failed for user: {UserId}. Errors: {Errors}", 
                    userId, string.Join(", ", result.Errors.Select(e => e.Description)));
                return RedirectToAction(nameof(Status), new {
                    title = "Email Confirmation Failed",
                    message = "There was an error confirming your email address. The link may have expired.",
                    type = "error",
                    details = "Confirmation links expire after a certain period for security reasons. Please request a new confirmation email.",
                    redirectUrl = Url.Action(nameof(Resend), "Confirm", new { area = "Account" }),
                    redirectText = "Request a new confirmation email"
                });
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during email confirmation with code: {Code}", code);
            return RedirectToAction(nameof(InvalidCode));
        }
    }

    /// <summary>
    /// Displays the resend confirmation form.
    /// </summary>
    /// <returns>The resend confirmation view.</returns>
    [HttpGet("Resend")]
    public IActionResult Resend()
    {
        return View(new ResendConfirmationViewModel());
    }

    /// <summary>
    /// Processes the resend confirmation request.
    /// </summary>
    /// <param name="inputModel">The resend confirmation input model containing the email address.</param>
    /// <returns>The resend confirmation view with status information.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the inputModel is null.</exception>
    [HttpPost("Resend")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Resend([FromForm] ResendConfirmationInputModel inputModel)
    {
        ArgumentNullException.ThrowIfNull(inputModel);

        var viewModel = new ResendConfirmationViewModel(inputModel);

        if (!ModelState.IsValid)
        {
            // Return the view model with validation errors
            return View(viewModel);
        }

        try
        {
            var user = await userManager.FindByEmailAsync(inputModel.Email);
            if (user is null)
            {
                logger.LogWarning("Resend confirmation requested for non-existent email: {Email}", inputModel.Email);
                // Don't reveal that the user doesn't exist for security reasons
                viewModel.StatusMessage = "If an account with that email address exists, a confirmation email has been sent.";
                viewModel.StatusType = "info";
                return View(viewModel);
            }

            if (user.EmailConfirmed)
            {
                logger.LogInformation("Resend confirmation requested for already confirmed user: {UserId}", user.Id);
                viewModel.StatusMessage = "This email address has already been confirmed.";
                viewModel.StatusType = "info";
                return View(viewModel);
            }

            // Generate new confirmation token
            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
            var code = linkService.EncodeResponseCode(user.Id, token);
            var confirmationLink = linkService.CreateResponseLink("Account", "Confirm", "Email", code);

            // Send confirmation email
            await emailSender.SendConfirmationLinkAsync(user, inputModel.Email, confirmationLink);
            
            logger.LogInformation("Confirmation email sent successfully to: {Email}", inputModel.Email);

            viewModel.StatusMessage = "A confirmation email has been sent to your email address. Please check your inbox and click the link to confirm your account.";
            viewModel.StatusType = "success";
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error generating or sending confirmation email for: {Email}", inputModel.Email);
            viewModel.StatusMessage = "An error occurred while sending the confirmation email. Please try again later.";
            viewModel.StatusType = "error";
            
            // For email sending failures, we could add more specific handling here
            // The view will automatically show a "try again" button since it's the same form
        }

        return View(viewModel);
    }

    /// <summary>
    /// Displays a success page for email confirmation.
    /// </summary>
    /// <param name="message">The success message to display.</param>
    /// <param name="type">The type of message (success, info).</param>
    /// <returns>The success view.</returns>
    [HttpGet("Success")]
    public IActionResult Success([FromQuery] string? message = null, [FromQuery] string type = "success")
    {
        var model = new EmailConfirmationViewModel
        {
            StatusMessage = message ?? "Your email address has been confirmed successfully.",
            StatusType = type
        };

        return View("Email", model);
    }

    /// <summary>
    /// Displays a status page with customizable content.
    /// </summary>
    /// <param name="title">The title for the status page.</param>
    /// <param name="message">The main message to display.</param>
    /// <param name="type">The type of status (error, warning, info).</param>
    /// <param name="details">Additional details about the status.</param>
    /// <param name="redirectUrl">URL for an action button.</param>
    /// <param name="redirectText">Text for the action button.</param>
    /// <returns>The status view.</returns>
    [HttpGet("Status")]
    public IActionResult Status(
        [FromQuery] string? title = null,
        [FromQuery] string? message = null,
        [FromQuery] string type = "info",
        [FromQuery] string? details = null,
        [FromQuery] string? redirectUrl = null,
        [FromQuery] string? redirectText = null)
    {
        var model = new StatusViewModel
        {
            Title = title ?? "Status",
            Message = message ?? "An operation has completed.",
            StatusType = type,
            Details = details,
            RedirectUrl = redirectUrl,
            RedirectText = redirectText
        };

        return View(model);
    }

    /// <summary>
    /// Displays a status message indicating that the account was not found.
    /// </summary>
    /// <returns>The account not found view.</returns>
    [HttpGet("AccountNotFound")]
    public IActionResult AccountNotFound()
    {
        var model = new StatusViewModel
        {
            Title = "Account Not Found",
            Message = "The account associated with this confirmation link could not be found.",
            StatusType = "error",
            Details = "This may happen if the account has been deleted or if you're using an old confirmation link.",
            RedirectUrl = Url.Action(nameof(Resend), "Confirm", new { area = "Account" }),
            RedirectText = "Request a new confirmation email"
        };

        return View("Status", model);
    }

    /// <summary>
    /// Displays a status message indicating that the confirmation code is invalid.
    /// </summary>
    /// <returns>The invalid code view.</returns>
    [HttpGet("InvalidCode")]
    public IActionResult InvalidCode()
    {
        var model = new StatusViewModel
        {
            Title = "Invalid Confirmation Code",
            Message = "The confirmation code is invalid or has expired.",
            StatusType = "error",
            Details = "Confirmation links expire after a certain period for security reasons. Please request a new confirmation email.",
            RedirectUrl = Url.Action(nameof(Resend), "Confirm", new { area = "Account" }),
            RedirectText = "Request a new confirmation email"
        };

        return View("Status", model);
    }
}
