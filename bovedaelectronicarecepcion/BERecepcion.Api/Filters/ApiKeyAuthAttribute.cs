using BERecepcion.Api.Infrastructure.Auth;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BERecepcion.Api.Filters
{
    /// <summary>
    /// Filtro de autenticación dual: JWT Bearer Token (preferido) o ApiKey (fallback).
    /// Responsabilidad única: verificar que la request esté autenticada por alguno de los dos mecanismos.
    /// La autorización por rol se delega a las políticas de ASP.NET Core ([Authorize(Policy=...)]).
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class ApiKeyAuthAttribute : Attribute, IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var logger      = context.HttpContext.RequestServices.GetService<ILogger<ApiKeyAuthAttribute>>();
            var environment = context.HttpContext.RequestServices.GetService<IWebHostEnvironment>();

            // En desarrollo, permitir sin autenticación
            if (environment.IsDevelopment())
            {
                logger?.LogInformation("Ambiente de desarrollo — autenticación bypass.");
                await next();
                return;
            }

            // ── Opción 1: JWT Bearer Token (preferido) ───────────────────────────
            if (context.HttpContext.Request.Headers.TryGetValue(ApiAuthConstants.AuthorizationHeader, out var authHeader)
                && authHeader.ToString().StartsWith(ApiAuthConstants.BearerPrefix, StringComparison.OrdinalIgnoreCase))
            {
                if (context.HttpContext.User?.Identity?.IsAuthenticated == true)
                {
                    var userName = context.HttpContext.User.Claims
                        .FirstOrDefault(c => c.Type == ApiAuthConstants.PreferredUsernameClaim)?.Value
                        ?? context.HttpContext.User.Claims
                        .FirstOrDefault(c => c.Type == ApiAuthConstants.EmailClaim)?.Value;

                    logger?.LogInformation("✓ Autenticado con JWT Bearer — Usuario: {UserName}", userName);
                    await next();
                    return;
                }

                logger?.LogWarning("✗ Bearer Token presente pero usuario no autenticado — Token inválido o expirado.");
                context.Result = new UnauthorizedObjectResult(new { message = "Token inválido o expirado." });
                return;
            }

            // ── Opción 2: ApiKey fallback ─────────────────────────────────────────
            if (!context.HttpContext.Request.Headers.TryGetValue(ApiAuthConstants.ApiKeyHeaderName, out var posibleApiKey))
            {
                logger?.LogWarning("✗ No se encontró ni Bearer Token ni ApiKey.");
                context.Result = new UnauthorizedObjectResult(new { message = "Autenticación requerida — Bearer Token o ApiKey." });
                return;
            }

            var configuration  = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
            var consultaApiKey = configuration[ApiAuthConstants.ApiKeyConfigKey];

            if (!consultaApiKey.Equals(posibleApiKey))
            {
                logger?.LogWarning("✗ ApiKey inválido.");
                context.Result = new UnauthorizedObjectResult(new { message = "ApiKey inválido." });
                return;
            }

            logger?.LogInformation("✓ Autenticado con ApiKey (fallback).");
            await next();
        }
    }
}
