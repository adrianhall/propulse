namespace Propulse.Infrastructure.Services;

/// <summary>
/// This replaces the older ASP.NET Core Identity <c>IEmailSender</c> typed
/// interface to provide a simpler interface for sending transactional emails.
/// </summary>
/// <typeparam name="TUser">The type of the user record.</typeparam>
public interface IPropulseEmailSender<TUser> where TUser : class
{
    /// <summary>
    /// Sends an account confirmation link to the user.
    /// </summary>
    /// <param name="email">The email address of the user.</param>
    /// <param name="link">The link to be sent.</param>
    /// <returns>A task that resolves when the email is sent.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the link cannot be sent.</exception>
    Task SendAccountConfirmationLinkAsync(string email, string link);

    /// <summary>
    /// Sends a password reset link to the user.
    /// </summary>
    /// <param name="email">The email address of the user.</param>
    /// <param name="link">The link to be sent.</param>
    /// <returns>A task that resolves when the email is sent.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the link cannot be sent.</exception>
    Task SendPasswordResetLinkAsync(string email, string link);
}
