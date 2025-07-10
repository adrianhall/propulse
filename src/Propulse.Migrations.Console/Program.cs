using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Propulse.Migrations;
using System.CommandLine;

namespace Propulse.Migrations.Console;

/// <summary>
/// Console application for applying database migrations using the Propulse migration service.
/// </summary>
/// <remarks>
/// This application provides a command-line interface for running database migrations.
/// It supports both command-line arguments and environment variables for configuration.
/// </remarks>
public class Program
{
    /// <summary>
    /// Exit code for successful migration execution.
    /// </summary>
    public const int ExitCodeSuccess = 0;

    /// <summary>
    /// Exit code for invalid arguments or configuration errors.
    /// </summary>
    public const int ExitCodeInvalidArguments = 1;

    /// <summary>
    /// Exit code for migration execution failures.
    /// </summary>
    public const int ExitCodeMigrationFailed = 2;

    /// <summary>
    /// Exit code for unexpected errors.
    /// </summary>
    public const int ExitCodeUnexpectedError = 3;

    /// <summary>
    /// Main entry point for the migration console application.
    /// </summary>
    /// <param name="args">Command-line arguments.</param>
    /// <returns>Exit code indicating success or failure.</returns>
    public static async Task<int> Main(string[] args)
    {
        try
        {
            var rootCommand = CreateRootCommand();
            var parseResult = rootCommand.Parse(args);
            return await parseResult.InvokeAsync();
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"Unexpected error: {ex.Message}");
            return ExitCodeUnexpectedError;
        }
    }

    /// <summary>
    /// Creates the root command with all options and handlers.
    /// </summary>
    /// <returns>The configured root command.</returns>
    private static RootCommand CreateRootCommand()
    {
        Option<string?> connectionStringOption = new("--connection-string", [ "-c" ])
        {
            Description = "The database connection string to use for migrations"
        };

        Option<bool> noLoggingOption = new("--no-logging")
        {
            Description = "Disable logging output for testing purposes"
        };

        var rootCommand = new RootCommand("Propulse Database Migration Tool")
        {
            connectionStringOption,
            noLoggingOption
        };

        rootCommand.SetAction(async (parseResult, cancellationToken) =>
        {
            var connectionString = parseResult.GetValue(connectionStringOption);
            var noLogging = parseResult.GetValue(noLoggingOption);
            var exitCode = await ExecuteMigrationAsync(connectionString, noLogging);
            return exitCode;
        });

        return rootCommand;
    }

    /// <summary>
    /// Executes the database migration with the provided configuration.
    /// </summary>
    /// <param name="connectionStringArgument">Connection string from command line argument.</param>
    /// <param name="noLogging">Whether to disable logging.</param>
    /// <returns>Exit code indicating success or failure.</returns>
    private static async Task<int> ExecuteMigrationAsync(string? connectionStringArgument, bool noLogging)
    {
        try
        {
            // Build configuration from environment variables and command line
            var configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .Build();

            // Determine the connection string to use
            var connectionString = GetConnectionString(connectionStringArgument, configuration);

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                if (!noLogging)
                {
                    System.Console.WriteLine("Error: No connection string provided.");
                    System.Console.WriteLine("Use -c/--connection-string argument or set ConnectionStrings__DefaultConnection environment variable.");
                }
                return ExitCodeInvalidArguments;
            }

            // Configure services
            var services = new ServiceCollection();
            ConfigureServices(services, noLogging);

            using var serviceProvider = services.BuildServiceProvider();
            var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
            var migrationService = serviceProvider.GetRequiredService<IMigrationService>();

            // Execute migration
            if (!noLogging)
            {
                logger.LogInformation("Starting database migration...");
            }

            await migrationService.ApplySchemaChangesAsync(connectionString);

            if (!noLogging)
            {
                logger.LogInformation("Database migration completed successfully.");
            }

            return ExitCodeSuccess;
        }
        catch (ArgumentException ex)
        {
            if (!noLogging)
            {
                System.Console.WriteLine($"Invalid argument: {ex.Message}");
            }
            return ExitCodeInvalidArguments;
        }
        catch (InvalidOperationException ex)
        {
            if (!noLogging)
            {
                System.Console.WriteLine($"Migration failed: {ex.Message}");
            }
            return ExitCodeMigrationFailed;
        }
        catch (Exception ex)
        {
            if (!noLogging)
            {
                System.Console.WriteLine($"Unexpected error during migration: {ex.Message}");
            }
            return ExitCodeUnexpectedError;
        }
    }

    /// <summary>
    /// Determines the connection string to use from command line arguments or environment variables.
    /// </summary>
    /// <param name="connectionStringArgument">Connection string from command line argument.</param>
    /// <param name="configuration">Configuration containing environment variables.</param>
    /// <returns>The connection string to use, or null if none provided.</returns>
    private static string? GetConnectionString(string? connectionStringArgument, IConfiguration configuration)
    {
        // Command line argument takes precedence
        if (!string.IsNullOrWhiteSpace(connectionStringArgument))
        {
            return connectionStringArgument;
        }

        // Fall back to environment variable
        return configuration.GetConnectionString("DefaultConnection");
    }

    /// <summary>
    /// Configures the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="noLogging">Whether to disable logging.</param>
    private static void ConfigureServices(IServiceCollection services, bool noLogging)
    {
        // Configure logging
        if (!noLogging)
        {
            services.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Information);
            });
        }
        else
        {
            services.AddLogging(builder =>
            {
                builder.SetMinimumLevel(LogLevel.None);
            });
        }

        // Register migration service
        services.AddScoped<IMigrationService, MigrationService>();
    }
}
