using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Propulse.Web.Entities;
using Propulse.Web.Extensions;
using Propulse.Web.Services;
using System.Diagnostics.CodeAnalysis;

namespace Propulse.Web;

/// <summary>
/// Provides extension methods for configuring startup and development features in the Propulse web application.
/// </summary>
/// <remarks>
/// This static class contains extension methods that simplify the configuration of common application features
/// such as error handling, HSTS, and development-specific endpoints. These methods are designed to be called
/// during application startup to configure the HTTP request pipeline and register developer tools.
/// 
/// <para>
/// The methods in this class follow the ASP.NET Core convention of returning the same type they extend,
/// allowing for method chaining in the application configuration pipeline.
/// </para>
/// 
/// <para>
/// All methods are marked with <see cref="ExcludeFromCodeCoverageAttribute"/> as they primarily contain
/// configuration logic that is difficult to unit test effectively and is typically covered by integration tests.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var app = builder.Build();
/// 
/// // Configure error handling based on environment
/// app.UseErrorHandler("/Home/Error");
/// 
/// // Add developer endpoints in development environment
/// app.MapDeveloperEndpoints();
/// 
/// app.Run();
/// </code>
/// </example>
/// <seealso cref="DeveloperEndpoints"/>
[ExcludeFromCodeCoverage(Justification = "This method is used to configure services and is not covered by unit tests.")]
public static class StartupExtensions
{
    /// <summary>
    /// Configures error handling middleware based on the current hosting environment.
    /// </summary>
    /// <param name="app">The web application to configure error handling for.</param>
    /// <param name="errorPage">The error page route to redirect to in production environments.</param>
    /// <returns>The configured web application instance for method chaining.</returns>
    /// <remarks>
    /// This method applies different error handling strategies based on the hosting environment:
    /// <list type="bullet">
    /// <item>
    /// <strong>Development:</strong> Uses <c>UseDeveloperExceptionPage()</c> to display detailed 
    /// exception information including stack traces, request details, and debugging information.
    /// </item>
    /// <item>
    /// <strong>Production:</strong> Uses <c>UseExceptionHandler(errorPage)</c> to redirect to a 
    /// user-friendly error page and enables HSTS (HTTP Strict Transport Security) for enhanced security.
    /// </item>
    /// </list>
    /// 
    /// <para>
    /// The HSTS header is only added in production environments to enforce HTTPS connections
    /// and prevent man-in-the-middle attacks. This follows security best practices by not
    /// enforcing HSTS during development where HTTPS may not be properly configured.
    /// </para>
    /// </remarks>
    /// <param name="app">The <see cref="WebApplication"/> instance to configure.</param>
    /// <param name="errorPage">The route path for the error page (e.g., "/Home/Error").</param>
    /// <returns>The same <see cref="WebApplication"/> instance for method chaining.</returns>
    /// <example>
    /// <code>
    /// // Basic usage with custom error page
    /// app.UseErrorHandler("/Error");
    /// 
    /// // Using with Home controller error action
    /// app.UseErrorHandler("/Home/Error");
    /// 
    /// // Method chaining example
    /// app.UseErrorHandler("/Home/Error")
    ///    .UseHttpsRedirection()
    ///    .UseStaticFiles();
    /// </code>
    /// </example>
    public static WebApplication UseErrorHandler(this WebApplication app, string errorPage)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler(errorPage);
            app.UseHsts();
        }
        return app;
    }

    /// <summary>
    /// Registers a transactional email provider for the application based on the current environment.
    /// In development, registers a no-op email sender; in other environments, throws <see cref="NotImplementedException"/>.
    /// </summary>
    /// <typeparam name="TBuilder">The type of the application builder, implementing <see cref="IHostApplicationBuilder"/>.</typeparam>
    /// <param name="builder">The application builder used to register services.</param>
    /// <returns>The same <typeparamref name="TBuilder"/> instance for method chaining.</returns>
    /// <remarks>
    /// In development, <see cref="NullEmailSender{TUser}"/> is registered as a singleton for <see cref="IEmailSender{ApplicationUser}"/>.
    /// In production or other environments, transactional email services must be implemented and registered appropriately.
    /// </remarks>
    /// <exception cref="NotImplementedException">Thrown if called outside of development environment.</exception>
    /// <example>
    /// <code>
    /// builder.AddTransactionalEmailProvider();
    /// </code>
    /// </example>
    public static TBuilder AddTransactionalEmailProvider<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        if (builder.Environment.IsDevelopment())
        {
            builder.Services.AddSingleton<IEmailSender<ApplicationUser>, NullEmailSender<ApplicationUser>>();
        }
        else
        {
            throw new NotImplementedException("Transactional Email Services are not available");
        }

        return builder;
    }

    /// <summary>
    /// Maps developer-specific endpoints that provide debugging and diagnostic information.
    /// </summary>
    /// <param name="app">The web application to add developer endpoints to.</param>
    /// <returns>The configured web application instance for method chaining.</returns>
    /// <remarks>
    /// This method conditionally registers developer endpoints only when the application is running
    /// in the Development environment. The endpoints are grouped under the "/devapi" route prefix
    /// and provide the following functionality:
    /// 
    /// <list type="bullet">
    /// <item>
    /// <strong>/devapi/currentuser:</strong> Returns current user authentication information including
    /// user ID, claims, and roles. Useful for debugging authentication and authorization issues.
    /// </item>
    /// <item>
    /// <strong>/devapi/routemap:</strong> Returns a comprehensive map of all registered routes in the
    /// application, including route patterns, areas, controllers, actions, and HTTP verbs. Helpful
    /// for understanding the application's routing structure.
    /// </item>
    /// </list>
    /// 
    /// <para>
    /// All developer endpoints are configured to allow anonymous access since they are intended
    /// for development and debugging purposes. These endpoints are automatically excluded from
    /// production builds through environment checking.
    /// </para>
    /// 
    /// <para>
    /// If the application is not running in Development mode, this method returns immediately
    /// without registering any endpoints, ensuring that diagnostic information is not exposed
    /// in production environments.
    /// </para>
    /// </remarks>
    /// <returns>The same <see cref="WebApplication"/> instance for method chaining.</returns>
    /// <example>
    /// <code>
    /// // Basic usage - endpoints only registered in Development
    /// app.MapDeveloperEndpoints();
    /// 
    /// // Method chaining with other endpoint mapping
    /// app.MapDeveloperEndpoints()
    ///    .MapControllers()
    ///    .MapRazorPages();
    /// 
    /// // The following endpoints become available in Development:
    /// // GET /devapi/currentuser - Returns current user information
    /// // GET /devapi/routemap - Returns application route mapping
    /// </code>
    /// </example>
    /// <seealso cref="DeveloperEndpoints.GetCurrentUser"/>
    /// <seealso cref="DeveloperEndpoints.GetRouteMap"/>
    public static WebApplication MapDeveloperEndpoints(this WebApplication app)
    {
        // Short circuit if not in development
        if (!app.Environment.IsDevelopment())
        {
            return app;
        }

        var devAPI = app.MapGroup("/devapi");
        devAPI.MapGet("/currentuser", DeveloperEndpoints.GetCurrentUser).AllowAnonymous();
        devAPI.MapGet("/routemap", DeveloperEndpoints.GetRouteMap).AllowAnonymous();

        return app;
    }
}
