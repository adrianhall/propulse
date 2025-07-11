using Propulse.Web.Areas.Account.InputModels;

namespace Propulse.Web.Areas.Account.ViewModels;

/// <summary>
/// View model for resending account confirmation email.
/// Contains display data and status information.
/// </summary>
public class ResendConfirmationViewModel : ResendConfirmationInputModel
{
    /// <summary>
    /// Gets or sets the status message to display to the user.
    /// </summary>
    public string? StatusMessage { get; set; }

    /// <summary>
    /// Gets or sets the type of status message (success, error, info).
    /// </summary>
    public string StatusType { get; set; } = "info";

    /// <summary>
    /// Initializes a new instance of the <see cref="ResendConfirmationViewModel"/> class.
    /// </summary>
    public ResendConfirmationViewModel()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ResendConfirmationViewModel"/> class
    /// from an input model.
    /// </summary>
    /// <param name="inputModel">The input model to copy from.</param>
    public ResendConfirmationViewModel(ResendConfirmationInputModel inputModel) : base(inputModel)
    {
    }
}
