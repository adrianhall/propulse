namespace Propulse.Core;

/// <summary>
/// Contains constant values for database schema names and other database-related constants.
/// </summary>
/// <remarks>
/// This class centralizes database naming conventions to ensure consistency across the application.
/// All database schemas should be referenced through these constants rather than using string literals.
/// </remarks>
public static class DatabaseConstants
{
    /// <summary>
    /// The schema name for articles-related database objects.
    /// </summary>
    /// <remarks>
    /// This schema contains tables, views, and other database objects related to article management.
    /// </remarks>
    public const string ArticlesSchemaName = "articles";

    /// <summary>
    /// The schema name for security-related database objects.
    /// </summary>
    /// <remarks>
    /// This schema contains tables, views, and other database objects related to authentication,
    /// authorization, and user management.
    /// </remarks>
    public const string SecuritySchemaName = "security";

}
