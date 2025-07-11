using Microsoft.AspNetCore.Identity;
using Propulse.Web.Events;

namespace Propulse.Web.Services;

/// <summary>
/// A no-op implementation of <see cref="IEmailSender{TUser}"/> for scenarios where email sending is disabled or not required.
/// Logs email events and raises <see cref="EmailSent"/> without sending actual emails.
/// </summary>
/// <typeparam name="TUser">The user type for which emails are sent.</typeparam>
/// <remarks>
/// Useful for development, testing, or environments where email delivery is not configured.
/// </remarks>
/// <example>
/// <code>
/// var sender = new NullEmailSender<ApplicationUser>(logger);
/// await sender.SendConfirmationLinkAsync(user, email, link);
/// </code>
/// </example>
public class NullEmailSender<TUser>(ILogger<NullEmailSender<TUser>> logger) : IEmailSender<TUser> where TUser : class
{
    /// <summary>
    /// Occurs when an email event is triggered (confirmation, password reset, etc.).
    /// </summary>
    public event EventHandler<EmailSenderEventArgs>? EmailSent;

    /// <summary>
    /// Sends an account confirmation link email (logs only, does not send).
    /// </summary>
    /// <param name="user">The user to whom the email would be sent.</param>
    /// <param name="email">The recipient's email address.</param>
    /// <param name="confirmationLink">The confirmation link to include in the email.</param>
    /// <returns>A completed <see cref="Task"/>.</returns>
    /// <remarks>
    /// This method logs the event and raises <see cref="EmailSent"/>.
    /// </remarks>
    public Task SendConfirmationLinkAsync(TUser user, string email, string confirmationLink)
    {
        logger.LogInformation("Account Confirmation Link Sent to {email}: {confirmationLink}", email, confirmationLink);
        EmailSent?.Invoke(this, new EmailSenderEventArgs(EmailSenderEventType.AccountConfirmationLink, email, confirmationLink));
        return Task.CompletedTask;
    }

    /// <summary>
    /// Sends a password reset code email (logs only, does not send).
    /// </summary>
    /// <param name="user">The user to whom the email would be sent.</param>
    /// <param name="email">The recipient's email address.</param>
    /// <param name="resetCode">The password reset code to include in the email.</param>
    /// <returns>A completed <see cref="Task"/>.</returns>
    /// <remarks>
    /// This method logs the event and raises <see cref="EmailSent"/>.
    /// </remarks>
    public Task SendPasswordResetCodeAsync(TUser user, string email, string resetCode)
    {
        logger.LogInformation("Password Reset Code sent to {email}: {resetCode}", email, resetCode);
        EmailSent?.Invoke(this, new EmailSenderEventArgs(EmailSenderEventType.PasswordResetCode, email, resetCode));
        return Task.CompletedTask;
    }

    /// <summary>
    /// Sends a password reset link email (logs only, does not send).
    /// </summary>
    /// <param name="user">The user to whom the email would be sent.</param>
    /// <param name="email">The recipient's email address.</param>
    /// <param name="resetLink">The password reset link to include in the email.</param>
    /// <returns>A completed <see cref="Task"/>.</returns>
    /// <remarks>
    /// This method logs the event and raises <see cref="EmailSent"/>.
    /// </remarks>
    public Task SendPasswordResetLinkAsync(TUser user, string email, string resetLink)
    {
        logger.LogInformation("Password Reset Link sent to {email}: {resetLink}", email, resetLink);
        EmailSent?.Invoke(this, new EmailSenderEventArgs(EmailSenderEventType.PasswordResetLink, email, resetLink));
        return Task.CompletedTask;
    }
}