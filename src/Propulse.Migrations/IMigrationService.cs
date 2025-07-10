namespace Propulse.Migrations;

/// <summary>
/// Provides database migration services for applying schema changes to the database.
/// </summary>
/// <remarks>
/// This service is responsible for managing database schema upgrades using DbUp.
/// It should be implemented to handle the execution of SQL scripts in a controlled manner.
/// </remarks>
public interface IMigrationService
{
    /// <summary>
    /// Applies pending schema changes to the database using the specified connection string.
    /// </summary>
    /// <param name="connectionString">The database connection string to use for applying migrations.</param>
    /// <param name="cancellationToken">A token to cancel the operation if needed.</param>
    /// <returns>A task representing the asynchronous migration operation.</returns>
    /// <remarks>
    /// This method should execute all pending SQL scripts in the correct order to bring
    /// the database schema up to the latest version. The implementation should be idempotent
    /// and safe to run multiple times.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="connectionString"/> is null or empty.</exception>
    /// <exception cref="InvalidOperationException">Thrown when migration fails or database is in an inconsistent state.</exception>
    Task ApplySchemaChangesAsync(string connectionString, CancellationToken cancellationToken = default);
}
