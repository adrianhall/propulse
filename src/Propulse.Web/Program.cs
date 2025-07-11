using Microsoft.AspNetCore.Identity;
using Propulse.Web.Entities;
using Propulse.Web.Persistence;
using System.Diagnostics.CodeAnalysis;

namespace Propulse.Web;

/// <summary>
/// Entry point for the Propulse web application.
/// Configures and starts the ASP.NET Core web host, including service registration, middleware pipeline, and environment-specific settings.
/// </summary>
/// <remarks>
/// This class follows Clean Architecture and Vertical Slice Architecture principles.
/// It uses strongly-typed configuration, structured logging, and dependency injection.
/// For more information, see <see href="https://learn.microsoft.com/en-us/aspnet/core/fundamentals/host/web-host">ASP.NET Core Web Host</see>.
/// </remarks>
/// <example>
/// <code>
/// // To run the application:
/// // pwsh: dotnet run --project src/Propulse.Web/Propulse.Web.csproj
/// </code>
/// </example>
public class Program
{
    /// <summary>
    /// Main entry point for the Propulse web application.
    /// Configures services, middleware, endpoints, and starts the web host.
    /// </summary>
    /// <param name="args">Command-line arguments for the application.</param>
    /// <remarks>
    /// This method is excluded from code coverage as it is the application entry point.
    /// </remarks>
    /// <exception cref="System.Exception">Thrown if the web host fails to start.</exception>
    [ExcludeFromCodeCoverage(Justification = "This is the entry point of the application and is not covered by unit tests.")]
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        ConfigureServices(builder);
        
        var app = builder.Build();

        // Configure the HTTP request pipeline.
        ConfigureHttpRequestPipeline(app);

        // Map endpoints
        MapEndpoints(app);

        app.MapGet("/", () => "Hello World!");

        app.Run();
    }

    /// <summary>
    /// Configures services for the Propulse web application.
    /// </summary>
    /// <param name="builder">The <see cref="WebApplicationBuilder"/> used to register services and configuration.</param>
    /// <remarks>
    /// Follows Clean Architecture and Aspire service defaults. Identity options are bound from configuration.
    /// </remarks>
    /// <example>
    /// <code>
    /// var builder = WebApplication.CreateBuilder(args);
    /// Program.ConfigureServices(builder);
    /// </code>
    /// </example>
    internal static void ConfigureServices(WebApplicationBuilder builder)
    {
        // .NET Aspire Service Defaults (health checks, OpenTelemetry, etc.)
        builder.AddServiceDefaults();

        // HTTP Context Accessor for async access to the current HTTP context
        builder.Services.AddHttpContextAccessor();

        // Security / Identity database context
        builder.AddNpgsqlDbContext<SecurityDbContext>(connectionName: "SecurityConnection");

        // ASP.NET Identity Configuration, using Identity:Options to configure settings
        builder.Services
            .AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                // Configure identity with the Identity:Options section
                builder.Configuration.GetSection("Identity:Options").Bind(options);

                // Required overrides
                options.SignIn.RequireConfirmedAccount = true;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<SecurityDbContext>()
            .AddDefaultTokenProviders();

        // ASP.NET Core MVC
        builder.Services.AddControllersWithViews();
    }

    /// <summary>
    /// Configures the HTTP request pipeline for the Propulse web application.
    /// </summary>
    /// <param name="app">The <see cref="WebApplication"/> instance to configure middleware.</param>
    /// <remarks>
    /// Middleware is configured in the recommended order for ASP.NET Core applications.
    /// </remarks>
    /// <example>
    /// <code>
    /// var app = builder.Build();
    /// Program.ConfigureHttpRequestPipeline(app);
    /// </code>
    /// </example>
    internal static void ConfigureHttpRequestPipeline(WebApplication app)
    {
        // Configure an error handler based on the environment
        app.UseErrorHandler("/Home/Error");

        // Use HTTPS redirection
        app.UseHttpsRedirection();

        // Use static files
        app.UseStaticFiles();

        // Use routing
        app.UseRouting();
    }

    /// <summary>
    /// Maps endpoints for the Propulse web application.
    /// </summary>
    /// <param name="app">The <see cref="WebApplication"/> instance to map endpoints on.</param>
    /// <remarks>
    /// Endpoints are mapped to support modular areas and development diagnostics.
    /// </remarks>
    /// <example>
    /// <code>
    /// Program.MapEndpoints(app);
    /// </code>
    /// </example>
    internal static void MapEndpoints(WebApplication app)
    {
        // Map the .NET Aspire health check endpoints
        app.MapDefaultEndpoints();

        // Map the default Areas route
        app.MapControllerRoute(
            name: "areas",
            pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

        // Map the default controller route
        app.MapControllerRoute(
            name: "default", 
            pattern: "{controller=Home}/{action=Index}/{id?}");

        // Map the Developer Endpoints when in development mode
        app.MapDeveloperEndpoints();
    }
}
