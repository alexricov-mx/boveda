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
    /// Filtro de autenticación dual: JWT Bearer Token (preferido) o ApiKey (fallback)
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class ApiKeyAuthAttribute : Attribute, IAsyncActionFilter
    {
        private const string ApiKeyName = "ApiKey";

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var logger = context.HttpContext.RequestServices.GetService<ILogger<ApiKeyAuthAttribute>>();
            var environment = context.HttpContext.RequestServices.GetService<IWebHostEnvironment>();
            
            // En desarrollo, permitir sin autenticación
            if (environment.IsDevelopment())
            {
                logger?.LogInformation("Ambiente de desarrollo - Autenticación bypass");
                await next();
                return;
            }

            // Verificar si hay un JWT Bearer Token (preferido)
            if (context.HttpContext.Request.Headers.TryGetValue("Authorization", out var authHeader))
            {
                var authHeaderValue = authHeader.ToString();
                if (authHeaderValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    // Verificar si el usuario está autenticado por el middleware de JWT
                    if (context.HttpContext.User?.Identity?.IsAuthenticated == true)
                    {
                        var userName = context.HttpContext.User.Claims.FirstOrDefault(c => c.Type == "preferred_username")?.Value 
                                       ?? context.HttpContext.User.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
                        logger?.LogInformation($"✓ Autenticado con JWT Bearer Token - Usuario: {userName}");
                        
                        // Logging de claims del usuario autenticado
                        var claims = context.HttpContext.User.Claims;
                        logger?.LogInformation($"========== CLAIMS BACKEND (Total: {claims.Count()}) ==========");
                        foreach (var claim in claims)
                        {
                            logger?.LogInformation($"  - {claim.Type}: {claim.Value}");
                        }
                        logger?.LogInformation("========================================");
                        
                        await next();
                        return;
                    }
                    else
                    {
                        logger?.LogWarning("✗ Bearer Token presente pero usuario no autenticado - Token inválido o expirado");
                        context.Result = new UnauthorizedObjectResult(new { message = "Token inválido o expirado" });
                        return;
                    }
                }
            }

            // Fallback: Verificar ApiKey
            if (!context.HttpContext.Request.Headers.TryGetValue(ApiKeyName, out var posibleApiKey))
            {
                logger?.LogWarning("✗ No se encontró ni Bearer Token ni ApiKey");
                context.Result = new UnauthorizedObjectResult(new { message = "Autenticación requerida - Bearer Token o ApiKey" });
                return;
            }
            
            var configuration = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
            var consultaApiKey = configuration["Seguridad:ApiKey"];
            if (!consultaApiKey.Equals(posibleApiKey))
            {
                logger?.LogWarning($"✗ ApiKey inválido: {posibleApiKey}");
                context.Result = new UnauthorizedObjectResult(new { message = "ApiKey inválido" });
                return;
            }

            logger?.LogInformation("✓ Autenticado con ApiKey (fallback)");
            await next();
        }
    }
}
