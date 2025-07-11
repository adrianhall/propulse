using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Diagnostics;

namespace Propulse.Web.Tests.Helpers;

/// <summary>
/// Provides a test fixture for integration testing of the Propulse web application.
/// Manages the lifecycle of a test web server and database for reliable, isolated test execution.
/// </summary>
/// <remarks>
/// This fixture class implements the <see cref="IAsyncLifetime"/> interface to provide proper
/// setup and teardown of web application and database resources for xUnit test collections.
/// It ensures that each test run starts with a clean, fully configured web application
/// and database schema.
/// 
/// Key features:
/// <list type="bullet">
/// <item>In-memory web application factory using <see cref="WebApplicationFactory{TEntryPoint}"/></item>
/// <item>Integrated PostgreSQL test database using <see cref="DatabaseFixture"/></item>
/// <item>Automatic configuration of connection strings for the test environment</item>
/// <item>HTTP client factory with configurable options (cookies, redirects)</item>
/// <item>Access to the application's service provider for dependency resolution</item>
/// <item>Conditional logging based on debugger attachment</item>
/// </list>
/// 
/// The fixture is designed to be used with xUnit's collection fixtures to share the same
/// web application and database instance across multiple test classes while ensuring
/// proper isolation between test runs.
/// 
/// Web application configuration:
/// <list type="bullet">
/// <item>Environment: Development (for consistent test behavior)</item>
/// <item>Database: PostgreSQL container via DatabaseFixture</item>
/// <item>Logging: Disabled unless debugger is attached (reduces test noise)</item>
/// <item>Connection strings: Automatically configured for test database</item>
/// </list>
/// </remarks>
/// <example>
/// Using the fixture in a test collection:
/// <code>
/// [CollectionDefinition("WebService")]
/// public class WebServiceCollection : ICollectionFixture&lt;WebServiceFixture&gt;
/// {
///     // This class has no code, and is never created. Its purpose is simply
///     // to be the place to apply [CollectionDefinition] and all the
///     // ICollectionFixture&lt;&gt; interfaces.
/// }
/// 
/// [Collection("WebService")]
/// public class HomeControllerTests
/// {
///     private readonly WebServiceFixture _fixture;
/// 
///     public HomeControllerTests(WebServiceFixture fixture)
///     {
///         _fixture = fixture;
///     }
/// 
///     [Fact]
///     public async Task Get_ReturnsSuccessStatusCode()
///     {
///         // Arrange
///         using var client = _fixture.CreateClient();
/// 
///         // Act
///         var response = await client.GetAsync("/");
/// 
///         // Assert
///         response.Should().BeSuccessful();
///     }
/// 
///     [Fact]
///     public async Task CanAccessServices()
///     {
///         // Arrange
///         var userManager = _fixture.Services.GetRequiredService&lt;UserManager&lt;IdentityUser&gt;&gt;();
/// 
///         // Act & Assert
///         userManager.Should().NotBeNull();
///     }
/// }
/// </code>
/// </example>
/// <seealso cref="DatabaseFixture"/>
/// <seealso cref="WebApplicationFactory{TEntryPoint}"/>
/// <seealso cref="IAsyncLifetime"/>
public class WebServiceFixture : IAsyncLifetime
{
    /// <summary>
    /// Lazy-initialized instance of the web service factory to ensure single creation.
    /// </summary>
    /// <remarks>
    /// The lazy initialization pattern ensures that the web application factory is created
    /// only when first accessed, and that the same instance is reused throughout the
    /// fixture's lifetime. This improves performance and ensures consistency across tests.
    /// </remarks>
    private readonly Lazy<WebServiceFactory> lazyInstance;

    /// <summary>
    /// Initializes a new instance of the <see cref="WebServiceFixture"/> class.
    /// </summary>
    /// <remarks>
    /// The constructor sets up the lazy initialization for the web service factory,
    /// which will be created when first accessed through the <see cref="WebService"/> property.
    /// The actual web application and database initialization occurs in <see cref="InitializeAsync"/>.
    /// </remarks>
    public WebServiceFixture()
    {
        lazyInstance = new Lazy<WebServiceFactory>(CreateFactory);
    }

    /// <summary>
    /// Disposes the web service fixture by cleaning up the database resources.
    /// This method is called once after all tests in the collection have completed.
    /// </summary>
    /// <returns>A task representing the asynchronous disposal operation.</returns>
    /// <remarks>
    /// The disposal process delegates to the underlying <see cref="DatabaseFixture"/>
    /// to ensure proper cleanup of the PostgreSQL container and associated resources.
    /// The web application factory is automatically disposed by the framework.
    /// </remarks>
    public Task DisposeAsync()
        => Database.DisposeAsync();

    /// <summary>
    /// Initializes the web service fixture by setting up the database.
    /// This method is called once before any tests in the collection are executed.
    /// </summary>
    /// <returns>A task representing the asynchronous initialization operation.</returns>
    /// <remarks>
    /// The initialization process delegates to the underlying <see cref="DatabaseFixture"/>
    /// to start the PostgreSQL container and apply schema migrations. The web application
    /// factory is created lazily when first accessed.
    /// </remarks>
    public Task InitializeAsync()
        => Database.InitializeAsync();

    /// <summary>
    /// Creates and configures the web application factory for testing.
    /// </summary>
    /// <returns>A configured <see cref="WebServiceFactory"/> instance.</returns>
    /// <remarks>
    /// This method is called by the lazy initialization pattern when the web service
    /// is first accessed. It configures the web application with the test database
    /// connection string from the <see cref="DatabaseFixture"/>.
    /// </remarks>
    private WebServiceFactory CreateFactory()
    {
        var factory = new WebServiceFactory(builder =>
        {
            builder.UseSetting("ConnectionStrings:SecurityConnection", Database.ConnectionString);
        });
        return factory;
    }

    /// <summary>
    /// Gets the web application factory instance for creating test clients and accessing services.
    /// </summary>
    /// <value>
    /// A <see cref="WebServiceFactory"/> instance configured for testing with the test database.
    /// </value>
    /// <remarks>
    /// This property provides access to the underlying web application factory, which can be
    /// used to create HTTP clients for testing web endpoints or to access the application's
    /// service provider for dependency resolution in tests.
    /// 
    /// The factory is lazily initialized on first access to improve performance and ensure
    /// that database setup has completed before the web application is configured.
    /// </remarks>
    internal WebServiceFactory WebService { get => lazyInstance.Value; }

    /// <summary>
    /// Gets the database fixture instance that manages the PostgreSQL test container.
    /// </summary>
    /// <value>
    /// A <see cref="DatabaseFixture"/> instance that provides access to the test database.
    /// </value>
    /// <remarks>
    /// This property provides access to the underlying database fixture, which manages
    /// the PostgreSQL TestContainer and provides helper methods for database operations.
    /// The database is automatically configured and migrated during fixture initialization.
    /// </remarks>
    internal DatabaseFixture Database { get; } = new();

    /// <summary>
    /// Creates an HTTP client configured for testing the web application.
    /// </summary>
    /// <param name="configureClient">Optional action to configure client options such as base address, timeouts, or authentication.</param>
    /// <returns>An <see cref="HttpClient"/> instance configured for testing.</returns>
    /// <remarks>
    /// This method creates an HTTP client that can be used to send requests to the test
    /// web application. The client is pre-configured with common testing options:
    /// <list type="bullet">
    /// <item><strong>AllowAutoRedirect:</strong> Automatically follows HTTP redirects</item>
    /// <item><strong>HandleCookies:</strong> Maintains cookies across requests for session testing</item>
    /// </list>
    /// 
    /// Additional configuration can be applied through the optional <paramref name="configureClient"/>
    /// parameter, allowing tests to customize behavior such as authentication headers,
    /// custom timeouts, or specific base addresses.
    /// </remarks>
    /// <example>
    /// Creating a basic client:
    /// <code>
    /// using var client = _fixture.CreateClient();
    /// var response = await client.GetAsync("/api/health");
    /// </code>
    /// 
    /// Creating a client with custom configuration:
    /// <code>
    /// using var client = _fixture.CreateClient(options =>
    /// {
    ///     options.AllowAutoRedirect = false;
    ///     options.BaseAddress = new Uri("https://localhost/api/");
    /// });
    /// </code>
    /// </example>
    internal HttpClient CreateClient(Action<WebApplicationFactoryClientOptions>? configureClient = null)
    {
        var options = new WebApplicationFactoryClientOptions()
        {
            AllowAutoRedirect = true,
            HandleCookies = true
        };
        configureClient?.Invoke(options);
        return WebService.CreateClient(options);
    }

    /// <summary>
    /// Gets the service provider from the web application for dependency resolution in tests.
    /// </summary>
    /// <value>
    /// An <see cref="IServiceProvider"/> instance from the configured web application.
    /// </value>
    /// <remarks>
    /// This property provides access to the web application's dependency injection container,
    /// allowing tests to resolve services directly for more granular testing scenarios.
    /// This is particularly useful for testing service behaviors, accessing database contexts,
    /// or verifying service registrations.
    /// 
    /// The service provider reflects the actual configuration of the web application,
    /// including any test-specific overrides applied through the web application factory.
    /// </remarks>
    /// <example>
    /// Accessing services in tests:
    /// <code>
    /// var userManager = _fixture.Services.GetRequiredService&lt;UserManager&lt;IdentityUser&gt;&gt;();
    /// var dbContext = _fixture.Services.GetRequiredService&lt;SecurityDbContext&gt;();
    /// var emailSender = _fixture.Services.GetRequiredService&lt;IPropulseEmailSender&gt;();
    /// </code>
    /// </example>
    internal IServiceProvider Services { get => WebService.Services; }

    /// <summary>
    /// A customized web application factory for integration testing that configures the application
    /// with test-specific settings and environment configuration.
    /// </summary>
    /// <param name="configure">Optional action to configure the web host builder with additional settings.</param>
    /// <remarks>
    /// This internal factory class extends <see cref="WebApplicationFactory{TEntryPoint}"/> to provide
    /// a test-specific configuration of the Propulse web application. It automatically configures
    /// the application for testing with appropriate logging levels and environment settings.
    /// 
    /// Key configuration features:
    /// <list type="bullet">
    /// <item>Sets environment to "Development" for consistent test behavior</item>
    /// <item>Disables logging when not debugging to reduce test output noise</item>
    /// <item>Allows custom configuration through the constructor parameter</item>
    /// <item>Inherits all standard web application factory capabilities</item>
    /// </list>
    /// 
    /// The factory uses the application's Program class as the entry point, ensuring that
    /// the test environment closely mirrors the production configuration while allowing
    /// for test-specific overrides.
    /// </remarks>
    /// <example>
    /// The factory is typically used internally by the fixture:
    /// <code>
    /// var factory = new WebServiceFactory(builder =>
    /// {
    ///     builder.UseSetting("ConnectionStrings:SecurityConnection", testConnectionString);
    ///     builder.UseSetting("CustomSetting", "TestValue");
    /// });
    /// </code>
    /// </example>
    internal class WebServiceFactory(Action<IWebHostBuilder>? configure = null) : WebApplicationFactory<Propulse.Web.Program>()
    {
        /// <summary>
        /// Configures the web host for testing with appropriate environment and logging settings.
        /// </summary>
        /// <param name="builder">The web host builder to configure.</param>
        /// <remarks>
        /// This method overrides the base configuration to provide test-specific settings:
        /// <list type="bullet">
        /// <item>Sets the environment to "Development" for consistent test behavior</item>
        /// <item>Applies any custom configuration provided through the constructor</item>
        /// <item>Disables logging unless a debugger is attached to reduce test output noise</item>
        /// <item>Calls the base implementation to ensure standard factory behavior</item>
        /// </list>
        /// 
        /// The logging configuration helps maintain clean test output while still providing
        /// diagnostic information when needed during debugging sessions.
        /// </remarks>
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");

            configure?.Invoke(builder);

            // Ensure logging doesn't happen unless we are debugging.
            if (!Debugger.IsAttached)
            {
                builder.UseSetting("Logging:LogLevel:Default", "None");
                builder.UseSetting("Logging:LogLevel:Microsoft.AspNetCore", "None");
                builder.UseSetting("Logging:LogLevel:Propulse", "None");
            }

            base.ConfigureWebHost(builder);
        }
    }
}
