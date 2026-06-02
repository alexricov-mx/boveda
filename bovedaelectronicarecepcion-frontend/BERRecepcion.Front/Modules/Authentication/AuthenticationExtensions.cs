using System;
using System.Threading.Tasks;
using BERRecepcion.Front.Infrastructure.Auth;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Web;

namespace BERRecepcion.Front.Modules.Authentication;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddAuthenticationExtensions(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApp(configuration.GetSection("AzureAd"))
            .EnableTokenAcquisitionToCallDownstreamApi(new[] { configuration["AzureAd:Scopes"] })
            .AddInMemoryTokenCaches();

        services.Configure<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme, options =>
            {
                // IMPORTANTE: Guardar tokens para poder usarlos en llamadas al backend
                options.SaveTokens = true;

                // Solicitar scope para llamar al backend API
                var scopes = configuration["AzureAd:Scopes"];
                if (!string.IsNullOrEmpty(scopes))
                {
                    foreach (var scope in scopes.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                    {
                        if (!options.Scope.Contains(scope))
                        {
                            options.Scope.Add(scope);
                            Serilog.Log.Information($"Agregado scope: {scope}");
                        }
                    }
                }

                // Configurar redirect después de cerrar sesión
                options.SignedOutRedirectUri = "/";

                // Validar que el usuario pertenezca al grupo de Azure AD permitido
                var allowedGroups = configuration["AzureAd:AllowedGroups"]?.Split(',') ?? Array.Empty<string>();

                // CRÍTICO: No reemplazar options.Events — MSAL ya registró OnAuthorizationCodeReceived
                // en EnableTokenAcquisitionToCallDownstreamApi. Reemplazarlo vaciaría el caché de tokens
                // y causaría MsalUiRequiredException (user_null) en cada request post-login.
                options.Events ??= new OpenIdConnectEvents();
                var msalOnRemoteFailure  = options.Events.OnRemoteFailure;
                var msalOnTokenValidated = options.Events.OnTokenValidated;

                // ========================================
                // MANEJO TEMPORAL de AADSTS54005
                // ========================================
                // Este evento se puede ELIMINAR una vez que NetScaler tenga Cookie Persistence configurado.
                // Actualmente maneja el duplicado POST que NetScaler sigue enviando.
                options.Events.OnRemoteFailure = context =>
                {
                        if (context.Failure?.Message != null && 
                            (context.Failure.Message.Contains("AADSTS54005") || 
                             context.Failure.Message.Contains("already redeemed")))
                        {
                            Serilog.Log.Warning($"⚠ AADSTS54005 detectado - Duplicate POST del NetScaler");

                            // IMPORTANTE: Este duplicado es inevitable sin Cookie Persistence en NetScaler.
                            // Simplemente ignorarlo y dejar que el primer POST complete la autenticación.
                            Serilog.Log.Information($"→ Ignorando POST duplicado. Usuario debería ser autenticado por el primer POST.");
                            context.HandleResponse();

                            // Redirigir a home - si el primer POST fue exitoso, el usuario estará autenticado
                            // Si falló, el [Authorize] lo enviará al login de nuevo
                            context.Response.Redirect("/");
                            return Task.CompletedTask;
                        }

                        // Otros errores - mostrar página de error
                        Serilog.Log.Error($"❌ Error de autenticación: {context.Failure?.Message}");
                        context.HandleResponse();
                        context.Response.Redirect("/Home/Error");
                        return Task.CompletedTask;
                };

                // ========================================
                // LÓGICA DE NEGOCIO (permanente)
                // ========================================
                // REFACTOR: Lógica extraída a IUserLoginService para cumplir SRP
                // El servicio se obtiene del IServiceProvider del contexto
                options.Events.OnTokenValidated = async ctx =>
                {
                    // MSAL primero: puebla el caché de tokens antes de nuestra lógica de negocio.
                    // Sin esto, GetAccessTokenForUserAsync falla con user_null en cada request.
                    if (msalOnTokenValidated != null)
                        await msalOnTokenValidated(ctx);

                    // Delegar al servicio de negocio inyectado
                    var loginService = ctx.HttpContext.RequestServices.GetRequiredService<IUserLoginService>();
                    await loginService.EnrichPrincipalAsync(ctx);
                };
            });
        return services;
    }
}