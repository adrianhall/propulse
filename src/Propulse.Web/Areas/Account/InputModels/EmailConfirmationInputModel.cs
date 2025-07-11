using System.ComponentModel.DataAnnotations;

namespace Propulse.Web.Areas.Account.InputModels;

/// <summary>
/// Input model for email confirmation.
/// Contains only the data that should be accepted from requests.
/// </summary>
public class EmailConfirmationInputModel
{
    /// <summary>
    /// Gets or sets the confirmation code.
    /// </summary>
    [Required(ErrorMessage = "Confirmation code is required.")]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailConfirmationInputModel"/> class.
    /// </summary>
    public EmailConfirmationInputModel()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailConfirmationInputModel"/> class
    /// by copying values from another instance.
    /// </summary>
    /// <param name="other">The instance to copy from.</param>
    public EmailConfirmationInputModel(EmailConfirmationInputModel other)
    {
        ArgumentNullException.ThrowIfNull(other);
        Code = other.Code;
    }
}
