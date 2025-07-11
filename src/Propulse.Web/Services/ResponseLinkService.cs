using Microsoft.AspNetCore.WebUtilities;
using System.Text;

namespace Propulse.Web.Services;

public class ResponseLinkService(IHttpContextAccessor accessor, LinkGenerator generator, ILogger<ResponseLinkService> logger) : IResponseLinkService
{
    internal HttpContext Context
    {
        get => accessor.HttpContext ?? throw new InvalidOperationException("HttpContext is not available.");
    }

    #region IResponseLinkService

    /// <summary>
    /// Creates a response link based on the provided information.
    /// </summary>
    /// <param name="area">The area (may be an empty string to indicate no area)</param>
    /// <param name="controller">The controller containing the action.</param>
    /// <param name="action">The action to execute when the link is clicked.</param>
    /// <param name="code">The code to send along with the link.</param>
    /// <returns>The link</returns>
    /// <exception cref="InvalidOperationException">Thrown if the link cannot be created.</exception> 
    public string CreateResponseLink(string area, string controller, string action, string code)
    {
        logger.LogDebug("CreateResponseLink({area}, {controller}, {action}, {code})", area, controller, action, code);
        var link = generator.GetUriByAction(Context, action, controller, new { area, code });
        if (string.IsNullOrEmpty(link))
        {
            throw new InvalidOperationException("Requested link could not be generated");
        }
        return link;
    }

    /// <summary>
    /// Decodes a response code into its original parts.
    /// </summary>
    /// <param name="code">The response code the user was sent.</param>
    /// <returns>A tuple with the user ID and original token.</returns>
    public (Guid, string) DecodeResponseCode(string code)
    {
        ArgumentException.ThrowIfNullOrEmpty(code);
        logger.LogDebug("DecodeResponseCode({code})", code);

        byte[] decodedBytes = WebEncoders.Base64UrlDecode(code);
        if (decodedBytes.Length < 17)
        {
            logger.LogDebug("DecodeResponseCode({code}): Response code is too short", code);
            throw new FormatException("The response code is not in a correct format.");
        }

        Guid id = new(decodedBytes[..16]);
        string token = Encoding.UTF8.GetString(decodedBytes[16..]);
        return (id, token);
    }

    /// <summary>
    /// Creates a response code based on the user ID and the provided token string.
    /// </summary>
    /// <param name="id">The user ID that the response code is for.</param>
    /// <param name="token">The token that is used to confirm the operation.</param>
    /// <returns>The response code to send to the user.</returns>
    public string EncodeResponseCode(Guid id, string token)
    {
        ArgumentException.ThrowIfNullOrEmpty(token);
        logger.LogDebug("EncodeResponseCode({id}, {token})", id, token);

        byte[] idBytes = id.ToByteArray();
        byte[] tokenBytes = Encoding.UTF8.GetBytes(token);
        byte[] responseCodeBytes = [..idBytes.Concat(tokenBytes)];
        string responseCode = WebEncoders.Base64UrlEncode(responseCodeBytes);

        return responseCode;
    }

    #endregion
}