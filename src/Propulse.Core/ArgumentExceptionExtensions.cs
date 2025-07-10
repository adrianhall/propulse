using Propulse.Core.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace System;

/// <summary>
/// Provides extension methods for <see cref="ArgumentException"/> to perform common validation scenarios
/// specific to the Propulse application domain.
/// </summary>
public static class ArgumentExceptionExtensions
{
    extension(ArgumentException)
    {
        /// <summary>
        /// Throws an <see cref="ArgumentException"/> if the specified role name argument is not valid
        /// according to Propulse role naming conventions.
        /// </summary>
        /// <param name="argument">The role name string to validate.</param>
        /// <param name="paramName">
        /// The name of the parameter with which <paramref name="argument"/> corresponds.
        /// If you omit this argument, the name of <paramref name="argument"/> is used.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="argument"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="argument"/> is empty, contains only whitespace characters,
        /// or does not conform to Propulse role naming conventions.
        /// </exception>
        /// <remarks>
        /// <para>
        /// This method validates that the role name follows the Propulse naming conventions:
        /// </para>
        /// <list type="bullet">
        /// <item><description>Must start with an uppercase letter (A-Z)</description></item>
        /// <item><description>Must be between 3 and 64 characters long</description></item>
        /// <item><description>Can only contain letters (A-Z, a-z)</description></item>
        /// </list>
        /// <para>
        /// The validation is performed using <see cref="PropulseRoleNameAttribute"/>.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// // Valid usage - no exception thrown
        /// string roleName = "Administrator";
        /// ArgumentException.ThrowIfNotValidRoleName(roleName);
        /// 
        /// // Invalid usage - throws ArgumentException
        /// string invalidRole = "admin"; // starts with lowercase
        /// ArgumentException.ThrowIfNotValidRoleName(invalidRole); // throws
        /// </code>
        /// </example>
        public static void ThrowIfNotValidRoleName([NotNull] string? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(argument, paramName);

            var validator = new PropulseRoleNameAttribute();
            if (!validator.IsValid(argument))
            {
                throw new ArgumentException($"Invalid role name: '{argument}'. Role names must start with an uppercase letter and be between 3 and 64 characters long, containing only letters.", paramName);
            }
        }

        /// <summary>
        /// Throws an <see cref="ArgumentException"/> if the specified user name argument is not a valid Propulse user name (email address).
        /// </summary>
        /// <param name="argument">The user name string to validate.</param>
        /// <param name="paramName">
        /// The name of the parameter with which <paramref name="argument"/> corresponds.
        /// If you omit this argument, the name of <paramref name="argument"/> is used.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="argument"/> is <see langword="null"/> or whitespace.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="argument"/> does not conform to Propulse user name (email address) conventions.
        /// </exception>
        /// <remarks>
        /// <para>
        /// This method validates that the user name is a valid email address according to Propulse rules:
        /// </para>
        /// <list type="bullet">
        /// <item><description>The username part can only contain letters, numbers, dashes, dots, and underscores, and cannot have consecutive dots.</description></item>
        /// <item><description>The domain part must be a valid domain name with 2 to 6 segments, supporting punycode (IDN).</description></item>
        /// </list>
        /// <para>
        /// The validation is performed using <see cref="PropulseEmailAddressAttribute"/>.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// // Valid usage - no exception thrown
        /// string userName = "john.doe@example.com";
        /// ArgumentException.ThrowIfNotValidUserName(userName);
        /// 
        /// // Invalid usage - throws ArgumentException
        /// string invalidUser = "john..doe@example.com"; // consecutive dots
        /// ArgumentException.ThrowIfNotValidUserName(invalidUser); // throws
        /// </code>
        /// </example>
        public static void ThrowIfNotValidUserName([NotNull] string? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(argument, paramName);

            var validator = new PropulseEmailAddressAttribute();
            if (!validator.IsValid(argument))
            {
                throw new ArgumentException($"Invalid user name: '{argument}'. User names must be valid email addresses with strict format requirements.", paramName);
            }
        }
    }

}