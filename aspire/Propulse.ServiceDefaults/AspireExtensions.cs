using Azure.Monitor.OpenTelemetry.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ServiceDiscovery;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace Microsoft.Extensions.Hosting;

/// <summary>
/// Adds common .NET Aspire services: service discovery, resilience, health checks, and OpenTelemetry.
/// This project should be referenced by each service project in your solution.
/// </summary>
/// <remarks>
/// To learn more about using this project, see https://aka.ms/dotnet/aspire/service-defaults
/// </remarks>
public static class AspireExtensions
{
    private const string HealthEndpointPath = "/health";
    private const string AlivenessEndpointPath = "/alive";

    /// <summary>
    /// Adds the default service configurations for .NET Aspire applications including OpenTelemetry,
    /// health checks, service discovery, and HTTP client resilience.
    /// </summary>
    /// <typeparam name="TBuilder">The type of the host application builder.</typeparam>
    /// <param name="builder">The host application builder to configure.</param>
    /// <returns>The configured host application builder.</returns>
    /// <remarks>
    /// This method configures:
    /// - OpenTelemetry for observability (metrics, tracing, and logging)
    /// - Default health checks for application monitoring
    /// - Service discovery for microservices communication
    /// - HTTP client defaults with resilience and service discovery
    /// </remarks>
    public static TBuilder AddServiceDefaults<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.ConfigureOpenTelemetry();
        builder.AddDefaultHealthChecks();
        builder.Services.AddServiceDiscovery();

        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            http.AddStandardResilienceHandler();
            http.AddServiceDiscovery();
        });

        builder.Services.Configure<ServiceDiscoveryOptions>(options =>
        {
            options.AllowedSchemes = ["https"];
        });

        return builder;
    }

    /// <summary>
    /// Configures OpenTelemetry for the application with metrics, tracing, and logging instrumentation.
    /// </summary>
    /// <typeparam name="TBuilder">The type of the host application builder.</typeparam>
    /// <param name="builder">The host application builder to configure.</param>
    /// <returns>The configured host application builder.</returns>
    /// <remarks>
    /// This method sets up:
    /// - Logging with formatted messages and scopes
    /// - Metrics for ASP.NET Core, HTTP clients, and runtime
    /// - Tracing for ASP.NET Core and HTTP clients (excludes health check endpoints)
    /// - OpenTelemetry exporters based on configuration
    /// </remarks>
    public static TBuilder ConfigureOpenTelemetry<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });

        builder.Services
            .AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                metrics.AddAspNetCoreInstrumentation();
                metrics.AddHttpClientInstrumentation();
                metrics.AddRuntimeInstrumentation();
            })
            .WithTracing(tracing =>
            {
                tracing.AddSource(builder.Environment.ApplicationName);
                tracing.AddAspNetCoreInstrumentation(tracing =>
                {
                    tracing.Filter = ExcludeHealthCheckEndpoints;
                });
                tracing.AddHttpClientInstrumentation();
            })
            .AddOtlpExporter(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"])
            .AddAzureMonitorExporter(builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]);

        return builder;
    }

    /// <summary>
    /// A helper method to filter out health check endpoints from OpenTelemetry tracing.
    /// </summary>
    private static bool ExcludeHealthCheckEndpoints(HttpContext context)
    {
        return !context.Request.Path.StartsWithSegments(HealthEndpointPath)
            && !context.Request.Path.StartsWithSegments(AlivenessEndpointPath);
    }

    /// <summary>
    /// Adds the OTLP (OpenTelemetry Protocol) exporter to the OpenTelemetry builder if an endpoint is configured.
    /// </summary>
    /// <param name="builder">The OpenTelemetry builder to configure.</param>
    /// <param name="endpoint">The OTLP endpoint URL. If null or empty, the exporter will not be added.</param>
    /// <returns>The configured OpenTelemetry builder.</returns>
    /// <remarks>
    /// This method checks if the provided endpoint is not null or whitespace before adding the OTLP exporter.
    /// The OTLP exporter is used to send telemetry data to OpenTelemetry-compatible backends.
    /// </remarks>
    private static OpenTelemetryBuilder AddOtlpExporter(this OpenTelemetryBuilder builder, string? endpoint)
    {
        if (!string.IsNullOrWhiteSpace(endpoint))
        {
            builder.UseOtlpExporter();
        }

        return builder;
    }

    /// <summary>
    /// Adds the Azure Monitor exporter to the OpenTelemetry builder if a connection string is configured.
    /// </summary>
    /// <param name="builder">The OpenTelemetry builder to configure.</param>
    /// <param name="connectionString">The Azure Monitor connection string. If null or empty, the exporter will not be added.</param>
    /// <returns>The configured OpenTelemetry builder.</returns>
    /// <remarks>
    /// This method checks if the provided connection string is not null or whitespace before adding the Azure Monitor exporter.
    /// The Azure Monitor exporter is used to send telemetry data to Azure Application Insights.
    /// Requires the Azure.Monitor.OpenTelemetry.AspNetCore NuGet package.
    /// </remarks>
    private static OpenTelemetryBuilder AddAzureMonitorExporter(this OpenTelemetryBuilder builder, string? connectionString)
    {
        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            builder.UseAzureMonitor();
        }

        return builder;
    }

    /// <summary>
    /// Adds default health checks to the application for monitoring application health and liveness.
    /// </summary>
    /// <typeparam name="TBuilder">The type of the host application builder.</typeparam>
    /// <param name="builder">The host application builder to configure.</param>
    /// <returns>The configured host application builder.</returns>
    /// <remarks>
    /// This method adds a basic "self" health check that always returns healthy status,
    /// tagged with "live" for liveness probes. Additional health checks can be added
    /// by calling AddHealthChecks() on the builder's services.
    /// </remarks>
    public static TBuilder AddDefaultHealthChecks<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.Services.AddHealthChecks()
            // Add a default liveness check to ensure app is responsive
            .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

        return builder;
    }

    /// <summary>
    /// Maps the default health check endpoints for the application.
    /// </summary>
    /// <param name="app">The web application to configure.</param>
    /// <returns>The configured web application.</returns>
    /// <remarks>
    /// This method maps two health check endpoints in development environments only:
    /// - /health: All health checks must pass for the app to be considered ready
    /// - /alive: Only health checks tagged with "live" must pass for liveness probes
    /// 
    /// Security note: Adding health checks endpoints in non-development environments 
    /// has security implications. See https://aka.ms/dotnet/aspire/healthchecks for details.
    /// </remarks>
    public static WebApplication MapDefaultEndpoints(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.MapHealthChecks(HealthEndpointPath);
            app.MapHealthChecks(AlivenessEndpointPath, new HealthCheckOptions
            {
                Predicate = r => r.Tags.Contains("live")
            });
        }

        return app;
    }
}
