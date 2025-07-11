using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Propulse.Core.DataAnnotations;

/// <summary>
/// Validates that an email address is well-formed according to specific Propulse rules.
/// </summary>
/// <remarks>
/// This validation is more comprehensive than the standard <see cref="EmailAddressAttribute"/>.
/// It splits the email into a username and a domain part.
/// The username can only contain alphanumeric characters, dashes, dots, and underscores, and cannot have consecutive dots.
/// The domain must be a valid domain name with 2 to 6 segments, allowing for punycode.
/// </remarks>
public class PropulseEmailAddressAttribute : ValidationAttribute
{
    private static readonly Regex UsernameRegex = new("^[a-zA-Z0-9_.-]+$", RegexOptions.Compiled);

    /// <inheritdoc />
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null)
        {
            return ValidationResult.Success;
        }

        if (value is not string email || string.IsNullOrWhiteSpace(email))
        {
            return new ValidationResult(ErrorMessage ?? "Email address must be a non-empty string.");
        }

        var parts = email.Split('@');
        if (parts.Length != 2)
        {
            return new ValidationResult(ErrorMessage ?? "Email address must contain exactly one '@' symbol.");
        }

        var username = parts[0];
        var domain = parts[1];

        if (!IsValidUsername(username))
        {
            return new ValidationResult(ErrorMessage ?? "Invalid email username part. It can only contain letters, numbers, dots, dashes, and underscores, and cannot have consecutive dots.");
        }

        if (!IsValidDomain(domain))
        {
            return new ValidationResult(ErrorMessage ?? "Invalid email domain part. It must be a valid domain name with 2 to 6 segments.");
        }

        return ValidationResult.Success;
    }

    /// <summary>
    /// Validates the username part of an email address.
    /// </summary>
    /// <param name="username">The username to validate.</param>
    /// <returns><c>true</c> if the username is valid; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// A valid username can only contain alphanumeric characters, dashes, dots, and underscores.
    /// It cannot contain consecutive dots.
    /// </remarks>
    internal static bool IsValidUsername(string username)
    {
        if (string.IsNullOrEmpty(username))
        {
            return false;
        }

        if (username.Contains(".."))
        {
            return false;
        }

        return UsernameRegex.IsMatch(username);
    }

    /// <summary>
    /// Validates the domain part of an email address.
    /// </summary>
    /// <param name="domain">The domain to validate.</param>
    /// <returns><c>true</c> if the domain is valid; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// A valid domain must have between 2 and 6 segments separated by dots.
    /// Each segment must not be empty. It supports internationalized domain names (punycode).
    /// <para>
    /// This method uses <see cref="IdnMapping"/> to convert the domain to its ASCII-compatible encoding (Punycode).
    /// <see cref="IdnMapping"/> enables support for Internationalized Domain Names (IDN), allowing Unicode 
    /// characters in domain names. If <see cref="IdnMapping.GetAscii(string)"/> throws an <see cref="ArgumentException"/>, 
    /// the domain is not valid per the IDNA standard.
    /// </para>
    /// </remarks>
    internal static bool IsValidDomain(string domain)
    {
        if (string.IsNullOrEmpty(domain))
        {
            return false;
        }

        var segments = domain.Split('.');
        if (segments.Length is < 2 or > 6)
        {
            return false;
        }

        if (segments.Any(string.IsNullOrEmpty))
        {
            return false;
        }

        try
        {
            var idn = new IdnMapping();
            _ = idn.GetAscii(domain);
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }
}