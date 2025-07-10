using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Propulse.Core;
using Propulse.Persistence.Entities;

namespace Propulse.Persistence;

/// <summary>
/// Entity Framework Core database context for ASP.NET Core Identity authentication and authorization.
/// Provides access to user accounts, roles, claims, and related authentication data stored in PostgreSQL.
/// </summary>
/// <remarks>
/// This context is specifically configured for PostgreSQL with the following optimizations:
/// <list type="bullet">
/// <item>Uses UUID primary keys for better performance and distribution</item>
/// <item>Leverages CITEXT extension for case-insensitive text comparisons</item>
/// <item>Stores all Identity tables in a dedicated security schema</item>
/// <item>Follows Propulse naming conventions for table names</item>
/// </list>
/// 
/// The context inherits from <see cref="IdentityDbContext{TUser, TRole, TKey}"/> which provides
/// all the standard ASP.NET Core Identity functionality including user management, role-based
/// authorization, claims-based authorization, and external login provider support.
/// </remarks>
/// <example>
/// Basic usage with dependency injection:
/// <code>
/// // In Program.cs or Startup.cs
/// services.AddDbContext&lt;SecurityDbContext&gt;(options =&gt;
///     options.UseNpgsql(connectionString));
/// 
/// // In a controller or service
/// public class UserService
/// {
///     private readonly SecurityDbContext _context;
///     
///     public UserService(SecurityDbContext context)
///     {
///         _context = context;
///     }
///     
///     public async Task&lt;ApplicationUser&gt; GetUserAsync(string email)
///     {
///         return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
///     }
/// }
/// </code>
/// </example>
public class SecurityDbContext(DbContextOptions<SecurityDbContext> options)
    : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>(options)
{
    /// <summary>
    /// Configures the Entity Framework model for the security database context.
    /// Sets up PostgreSQL-specific extensions, schema configuration, and table naming conventions.
    /// </summary>
    /// <param name="builder">The model builder used to configure the database context model.</param>
    /// <remarks>
    /// This method performs the following configuration tasks:
    /// <list type="number">
    /// <item>Enables required PostgreSQL extensions (citext, uuid-ossp)</item>
    /// <item>Sets the default schema to the security schema</item>
    /// <item>Calls the base Identity configuration</item>
    /// <item>Renames Identity tables to match Propulse conventions</item>
    /// </list>
    /// 
    /// The PostgreSQL extensions provide:
    /// <list type="bullet">
    /// <item><c>citext</c>: Case-insensitive text type for usernames and emails</item>
    /// <item><c>uuid-ossp</c>: UUID generation functions for primary keys</item>
    /// </list>
    /// 
    /// Table naming follows the convention of removing the "AspNet" prefix from
    /// standard Identity table names for cleaner, more intuitive names.
    /// </remarks>
    /// <example>
    /// The method transforms standard Identity table names as follows:
    /// <code>
    /// AspNetRoles        → Roles
    /// AspNetRoleClaims   → RoleClaims
    /// AspNetUsers        → Users
    /// AspNetUserRoles    → UserRoles
    /// AspNetUserClaims   → UserClaims
    /// AspNetUserLogins   → UserLogins
    /// AspNetUserTokens   → UserTokens
    /// </code>
    /// </example>
    protected override void OnModelCreating(ModelBuilder builder)
    {
        // Configure the model to allow the use of installed extensions
        builder.HasPostgresExtension("citext");
        builder.HasPostgresExtension("uuid-ossp");

        // Set up the default schema for the database
        builder.HasDefaultSchema(DatabaseConstants.SecuritySchemaName);

        // Create the base identity model.
        base.OnModelCreating(builder);

        // Rename the default Identity tables to match the Propulse naming conventions.
        builder.Entity<ApplicationRole>(b => b.ToTable("Roles"));
        builder.Entity<IdentityRoleClaim<Guid>>(b => b.ToTable("RoleClaims"));
        builder.Entity<ApplicationUser>(b => b.ToTable("Users"));
        builder.Entity<IdentityUserRole<Guid>>(b => b.ToTable("UserRoles"));
        builder.Entity<IdentityUserClaim<Guid>>(b => b.ToTable("UserClaims"));
        builder.Entity<IdentityUserLogin<Guid>>(b => b.ToTable("UserLogins"));
        builder.Entity<IdentityUserToken<Guid>>(b => b.ToTable("UserTokens"));

        // Apply any model configurations from this assembly.
        // Commented out right now as there are no configurations to apply.
        //builder.ApplyConfigurationsFromAssembly(typeof(SecurityDbContext).Assembly);
    }
}
