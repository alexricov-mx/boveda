using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BERecepcion.Api.StartupExtensions
{
    public static class HealthChecksConfigurationExtension
    {
        public static void AddHealthChecksConfig(this IServiceCollection services,IConfiguration configuration)
        {
            IHealthChecksBuilder healthChecksBuilder = services.AddHealthChecks()
                .AddSqlServer(
                     configuration["ConnectionStrings:SQLServerSQLDEV002"],
                    name: "SQLServer",
                    failureStatus: HealthStatus.Unhealthy
                );
        }

        public static void UseHealthChecksConfig(this IApplicationBuilder app)
        {
            app.UseHealthChecks("/api/health", new HealthCheckOptions()
            {
                Predicate = _ => true,
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
                ResultStatusCodes = {
                    [HealthStatus.Healthy] = StatusCodes.Status200OK,
                    [HealthStatus.Degraded] = StatusCodes.Status500InternalServerError,
                    [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
                }
            });
        }
    }
}
