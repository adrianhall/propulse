using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;

namespace Propulse.Web.Extensions;

/// <summary>
/// Implements developer endpoints for debugging and development purposes.
/// </summary>
/// <remarks>
/// This class provides endpoint handlers that expose diagnostic information about the application
/// state, user authentication, and routing configuration. These endpoints are intended for development
/// and debugging scenarios only and should never be exposed in production environments.
/// 
/// <para>
/// All methods in this class are marked as internal to prevent direct usage outside of the
/// <see cref="StartupExtensions.MapDeveloperEndpoints"/> method, which handles the conditional
/// registration based on the hosting environment.
/// </para>
/// </remarks>
[ExcludeFromCodeCoverage(Justification = "This class is used in development and debugging scenarios, not in production.")]
public static class DeveloperEndpoints
{
    /// <summary>
    /// A request delegate handler that returns the current user information as a JSON object.
    /// If the user is not authenticated, it returns a 401 Unauthorized response. If the user
    /// is authenticated, it returns a 200 OK response with the user information, including
    /// user ID, claims, and roles.
    /// </summary>
    /// <param name="contextAccessor">The HTTP context accessor to retrieve the current request context.</param>
    /// <returns>
    /// An <see cref="IResult"/> containing either:
    /// <list type="bullet">
    /// <item>401 Unauthorized if no user is authenticated</item>
    /// <item>200 OK with user information JSON if user is authenticated</item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// The returned user information includes:
    /// <list type="bullet">
    /// <item><strong>IsAuthenticated:</strong> Boolean indicating authentication status</item>
    /// <item><strong>UserId:</strong> The user's unique identifier from claims</item>
    /// <item><strong>UserName:</strong> The user's name identifier</item>
    /// <item><strong>Email:</strong> The user's email address if available</item>
    /// <item><strong>Claims:</strong> Array of all user claims with type and value</item>
    /// <item><strong>Roles:</strong> Array of role names assigned to the user</item>
    /// </list>
    /// 
    /// <para>
    /// This endpoint is particularly useful for debugging authentication issues, verifying
    /// claim assignments, and understanding the user context during development.
    /// </para>
    /// </remarks>
    /// <example>
    /// Example response for an authenticated user:
    /// <code>
    /// {
    ///   "isAuthenticated": true,
    ///   "userId": "123e4567-e89b-12d3-a456-426614174000",
    ///   "userName": "john.doe@example.com",
    ///   "email": "john.doe@example.com",
    ///   "claims": [
    ///     { "type": "sub", "value": "123e4567-e89b-12d3-a456-426614174000" },
    ///     { "type": "email", "value": "john.doe@example.com" }
    ///   ],
    ///   "roles": ["User", "Administrator"]
    /// }
    /// </code>
    /// </example>
    internal static IResult GetCurrentUser(IHttpContextAccessor contextAccessor)
    {
        var context = contextAccessor.HttpContext;
        if (context is null)
        {
            return Results.Problem("HttpContext is not available", statusCode: 500);
        }

        var user = context.User;
        if (user?.Identity?.IsAuthenticated != true)
        {
            return Results.Unauthorized();
        }

        // Extract user information from claims
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                  ?? user.FindFirst("sub")?.Value 
                  ?? user.FindFirst("id")?.Value;

        var userName = user.FindFirst(ClaimTypes.Name)?.Value 
                    ?? user.FindFirst("name")?.Value 
                    ?? user.Identity.Name;

        var email = user.FindFirst(ClaimTypes.Email)?.Value 
                 ?? user.FindFirst("email")?.Value;

        // Get all claims
        var claims = user.Claims.Select(c => new { type = c.Type, value = c.Value }).ToArray();

        // Get roles
        var roles = user.FindAll(ClaimTypes.Role)
                       .Select(c => c.Value)
                       .Union(user.FindAll("role").Select(c => c.Value))
                       .Distinct()
                       .ToArray();

        var userInfo = new
        {
            isAuthenticated = true,
            userId,
            userName,
            email,
            claims,
            roles
        };

        return Results.Ok(userInfo);
    }

    /// <summary>
    /// A request delegate handler that returns the current route map as a JSON object. The
    /// JSON object contains the route names, patterns, areas, controllers, actions, and HTTP
    /// verb information for each route. This is useful for debugging and understanding the
    /// internal routing structure of the application.
    /// </summary>
    /// <param name="contextAccessor">The HTTP context accessor to retrieve the current request context.</param>
    /// <returns>
    /// An <see cref="IResult"/> containing a 200 OK response with the route map JSON object.
    /// </returns>
    /// <remarks>
    /// The returned route map includes detailed information about all registered routes:
    /// <list type="bullet">
    /// <item><strong>Name:</strong> The route name if specified</item>
    /// <item><strong>Pattern:</strong> The route pattern template</item>
    /// <item><strong>Area:</strong> The area name for area-based routes</item>
    /// <item><strong>Controller:</strong> The controller name for MVC routes</item>
    /// <item><strong>Action:</strong> The action method name for MVC routes</item>
    /// <item><strong>HttpMethods:</strong> Array of supported HTTP verbs (GET, POST, etc.)</item>
    /// <item><strong>Defaults:</strong> Default route values</item>
    /// <item><strong>Constraints:</strong> Route constraints applied</item>
    /// </list>
    /// 
    /// <para>
    /// This endpoint is invaluable for debugging routing issues, understanding how URLs map
    /// to controllers and actions, and verifying that routes are registered correctly during
    /// application development.
    /// </para>
    /// </remarks>
    /// <example>
    /// Example response showing route information:
    /// <code>
    /// {
    ///   "routes": [
    ///     {
    ///       "name": "default",
    ///       "pattern": "{controller=Home}/{action=Index}/{id?}",
    ///       "area": null,
    ///       "controller": "Home",
    ///       "action": "Index",
    ///       "httpMethods": ["GET", "POST"],
    ///       "defaults": { "controller": "Home", "action": "Index" },
    ///       "constraints": {}
    ///     },
    ///     {
    ///       "name": "areas",
    ///       "pattern": "{area:exists}/{controller=Home}/{action=Index}/{id?}",
    ///       "area": null,
    ///       "controller": "Home",
    ///       "action": "Index",
    ///       "httpMethods": ["GET", "POST"],
    ///       "defaults": { "area": "", "controller": "Home", "action": "Index" },
    ///       "constraints": { "area": "exists" }
    ///     }
    ///   ],
    ///   "totalRoutes": 2
    /// }
    /// </code>
    /// </example>
    internal static IResult GetRouteMap(IHttpContextAccessor contextAccessor)
    {
        var context = contextAccessor.HttpContext;
        if (context is null)
        {
            return Results.Problem("HttpContext is not available", statusCode: 500);
        }

        // Get the endpoint data source which contains all registered routes
        var endpointDataSource = context.RequestServices.GetService<EndpointDataSource>();
        if (endpointDataSource is null)
        {
            return Results.Problem("EndpointDataSource is not available", statusCode: 500);
        }

        var routes = new List<object>();

        foreach (var endpoint in endpointDataSource.Endpoints)
        {
            if (endpoint is not RouteEndpoint routeEndpoint)
            {
                continue;
            }

            // Extract route information
            var routePattern = routeEndpoint.RoutePattern;
            var metadata = endpoint.Metadata;

            // Get HTTP methods
            var httpMethodMetadata = metadata.OfType<HttpMethodMetadata>().FirstOrDefault();
            string[] httpMethods = httpMethodMetadata?.HttpMethods?.ToArray() ?? [];

            // Extract controller and action from metadata or route values
            var controllerName = routePattern.Defaults?.GetValueOrDefault("controller")?.ToString();
            var actionName = routePattern.Defaults?.GetValueOrDefault("action")?.ToString();
            var areaName = routePattern.Defaults?.GetValueOrDefault("area")?.ToString();

            // Get route name
            var routeName = routeEndpoint.DisplayName;

            // Extract constraints
            var constraints = routePattern.ParameterPolicies
                .Where(p => p.Value.Any())
                .ToDictionary(
                    p => p.Key,
                    p => string.Join(", ", p.Value.Select(policy => policy.GetType().Name))
                );

            var routeInfo = new
            {
                name = routeName,
                pattern = routePattern.RawText,
                area = areaName,
                controller = controllerName,
                action = actionName,
                httpMethods,
                defaults = routePattern.Defaults?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value?.ToString()),
                constraints = constraints.Count != 0 ? constraints : null,
                order = routeEndpoint.Order
            };

            routes.Add(routeInfo);
        }

        // Sort routes by order, then by pattern for consistent output
        var sortedRoutes = routes
            .Cast<dynamic>()
            .OrderBy(r => r.order)
            .ThenBy(r => r.pattern)
            .ToArray();

        var routeMap = new
        {
            routes = sortedRoutes,
            totalRoutes = routes.Count
        };

        return Results.Ok(routeMap);
    }
}
