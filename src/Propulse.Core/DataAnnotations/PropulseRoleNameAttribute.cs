using System.ComponentModel.DataAnnotations;

namespace Propulse.Core.DataAnnotations;

/// <summary>
/// A data annotation attribute for validating role names in the Propulse application.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class PropulseRoleNameAttribute() : RegularExpressionAttribute(RoleNamePattern)
{
    /// <summary>
    /// This attribute validates that a role name follows the Propulse naming conventions:
    /// - Must start with an uppercase letter
    /// - Must be between 3 and 64 characters long
    /// - Can only contain letters (A-Z, a-z)
    /// </summary>
    private const string RoleNamePattern = @"^[A-Z][A-Za-z]{2,63}$";
}