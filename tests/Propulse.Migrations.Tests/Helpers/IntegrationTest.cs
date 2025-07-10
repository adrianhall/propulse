using DotNet.Testcontainers.Builders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Npgsql;
using Testcontainers.PostgreSql;
using Xunit;

namespace Propulse.Migrations.Tests.Helpers;

/// <summary>
/// Base class for integration tests that require a PostgreSQL database container.
/// </summary>
/// <remarks>
/// This class provides a common setup for integration tests using TestContainers to create
/// an isolated PostgreSQL database instance. Tests inheriting from this class will have
/// access to a fresh database container for each test run, ensuring test isolation.
/// The class is part of the <see cref="MigrationCollection"/> to ensure tests run sequentially
/// and avoid conflicts with shared database resources.
/// </remarks>
[Collection(MigrationCollection.Name)]
public abstract class IntegrationTest : IAsyncLifetime
{
    /// <summary>
    /// The PostgreSQL container instance used for testing.
    /// </summary>
    /// <remarks>
    /// This container is configured with:
    /// - PostgreSQL 17 Alpine image for lightweight, fast startup
    /// - Null logger to reduce test output noise
    /// - Wait strategy to ensure the container is ready before tests run
    /// - Automatic cleanup after test completion
    /// </remarks>
    private readonly PostgreSqlContainer container = new PostgreSqlBuilder()
        .WithImage("postgres:17-alpine")
        .WithLogger(NullLogger.Instance)
        .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5432))
        .WithCleanUp(true)
        .Build();

    /// <summary>
    /// Initializes the test fixture, setting up any necessary resources.
    /// </summary>
    /// <returns>A task representing the asynchronous initialization operation.</returns>
    public async Task InitializeAsync()
    {
        await container.StartAsync();
    }

    /// <summary>
    /// Cleans up resources after all tests in the collection have run.
    /// </summary>
    /// <returns>A task representing the asynchronous cleanup operation.</returns>
    public Task DisposeAsync() => container.DisposeAsync().AsTask();

    /// <summary>
    /// Gets the connection string for the PostgreSQL test container.
    /// </summary>
    /// <value>
    /// A connection string that can be used to connect to the test database container.
    /// </value>
    /// <remarks>
    /// This property provides access to the connection string after the container has been started.
    /// The connection string is dynamically generated based on the container's assigned port and credentials.
    /// </remarks>
    protected string ConnectionString { get => container.GetConnectionString(); }

    /// <summary>
    /// Executes a SQL query against the test database and returns the results.
    /// </summary>
    /// <param name="sql">The SQL statement to execute.</param>
    /// <param name="parameters">Optional parameters to substitute in the SQL statement.</param>
    /// <returns>A list of rows, where each row is represented as a dictionary of column names to values.</returns>
    /// <remarks>
    /// This method provides a convenient way to execute SQL queries during tests and examine the results.
    /// Parameters are substituted using PostgreSQL's parameterized query format (@paramName).
    /// The method handles type conversion and null values appropriately.
    /// </remarks>
    /// <exception cref="InvalidOperationException">Thrown when the container is not started or the SQL execution fails.</exception>
    protected List<Dictionary<string, object?>> Query(string sql, params (string, object?)[] parameters)
    {
        using var connection = new NpgsqlConnection(ConnectionString);
        connection.Open();

        using var command = new NpgsqlCommand(sql, connection);
        
        // Add parameters to the command
        foreach (var (name, value) in parameters)
        {
            command.Parameters.AddWithValue(name, value ?? DBNull.Value);
        }

        var results = new List<Dictionary<string, object?>>();
        
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var row = new Dictionary<string, object?>();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                var columnName = reader.GetName(i);
                var value = reader.IsDBNull(i) ? null : reader.GetValue(i);
                row[columnName] = value;
            }
            results.Add(row);
        }

        return results;
    }

    /// <summary>
    /// Gets the list of applied migration scripts from the DbUp journal table.
    /// </summary>
    /// <returns>
    /// A list of script names that have been applied to the database.
    /// Returns an empty list if the DbUp journal table doesn't exist (i.e., no migrations have been run).
    /// </returns>
    /// <remarks>
    /// This method first checks if the DbUpSchemaVersions table exists before querying it.
    /// This handles the edge case where no migrations have been applied and DbUp hasn't created
    /// its journal table yet.
    /// </remarks>
    protected List<string> GetAppliedScripts()
    {
        // First check if the DbUp journal table exists
        var journalTableExists = Query("SELECT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'DbUpSchemaVersions' AND table_schema = 'public')");
        var tableExists = (bool)(journalTableExists[0]["exists"] ?? false);
        
        if (!tableExists)
        {
            return [];
        }

        // Table exists, so query for applied scripts
        var appliedScripts = Query("SELECT scriptname FROM public.\"DbUpSchemaVersions\" ORDER BY scriptname");
        return [..appliedScripts.Select(row => (string)row["scriptname"]!)];
    }
}
