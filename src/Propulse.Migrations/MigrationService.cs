using DbUp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Propulse.Core;
using System.Reflection;

namespace Propulse.Migrations;

/// <summary>
/// Provides database migration services using DbUp to apply schema changes to PostgreSQL databases.
/// </summary>
/// <remarks>
/// This service manages the execution of embedded SQL scripts to upgrade database schemas.
/// It uses DbUp to track applied migrations and ensures scripts are executed in the correct order.
/// The service supports variable substitution and transactional script execution.
/// </remarks>
public class MigrationService : IMigrationService
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MigrationService"/> class for derived classes.
    /// </summary>
    /// <remarks>
    /// This constructor is protected and intended for use by derived classes.
    /// Uses a null logger instance by default.
    /// </remarks>
    protected MigrationService()
    {
        ScriptFilter = script => script.EndsWith(".sql", StringComparison.OrdinalIgnoreCase);
        ScriptAssembly = Assembly.GetExecutingAssembly();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MigrationService"/> class with the specified logger.
    /// </summary>
    /// <param name="logger">The logger instance to use for logging migration operations.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="logger"/> is null.</exception>
    public MigrationService(ILogger<MigrationService> logger) : this()
    {
        ArgumentNullException.ThrowIfNull(logger);
        Logger = logger;
    }

    /// <summary>
    /// Gets the logger instance used for logging migration operations.
    /// </summary>
    /// <value>
    /// The logger instance. Defaults to <see cref="NullLogger.Instance"/> if not provided.
    /// </value>
    protected ILogger Logger { get; } = NullLogger.Instance;

    /// <summary>
    /// Gets or sets the assembly containing the embedded SQL migration scripts.
    /// </summary>
    /// <value>
    /// The assembly containing migration scripts. Defaults to the executing assembly.
    /// </value>
    /// <remarks>
    /// This property allows derived classes to specify a different assembly containing migration scripts.
    /// </remarks>
    protected Assembly ScriptAssembly { get; set; }

    /// <summary>
    /// Gets or sets the filter function used to identify SQL migration scripts.
    /// </summary>
    /// <value>
    /// A function that takes a script name and returns true if it should be included in migrations.
    /// Defaults to filtering files that end with ".sql" (case-insensitive).
    /// </value>
    /// <remarks>
    /// This property allows customization of which embedded resources are treated as migration scripts.
    /// </remarks>
    protected Func<string, bool> ScriptFilter { get; set; } 

    /// <inheritdoc />
    public Task ApplySchemaChangesAsync(string connectionString, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        Logger.LogInformation("Applying schema changes to the database using connection string: {ConnectionString}", connectionString);
        Logger.LogInformation("Using script assembly: {ScriptAssembly}", ScriptAssembly.FullName);
        
        var variables = new Dictionary<string, string>
        {
            { "SecuritySchemaName", DatabaseConstants.SecuritySchemaName },
            { "ArticlesSchemaName", DatabaseConstants.ArticlesSchemaName }
        };

        var upgrader = DeployChanges.To
            .PostgresqlDatabase(connectionString)
            .JournalToPostgresqlTable("public", "DbUpSchemaVersions")
            .WithScriptsEmbeddedInAssembly(ScriptAssembly, ScriptFilter)
            .WithTransactionPerScript()
            .WithVariables(variables)
            .LogTo(Logger)
            .LogScriptOutput()
            .Build();

        Logger.LogDebug("Starting database migration");
        var result = upgrader.PerformUpgrade();

        if (result.Successful)
        {
            int appliedScriptsCount = result.Scripts.Count();
            Logger.LogInformation("Database migration completed successfully. # Scripts applied: {Count}", appliedScriptsCount);
            return Task.CompletedTask;
        }
        else
        {
            Logger.LogError(result.Error, "Database migration failed. Script={ScriptName}, Error={ErrorMessage}", result.ErrorScript.Name, result.Error.Message);
            throw new InvalidOperationException($"Database migration failed.  Script={result.ErrorScript.Name}", result.Error);
        }
    }
}