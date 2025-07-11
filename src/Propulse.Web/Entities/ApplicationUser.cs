using Microsoft.AspNetCore.Identity;

namespace Propulse.Web.Entities;

/// <summary>
/// Represents an application user in the Propulse authentication and authorization system.
/// Extends the standard ASP.NET Core Identity user to use UUID version 7 primary keys
/// for enhanced performance, natural sorting, and distributed system compatibility.
/// </summary>
/// <remarks>
/// This class provides comprehensive user account management capabilities within the Propulse application.
/// Key features include:
/// <list type="bullet">
/// <item>UUID version 7 primary keys for optimal database performance and temporal ordering</item>
/// <item>Automatic username and email normalization for case-insensitive operations</item>
/// <item>Full compatibility with ASP.NET Core Identity user management features</item>
/// <item>Support for authentication, authorization, claims, roles, and external logins</item>
/// <item>Integration with PostgreSQL CITEXT columns for efficient text comparisons</item>
/// </list>
/// 
/// UUID version 7 advantages over traditional GUIDs:
/// <list type="bullet">
/// <item>Time-ordered generation ensures better database index performance</item>
/// <item>Lexicographically sortable by creation timestamp</item>
/// <item>Reduced B-tree fragmentation in PostgreSQL indexes</item>
/// <item>Improved query performance for range operations</item>
/// <item>Better replication and distributed system behavior</item>
/// </list>
/// 
/// The automatic normalization of usernames and emails ensures consistent lookups
/// regardless of case, leveraging the case-insensitive CITEXT data type in PostgreSQL
/// for optimal performance without the overhead of constant case conversion.
/// </remarks>
/// <example>
/// Creating and managing application users:
/// <code>
/// // Create a new user with automatic UUID generation
/// var user = new ApplicationUser("john.doe@example.com");
/// 
/// // Create a user using the default constructor
/// var newUser = new ApplicationUser();
/// newUser.UserName = "jane.smith@example.com";
/// newUser.Email = "jane.smith@example.com";
/// 
/// // Register the user with a password
/// var result = await userManager.CreateAsync(user, "SecurePassword123!");
/// 
/// // Find user by email (case-insensitive)
/// var foundUser = await userManager.FindByEmailAsync("JOHN.DOE@EXAMPLE.COM");
/// 
/// // Add user to role
/// await userManager.AddToRoleAsync(user, "Administrator");
/// 
/// // Check user claims
/// var claims = await userManager.GetClaimsAsync(user);
/// </code>
/// </example>
/// <seealso cref="ApplicationRole"/>
/// <seealso cref="SecurityDbContext"/>
/// <seealso cref="IdentityUser{TKey}"/>
public class ApplicationUser : IdentityUser<Guid>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ApplicationUser"/> class with an automatically generated UUID version 7 primary key.
    /// </summary>
    /// <remarks>
    /// This constructor creates a new user account with a time-ordered UUID version 7 identifier.
    /// The UUID version 7 format provides superior database performance characteristics
    /// compared to standard random GUIDs by maintaining temporal ordering and reducing index fragmentation.
    /// 
    /// After instantiation, you should configure the user properties such as:
    /// <list type="bullet">
    /// <item><see cref="IdentityUser{TKey}.UserName"/> - The user's login name</item>
    /// <item><see cref="IdentityUser{TKey}.Email"/> - The user's email address</item>
    /// <item><see cref="IdentityUser{TKey}.NormalizedUserName"/> - Uppercase username for lookups</item>
    /// <item><see cref="IdentityUser{TKey}.NormalizedEmail"/> - Uppercase email for lookups</item>
    /// </list>
    /// 
    /// Alternatively, use the parameterized constructor for automatic property initialization.
    /// </remarks>
    /// <example>
    /// <code>
    /// var user = new ApplicationUser();
    /// user.UserName = "john.doe@example.com";
    /// user.Email = "john.doe@example.com";
    /// user.NormalizedUserName = "JOHN.DOE@EXAMPLE.COM";
    /// user.NormalizedEmail = "JOHN.DOE@EXAMPLE.COM";
    /// 
    /// var result = await userManager.CreateAsync(user, "Password123!");
    /// </code>
    /// </example>
    public ApplicationUser() : base()
    {
        Id = Guid.CreateVersion7();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApplicationUser"/> class with the specified username/email.
    /// Automatically generates a UUID version 7 primary key and normalizes the username and email for consistent lookups.
    /// </summary>
    /// <param name="userName">The username for the user account. This value is also used as the email address and both are normalized to uppercase for efficient database operations.</param>
    /// <remarks>
    /// This constructor provides a convenient way to create a fully configured user account in a single step.
    /// The username serves dual purpose as both the login identifier and email address, which is a common
    /// pattern for modern web applications where email addresses are used as primary identifiers.
    /// 
    /// The normalization process uses <see cref="string.ToUpperInvariant()"/> to ensure consistent
    /// case-insensitive lookups that align with the PostgreSQL CITEXT column configuration.
    /// This eliminates the need for runtime case conversion during database queries.
    /// 
    /// Both <see cref="IdentityUser{TKey}.UserName"/> and <see cref="IdentityUser{TKey}.Email"/> are set to the same value,
    /// while their normalized counterparts are automatically generated for optimal database performance.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Create a user with email as username
    /// var user = new ApplicationUser("jane.smith@example.com");
    /// // user.UserName = "jane.smith@example.com"
    /// // user.Email = "jane.smith@example.com"
    /// // user.NormalizedUserName = "JANE.SMITH@EXAMPLE.COM"
    /// // user.NormalizedEmail = "JANE.SMITH@EXAMPLE.COM"
    /// // user.Id = [auto-generated UUID v7]
    /// 
    /// // Register with password
    /// var result = await userManager.CreateAsync(user, "SecurePassword123!");
    /// 
    /// // Later lookup works case-insensitively
    /// var foundUser = await userManager.FindByEmailAsync("JANE.SMITH@EXAMPLE.COM");
    /// </code>
    /// </example>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="userName"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="userName"/> is empty or contains only whitespace.</exception>
    public ApplicationUser(string userName) : this()
    {
        ArgumentException.ThrowIfNotValidUserName(userName);
        
        UserName = userName;
        NormalizedUserName = userName.ToUpperInvariant();
        Email = userName;
        NormalizedEmail = userName.ToUpperInvariant();
    }
}