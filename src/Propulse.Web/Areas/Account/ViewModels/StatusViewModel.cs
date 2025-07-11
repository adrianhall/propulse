namespace Propulse.Web.Areas.Account.ViewModels;

/// <summary>
/// View model for displaying status messages to users.
/// </summary>
public class StatusViewModel
{
    /// <summary>
    /// Gets or sets the title of the status page.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the main message to display to the user.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the type of status message (success, error, info, warning).
    /// </summary>
    public string StatusType { get; set; } = "info";

    /// <summary>
    /// Gets or sets additional details or instructions for the user.
    /// </summary>
    public string? Details { get; set; }

    /// <summary>
    /// Gets or sets the URL to redirect the user to (optional).
    /// </summary>
    public string? RedirectUrl { get; set; }

    /// <summary>
    /// Gets or sets the text for the redirect link (optional).
    /// </summary>
    public string? RedirectText { get; set; }
}
