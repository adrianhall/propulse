using DotNet.Testcontainers.Builders;
using Microsoft.Extensions.Logging.Abstractions;
using Propulse.Migrations;
using System.Text;
using Testcontainers.PostgreSql;

namespace Propulse.Web.Tests.Helpers;


/// <summary>
/// Provides a shared PostgreSQL database instance for integration tests using TestContainers.
/// Manages the lifecycle of a containerized PostgreSQL database with automatic schema migration
/// and cleanup for reliable, isolated test execution.
/// </summary>
/// <remarks>
/// This fixture class implements the <see cref="IAsyncLifetime"/> interface to provide proper
/// setup and teardown of database resources for xUnit test collections. It ensures that each
/// test run starts with a clean, fully migrated database schema.
/// 
/// Key features:
/// <list type="bullet">
/// <item>PostgreSQL 17 Alpine container for lightweight, fast test execution</item>
/// <item>Automatic schema migration using the MigrationService</item>
/// <item>Type reloading for proper Entity Framework Core integration</item>
/// <item>Automatic cleanup when tests complete</item>
/// <item>Helper methods for database schema introspection</item>
/// </list>
/// 
/// The fixture is designed to be used with xUnit's collection fixtures to share the same
/// database instance across multiple test classes while ensuring proper isolation between
/// test runs.
/// 
/// Database configuration:
/// <list type="bullet">
/// <item>Image: postgres:17-alpine (lightweight PostgreSQL 17)</item>
/// <item>Extensions: citext, uuid-ossp (automatically applied via migrations)</item>
/// <item>Schema: security (for Identity tables) + any additional schemas</item>
/// <item>Wait strategy: Port 5432 availability check</item>
/// </list>
/// </remarks>
/// <example>
/// Using the fixture in a test collection:
/// <code>
/// [CollectionDefinition("Database")]
/// public class DatabaseCollection : ICollectionFixture&lt;DatabaseFixture&gt;
/// {
///     // This class has no code, and is never created. Its purpose is simply
///     // to be the place to apply [CollectionDefinition] and all the
///     // ICollectionFixture&lt;&gt; interfaces.
/// }
/// 
/// [Collection("Database")]
/// public class SecurityDbContextTests
/// {
///     private readonly DatabaseFixture _fixture;
/// 
///     public SecurityDbContextTests(DatabaseFixture fixture)
///     {
///         _fixture = fixture;
///     }
/// 
///     [Fact]
///     public async Task CanConnectToDatabase()
///     {
///         // Arrange
///         var options = new DbContextOptionsBuilder&lt;SecurityDbContext&gt;()
///             .UseNpgsql(_fixture.ConnectionString)
///             .Options;
/// 
///         // Act & Assert
///         using var context = new SecurityDbContext(options);
///         var canConnect = await context.Database.CanConnectAsync();
///         canConnect.Should().BeTrue();
///     }
/// 
///     [Fact]
///     public void HasCorrectTables()
///     {
///         // Act
///         var tables = _fixture.GetTableNames("security");
/// 
///         // Assert
///         tables.Should().Contain(["Roles", "Users", "UserRoles"]);
///     }
/// }
/// </code>
/// </example>
/// <seealso cref="SecurityDbContext"/>
/// <seealso cref="MigrationService"/>
/// <seealso cref="IAsyncLifetime"/>
public class DatabaseFixture : IAsyncLifetime
{
    /// <summary>
    /// The PostgreSQL TestContainer instance configured for integration testing.
    /// </summary>
    /// <remarks>
    /// This container is configured with:
    /// <list type="bullet">
    /// <item>PostgreSQL 17 Alpine image for optimal performance and minimal size</item>
    /// <item>Null logger to reduce test output noise</item>
    /// <item>Wait strategy that ensures port 5432 is available before proceeding</item>
    /// <item>Automatic cleanup when the fixture is disposed</item>
    /// </list>
    /// </remarks>
    private readonly PostgreSqlContainer container = new PostgreSqlBuilder()
        .WithImage("postgres:17-alpine")
        .WithLogger(NullLogger.Instance)
        .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5432))
        .WithCleanUp(true)
        .Build();

    /// <summary>
    /// Initializes the database fixture by starting the PostgreSQL container and applying schema migrations.
    /// This method is called once before any tests in the collection are executed.
    /// </summary>
    /// <returns>A task representing the asynchronous initialization operation.</returns>
    /// <remarks>
    /// The initialization process includes:
    /// <list type="number">
    /// <item>Starting the PostgreSQL TestContainer</item>
    /// <item>Applying all schema migrations using the MigrationService</item>
    /// <item>Reloading PostgreSQL types to ensure EF Core recognizes custom types (UUID, CITEXT)</item>
    /// </list>
    /// 
    /// The type reload is crucial for Entity Framework Core to properly map .NET types
    /// to PostgreSQL custom types like CITEXT and UUID, especially after schema changes.
    /// </remarks>
    /// <example>
    /// This method is automatically called by xUnit when using collection fixtures:
    /// <code>
    /// // xUnit automatically calls this method before running tests
    /// // No manual invocation required
    /// </code>
    /// </example>
    /// <exception cref="InvalidOperationException">Thrown if the container fails to start or migrations fail to apply.</exception>
    public async Task InitializeAsync()
    {
        // Start the PostgreSQL container
        await container.StartAsync();

        // Apply schema migrations to the database.
        var connectionString = container.GetConnectionString();
        var migrationService = new MigrationService(NullLogger<MigrationService>.Instance);
        await migrationService.ApplySchemaChangesAsync(connectionString);

        // Reload types in the Npgsql connection to ensure that EF Core recognizes the new schema
        using var connection = new Npgsql.NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        await connection.ReloadTypesAsync();
    }

    /// <summary>
    /// Disposes the database fixture by stopping and cleaning up the PostgreSQL container.
    /// This method is called once after all tests in the collection have completed.
    /// </summary>
    /// <returns>A task representing the asynchronous disposal operation.</returns>
    /// <remarks>
    /// The disposal process automatically:
    /// <list type="bullet">
    /// <item>Stops the running PostgreSQL container</item>
    /// <item>Removes the container and associated resources</item>
    /// <item>Cleans up any temporary files or networks created by TestContainers</item>
    /// </list>
    /// 
    /// This ensures that no database containers are left running after test execution,
    /// preventing resource leaks and port conflicts in subsequent test runs.
    /// </remarks>
    public Task DisposeAsync() => container.DisposeAsync().AsTask();

    /// <summary>
    /// Gets the connection string for the running PostgreSQL container.
    /// </summary>
    /// <value>
    /// A PostgreSQL connection string that can be used to connect to the test database.
    /// The connection string includes the dynamically assigned host, port, database name,
    /// username, and password for the container instance.
    /// </value>
    /// <remarks>
    /// This property provides the connection string needed to configure Entity Framework
    /// DbContext instances or create direct database connections for testing purposes.
    /// 
    /// The connection string is dynamically generated based on the container's actual
    /// network configuration, ensuring it works regardless of available ports or
    /// Docker network setup.
    /// </remarks>
    /// <example>
    /// Using the connection string with Entity Framework:
    /// <code>
    /// var options = new DbContextOptionsBuilder&lt;SecurityDbContext&gt;()
    ///     .UseNpgsql(_fixture.ConnectionString)
    ///     .Options;
    /// 
    /// using var context = new SecurityDbContext(options);
    /// </code>
    /// </example>
    public string ConnectionString { get => container.GetConnectionString(); }
}