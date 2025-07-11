namespace Propulse.Web.Services;

/// <summary>
/// A testable mechanism for generating links to be sent via email with codes for operation confirmation.
/// </summary>
public interface IResponseLinkService
{
    /// <summary>
    /// Creates a response link based on the provided information.
    /// </summary>
    /// <param name="area">The area (may be an empty string to indicate no area)</param>
    /// <param name="controller">The controller containing the action.</param>
    /// <param name="action">The action to execute when the link is clicked.</param>
    /// <param name="code">The code to send along with the link.</param>
    /// <returns>The link</returns>
    /// <exception cref="InvalidOperationException">Thrown if the link cannot be created.</exception> 
    string CreateResponseLink(string area, string controller, string action, string code);

    /// <summary>
    /// Decodes a response code into its original parts.
    /// </summary>
    /// <param name="code">The response code the user was sent.</param>
    /// <returns>A tuple with the user ID and original token.</returns>
    (Guid, string) DecodeResponseCode(string code);

    /// <summary>
    /// Creates a response code based on the user ID and the provided token string.
    /// </summary>
    /// <param name="id">The user ID that the response code is for.</param>
    /// <param name="token">The token that is used to confirm the operation.</param>
    /// <returns>The response code to send to the user.</returns>
    string EncodeResponseCode(Guid id, string token);


}