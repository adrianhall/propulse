using Microsoft.Extensions.Logging;
using System.Reflection;

namespace Propulse.Migrations.Tests.Helpers;

/// <summary>
/// A test-friendly version of <see cref="MigrationService"/> that exposes protected members for testing.
/// </summary>
/// <remarks>
/// This class inherits from <see cref="MigrationService"/> and provides public access to protected properties
/// and methods, allowing tests to inspect and modify the migration service's internal state.
/// This is useful for unit testing scenarios where you need to verify configuration or mock dependencies.
/// </remarks>
public class TestMigrationService : MigrationService
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TestMigrationService"/> class with a null logger.
    /// </summary>
    /// <remarks>
    /// This constructor uses the base class's protected constructor, resulting in a null logger instance.
    /// Primarily used for testing scenarios where logging is not required.
    /// </remarks>
    public TestMigrationService() : base()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TestMigrationService"/> class with the specified logger.
    /// </summary>
    /// <param name="logger">The logger instance to use for logging migration operations.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="logger"/> is null.</exception>
    public TestMigrationService(ILogger<MigrationService> logger) : base(logger)
    {
    }

    /// <summary>
    /// Gets the logger instance used for logging migration operations.
    /// </summary>
    /// <value>
    /// The logger instance from the base class. May be a null logger if not provided.
    /// </value>
    /// <remarks>
    /// This property exposes the base class's protected Logger property for testing purposes,
    /// allowing tests to verify logging behavior or inject mock loggers.
    /// </remarks>
    public new ILogger Logger { get => base.Logger; }

    /// <summary>
    /// Gets or sets the assembly containing the embedded SQL migration scripts.
    /// </summary>
    /// <value>
    /// The assembly containing migration scripts. Defaults to the executing assembly.
    /// </value>
    /// <remarks>
    /// This property exposes the base class's protected ScriptAssembly property for testing purposes,
    /// allowing tests to specify different assemblies containing test migration scripts.
    /// </remarks>
    public new Assembly ScriptAssembly
    {
        get => base.ScriptAssembly;
        set => base.ScriptAssembly = value;
    }

    /// <summary>
    /// Gets or sets the filter function used to identify SQL migration scripts.
    /// </summary>
    /// <value>
    /// A function that takes a script name and returns true if it should be included in migrations.
    /// Defaults to filtering files that end with ".sql" (case-insensitive).
    /// </value>
    /// <remarks>
    /// This property exposes the base class's protected ScriptFilter property for testing purposes,
    /// allowing tests to customize which embedded resources are treated as migration scripts.
    /// Useful for testing specific subsets of migration scripts or custom naming conventions.
    /// </remarks>
    public new Func<string, bool> ScriptFilter
    {
        get => base.ScriptFilter;
        set => base.ScriptFilter = value;
    }
}