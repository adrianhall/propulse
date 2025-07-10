using Microsoft.AspNetCore.Identity;

namespace Propulse.Persistence.Entities;

/// <summary>
/// Represents an application role in the Propulse security system.
/// Extends the standard ASP.NET Core Identity role to use UUID version 7 primary keys
/// for better performance, sorting, and distributed system compatibility.
/// </summary>
/// <remarks>
/// This class provides role-based authorization capabilities within the Propulse application.
/// Key features include:
/// <list type="bullet">
/// <item>UUID version 7 primary keys for optimal database performance and natural sorting</item>
/// <item>Automatic role name normalization for case-insensitive lookups</item>
/// <item>Full compatibility with ASP.NET Core Identity role management</item>
/// <item>Support for role-based claims and permissions</item>
/// </list>
/// 
/// UUID version 7 provides several advantages over traditional GUIDs:
/// <list type="bullet">
/// <item>Time-ordered for better database index performance</item>
/// <item>Lexicographically sortable by creation time</item>
/// <item>Reduced index fragmentation in PostgreSQL</item>
/// <item>Better clustering and query performance</item>
/// </list>
/// 
/// The role name normalization ensures consistent lookups regardless of case,
/// supporting the case-insensitive CITEXT columns in the PostgreSQL schema.
/// </remarks>
/// <example>
/// Creating and using application roles:
/// <code>
/// // Create a new role with automatic UUID generation
/// var adminRole = new ApplicationRole("Administrator");
/// 
/// // Create a role using the default constructor
/// var userRole = new ApplicationRole();
/// userRole.Name = "User";
/// userRole.NormalizedName = "USER";
/// 
/// // Add role to the context
/// await roleManager.CreateAsync(adminRole);
/// 
/// // Check if user has role
/// bool isAdmin = await userManager.IsInRoleAsync(user, "Administrator");
/// </code>
/// </example>
/// <seealso cref="ApplicationUser"/>
/// <seealso cref="Microsoft.AspNetCore.Identity.IdentityRole{TKey}"/>
public class ApplicationRole : IdentityRole<Guid>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ApplicationRole"/> class with an automatically generated UUID version 7 primary key.
    /// </summary>
    /// <remarks>
    /// This constructor creates a new role with a time-ordered UUID version 7 identifier.
    /// The UUID version 7 format provides better database performance characteristics
    /// compared to standard random GUIDs by maintaining temporal ordering.
    /// 
    /// After instantiation, you should set the <see cref="IdentityRole{TKey}.Name"/> and
    /// <see cref="IdentityRole{TKey}.NormalizedName"/> properties as needed, or use the
    /// parameterized constructor for convenience.
    /// </remarks>
    /// <example>
    /// <code>
    /// var role = new ApplicationRole();
    /// role.Name = "Manager";
    /// role.NormalizedName = "MANAGER";
    /// </code>
    /// </example>
    public ApplicationRole() : base()
    {
        Id = Guid.CreateVersion7();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApplicationRole"/> class with the specified role name.
    /// Automatically generates a UUID version 7 primary key and normalizes the role name for consistent lookups.
    /// </summary>
    /// <param name="roleName">The name of the role. This value will be used for both the display name and normalized name (converted to uppercase).</param>
    /// <remarks>
    /// This constructor provides a convenient way to create a fully configured role in a single step.
    /// The role name normalization uses <see cref="string.ToUpperInvariant()"/> to ensure consistent
    /// case-insensitive lookups that align with the PostgreSQL CITEXT column configuration.
    /// 
    /// The normalized name is used internally by ASP.NET Core Identity for efficient role lookups
    /// and comparisons, while the original name is preserved for display purposes.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Create an administrator role
    /// var adminRole = new ApplicationRole("Administrator");
    /// // adminRole.Name = "Administrator"
    /// // adminRole.NormalizedName = "ADMINISTRATOR"
    /// // adminRole.Id = [auto-generated UUID v7]
    /// 
    /// // Create a user role
    /// var userRole = new ApplicationRole("User");
    /// await roleManager.CreateAsync(userRole);
    /// </code>
    /// </example>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="roleName"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="roleName"/> is empty or contains only whitespace.</exception>
    public ApplicationRole(string roleName) : this()
    {
        ArgumentException.ThrowIfNotValidRoleName(roleName);

        Name = roleName;
        NormalizedName = roleName.ToUpperInvariant();
        ConcurrencyStamp = Guid.NewGuid().ToString();
    }
}