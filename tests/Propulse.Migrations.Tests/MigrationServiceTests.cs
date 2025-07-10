using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Propulse.Migrations.Tests.Helpers;
using System.Reflection;

namespace Propulse.Migrations.Tests;

/// <summary>
/// Integration tests for the MigrationService class.
/// </summary>
[Collection(MigrationCollection.Name)]
public class MigrationServiceTests : IntegrationTest
{
    /// <summary>
    /// Tests that the migration service successfully applies valid migration scripts.
    /// </summary>
    [Fact]
    public async Task ApplySchemaChangesAsync_WithValidScripts_ShouldSucceed()
    {
        // Arrange
        var logger = new FakeLogger<MigrationService>();
        var service = new TestMigrationService(logger)
        {
            ScriptAssembly = Assembly.GetExecutingAssembly(),
            ScriptFilter = script => script.EndsWith(".sql") && !script.Contains("bad")
        };

        // Act
        await service.ApplySchemaChangesAsync(ConnectionString);

        // Assert
        var appliedScripts = GetAppliedScripts();
        appliedScripts.Should().HaveCount(2).And.ContainInConsecutiveOrder([
            "Propulse.Migrations.Tests.Scripts.0001-CreateTestTable.sql",
            "Propulse.Migrations.Tests.Scripts.0002-AddEmailColumn.sql"
        ]);

        // Verify the table was created and modified correctly
        var tableExists = Query("SELECT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'test_table')");
        tableExists[0]["exists"].Should().Be(true);

        var columns = Query("SELECT column_name FROM information_schema.columns WHERE table_name = 'test_table' ORDER BY ordinal_position");
        columns.Should().HaveCount(4); // id, name, created_at, email
        columns[0]["column_name"].Should().Be("id");
        columns[1]["column_name"].Should().Be("name");
        columns[2]["column_name"].Should().Be("created_at");
        columns[3]["column_name"].Should().Be("email");

        // Verify logging
        logger.Collector.GetSnapshot().Should().Contain(log =>
            log.Level == LogLevel.Information &&
            log.Message!.Contains("Applying schema changes"));

        logger.Collector.GetSnapshot().Should().Contain(log =>
            log.Level == LogLevel.Information && log.Message!.Contains("success"));
    }

    /// <summary>
    /// Tests that the migration service handles invalid SQL scripts by throwing an exception.
    /// </summary>
    [Fact]
    public async Task ApplySchemaChangesAsync_WithInvalidScript_ShouldThrowException()
    {
        // Arrange
        var logger = new FakeLogger<MigrationService>();
        var service = new TestMigrationService(logger)
        {
            ScriptAssembly = Assembly.GetExecutingAssembly(),
            ScriptFilter = script => script.Contains("bad.sql")
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.ApplySchemaChangesAsync(ConnectionString));

        exception.Message.Should().Contain("Database migration failed").And.Contain("bad.sql");
        exception.InnerException.Should().NotBeNull();

        // Verify error logging - the error should be logged and contain the script name
        logger.Collector.GetSnapshot().Should().Contain(log => log.Level == LogLevel.Error && log.Message!.Contains("bad.sql"));
    }

    /// <summary>
    /// Tests that the migration service is idempotent and doesn't reapply already applied scripts.
    /// </summary>
    [Fact]
    public async Task ApplySchemaChangesAsync_RunTwice_ShouldBeIdempotent()
    {
        // Arrange
        var logger = new FakeLogger<MigrationService>();
        var service = new TestMigrationService(logger)
        {
            ScriptAssembly = Assembly.GetExecutingAssembly(),
            ScriptFilter = script => script.EndsWith(".sql") && !script.Contains("bad")
        };

        // Act - Run migrations twice
        await service.ApplySchemaChangesAsync(ConnectionString);
        await service.ApplySchemaChangesAsync(ConnectionString);

        // Assert - Should still have the same number of records in the journal
        var appliedScripts = GetAppliedScripts();
        appliedScripts.Should().HaveCount(2); // Only 2 scripts should be recorded

        // Verify the table structure is still correct
        var columns = Query("SELECT column_name FROM information_schema.columns WHERE table_name = 'test_table' ORDER BY ordinal_position");
        columns.Should().HaveCount(4); // id, name, created_at, email
    }

    /// <summary>
    /// Tests that the migration service properly handles null or empty connection strings.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task ApplySchemaChangesAsync_WithNullConnectionString_ShouldThrowException(string? connectionString)
    {
        // Arrange
        var logger = new FakeLogger<MigrationService>();
        var service = new TestMigrationService(logger);

        // Act & Assert
        Func<Task> act = () => service.ApplySchemaChangesAsync(connectionString!);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>().WithParameterName("connectionString");
    }

    /// <summary>
    /// Tests that the migration service throws an exception when the connection string is invalid.
    /// </summary>
    [Fact]
    public async Task ApplySchemaChangesAsync_WithInvalidConnectionString_ShouldThrowException()
    {
        // Arrange
        var logger = new FakeLogger<MigrationService>();
        var service = new TestMigrationService(logger)
        {
            ScriptAssembly = Assembly.GetExecutingAssembly(),
            ScriptFilter = script => script.EndsWith(".sql") && !script.Contains("bad")
        };

        // Act & Assert
        Func<Task> act = () => service.ApplySchemaChangesAsync("Server=nonexistent;Database=test;User Id=user;Password=pass;");

        // Assert
        await act.Should().ThrowAsync<Exception>();
    }

    /// <summary>
    /// Tests that the migration service works with custom script filters.
    /// </summary>
    [Fact]
    public async Task ApplySchemaChangesAsync_WithCustomScriptFilter_ShouldOnlyApplyFilteredScripts()
    {
        // Arrange
        var logger = new FakeLogger<MigrationService>();
        var service = new TestMigrationService(logger)
        {
            ScriptAssembly = Assembly.GetExecutingAssembly(),
            ScriptFilter = script => script.Contains("0001-CreateTestTable.sql") // Only apply the first script
        };

        // Act
        await service.ApplySchemaChangesAsync(ConnectionString);

        // Assert
        var appliedScripts = GetAppliedScripts();
        appliedScripts.Should().HaveCount(1);
        appliedScripts[0].Should().Be("Propulse.Migrations.Tests.Scripts.0001-CreateTestTable.sql");

        // Verify only the table was created (no email column)
        var columns = Query("SELECT column_name FROM information_schema.columns WHERE table_name = 'test_table' ORDER BY ordinal_position");
        columns.Should().HaveCount(3); // id, name, created_at (no email column)
        columns.Select(c => c["column_name"]).Should().NotContain("email");
    }

    /// <summary>
    /// Tests that the migration service creates the DbUp journal table correctly.
    /// </summary>
    [Fact]
    public async Task ApplySchemaChangesAsync_ShouldCreateDbUpJournalTable()
    {
        // Arrange
        var logger = new FakeLogger<MigrationService>();
        var service = new TestMigrationService(logger)
        {
            ScriptAssembly = Assembly.GetExecutingAssembly(),
            ScriptFilter = script => script.EndsWith(".sql") && !script.Contains("bad")
        };

        // Act
        await service.ApplySchemaChangesAsync(ConnectionString);

        // Assert
        var journalTableExists = Query("SELECT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'DbUpSchemaVersions' AND table_schema = 'public')");
        journalTableExists[0]["exists"].Should().Be(true);

        // Verify the journal table structure
        var journalColumns = Query("SELECT column_name FROM information_schema.columns WHERE table_name = 'DbUpSchemaVersions' AND table_schema = 'public' ORDER BY ordinal_position");
        journalColumns.Should().HaveCount(3);
        journalColumns[0]["column_name"].Should().Be("schemaversionsid");
        journalColumns[1]["column_name"].Should().Be("scriptname");
        journalColumns[2]["column_name"].Should().Be("applied");
    }

    /// <summary>
    /// Tests that the migration service logs the correct information during successful migrations.
    /// </summary>
    [Fact]
    public async Task ApplySchemaChangesAsync_ShouldLogMigrationProgress()
    {
        // Arrange
        var logger = new FakeLogger<MigrationService>();
        var service = new TestMigrationService(logger)
        {
            ScriptAssembly = Assembly.GetExecutingAssembly(),
            ScriptFilter = script => script.EndsWith(".sql") && !script.Contains("bad")
        };

        // Act
        await service.ApplySchemaChangesAsync(ConnectionString);

        // Assert
        var logs = logger.Collector.GetSnapshot();

        // Should log connection string (masked for security)
        logs.Should().Contain(log =>
            log.Level == LogLevel.Information &&
            log.Message!.Contains("Applying schema changes"));

        // Should log script assembly
        logs.Should().Contain(log =>
            log.Level == LogLevel.Information &&
            log.Message!.Contains("Using script assembly"));

        // Should log debug message about starting migration
        logs.Should().Contain(log =>
            log.Level == LogLevel.Debug &&
            log.Message!.Contains("Starting database migration"));

        // Should log success message with script count
        logs.Should().Contain(log =>
            log.Level == LogLevel.Information &&
            log.Message!.Contains("Database migration completed successfully") &&
            log.Message!.Contains("Scripts applied: 2"));
    }

    /// <summary>
    /// Tests that the migration service handles the case where no scripts match the filter.
    /// </summary>
    [Fact]
    public async Task ApplySchemaChangesAsync_WithNoMatchingScripts_ShouldSucceedWithZeroScripts()
    {
        // Arrange
        var logger = new FakeLogger<MigrationService>();
        var service = new TestMigrationService(logger)
        {
            ScriptAssembly = Assembly.GetExecutingAssembly(),
            ScriptFilter = script => script.Contains("nonexistent.sql") // No scripts match
        };

        // Act
        await service.ApplySchemaChangesAsync(ConnectionString);

        // Assert
        var appliedScripts = GetAppliedScripts();
        appliedScripts.Should().BeEmpty();

        // Should still log success with 0 scripts
        logger.Collector.GetSnapshot().Should().Contain(log =>
            log.Level == LogLevel.Information &&
            log.Message!.Contains("Database migration completed successfully") &&
            log.Message!.Contains("Scripts applied: 0"));
    }

    /// <summary>
    /// Runs the happy-path of the migration service to ensure it installs required extensions.
    /// Do not test the extensions themselves, just that they are installed.
    /// </summary>
    [Fact]
    public async Task ApplySchemaChangesAsync_DefaultSet_InstallsExtensions()
    {
        // Arrange
        var logger = new FakeLogger<MigrationService>();
        var service = new MigrationService(logger);

        // Act
        await service.ApplySchemaChangesAsync(ConnectionString);
        string[] extensions = [.. Query("SELECT extname FROM pg_extension").Select(row => (string)row["extname"]!)];

        // Assert
        extensions.Should().Contain("citext").And.Contain("uuid-ossp");
    }

    /// <summary>
    /// Tests that the script filter correctly identifies SQL migration scripts.
    /// </summary>
    [Fact]
    public void ScriptFilter_ShouldFilterScriptsCorrectly()
    {
        // Arrange
        var logger = new FakeLogger<MigrationService>();
        var service = new TestMigrationService(logger);
        string[] testScripts = [
            "README.md",
            "001-test1.sql",
            "002-test2.sql",
            "SomeOtherFile.txt"
        ];

        // Act
        string[] filteredScripts = [.. testScripts.Where(service.ScriptFilter)];


        // Assert
        filteredScripts.Should().HaveCount(2).And.ContainInOrder([
            "001-test1.sql",
            "002-test2.sql"
        ]);
    }
}
