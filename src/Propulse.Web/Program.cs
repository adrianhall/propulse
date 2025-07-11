using Propulse.Persistence;
using System.Diagnostics.CodeAnalysis;

namespace Propulse.Web;

public class Program
{
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

    internal static void ConfigureServices(WebApplicationBuilder builder)
    {
        // .NET Aspire Service Defaults (health checks, OpenTelemetry, etc.)
        builder.AddServiceDefaults();

        // HTTP Context Accessor for async access to the current HTTP context
        builder.Services.AddHttpContextAccessor();

        // Security / Identity database context
        builder.AddNpgsqlDbContext<SecurityDbContext>(connectionName: "SecurityConnection");

        // ASP.NET Core MVC
        builder.Services.AddControllersWithViews();
    }

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
