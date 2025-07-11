using System.ComponentModel.DataAnnotations;
using Propulse.Core.DataAnnotations;

namespace Propulse.Web.Areas.Account.InputModels;

/// <summary>
/// Input model for resending account confirmation email.
/// Contains only the data that should be accepted from POST requests.
/// </summary>
public class ResendConfirmationInputModel
{
    /// <summary>
    /// Gets or sets the email address to send the confirmation to.
    /// </summary>
    [Required(ErrorMessage = "Email address is required.")]
    [PropulseEmailAddress(ErrorMessage = "Please enter a valid email address.")]
    [Display(Name = "Email Address")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="ResendConfirmationInputModel"/> class.
    /// </summary>
    public ResendConfirmationInputModel()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ResendConfirmationInputModel"/> class
    /// by copying values from another instance.
    /// </summary>
    /// <param name="other">The instance to copy from.</param>
    public ResendConfirmationInputModel(ResendConfirmationInputModel other)
    {
        ArgumentNullException.ThrowIfNull(other);
        Email = other.Email;
    }
}
