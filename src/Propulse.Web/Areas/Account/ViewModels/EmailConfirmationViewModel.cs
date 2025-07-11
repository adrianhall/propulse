using Propulse.Web.Areas.Account.InputModels;

namespace Propulse.Web.Areas.Account.ViewModels;

/// <summary>
/// View model for email confirmation page.
/// Contains display data for the confirmation result.
/// </summary>
public class EmailConfirmationViewModel : EmailConfirmationInputModel
{
    /// <summary>
    /// Gets or sets a value indicating whether the confirmation was successful.
    /// </summary>
    public bool IsConfirmed { get; set; }

    /// <summary>
    /// Gets or sets the status message to display to the user.
    /// </summary>
    public string? StatusMessage { get; set; }

    /// <summary>
    /// Gets or sets the type of status message (success, error, info).
    /// </summary>
    public string StatusType { get; set; } = "info";

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailConfirmationViewModel"/> class.
    /// </summary>
    public EmailConfirmationViewModel()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailConfirmationViewModel"/> class
    /// from an input model.
    /// </summary>
    /// <param name="inputModel">The input model to copy from.</param>
    public EmailConfirmationViewModel(EmailConfirmationInputModel inputModel) : base(inputModel)
    {
    }
}
