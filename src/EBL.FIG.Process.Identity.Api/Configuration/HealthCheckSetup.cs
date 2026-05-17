using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using System.Text.Json;

namespace EBL.FIG.Process.Identity.Api.Configuration;

/// <summary>
/// Provides extension methods for configuring health check endpoints in an ASP.NET Core application.
/// </summary>
/// <remarks>This class contains static methods to simplify the setup of health check endpoints using the
/// application's request pipeline. It is intended to be used as part of the application's startup
/// configuration.</remarks>
public static class HealthCheckSetup
{
    /// <summary>
    /// Adds a JSON-formatted health check endpoint at the path "/healthz" to the application's request pipeline.
    /// </summary>
    /// <remarks>The endpoint responds with a JSON payload containing the overall health status and details
    /// for each registered health check. The response is suitable for use with monitoring systems and load
    /// balancers.</remarks>
    /// <param name="app">The application builder to configure with the health check endpoint. Must implement IEndpointRouteBuilder.</param>
    /// <returns>The same IApplicationBuilder instance so that additional configuration can be chained.</returns>
    public static IApplicationBuilder UseHealthCheckEndpoint(this IApplicationBuilder app)
    {
        var routeBuilder = (IEndpointRouteBuilder)app;

        routeBuilder.MapHealthChecks("/healthz", new HealthCheckOptions
        {
            ResponseWriter = async (context, report) =>
            {
                context.Response.ContentType = "application/json";
                var result = JsonSerializer.Serialize(new
                {
                    status = report.Status.ToString(),
                    details = report.Entries.Select(e => new
                    {
                        name = e.Key,
                        status = e.Value.Status.ToString(),
                        description = e.Value.Description,
                    }),
                });
                await context.Response.WriteAsync(result);
            },
        });

        return app;
    }
}
