using BERRecepcion.Front.Interfaces;
using BERRecepcion.Front.Models;
using BERRecepcion.Front.Services;
using BERRecepcion.Front.Models.Dto;
using BERRecepcion.Front.Utilities;
using BERRecepcion.Front.Middleware;
using BERRecepcion.Front.Infrastructure.Auth;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Forms.Mapping;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Newtonsoft.Json;
using RestSharp;
using Serilog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BERRecepcion.Front
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        private readonly IWebHostEnvironment _env;

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration).CreateLogger();
            Configuration = configuration;
            _env = env;
        }
            

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpContextAccessor();

            // ========================================
            // HTTP CLIENT FACTORY: Mejora gestión de HttpClient según best practices
            // ========================================
            services.AddHttpClient();

            // ========================================
            // SERVICIOS DE AUTENTICACIÓN/AUTORIZACIÓN
            // ========================================
            services.AddScoped<IUserLoginService, UserLoginService>();
            services.AddScoped<ICurrentUserService, CurrentUserService>();

            // ========================================
            // DATA PROTECTION: Persistencia de claves para autenticación
            // ========================================
            
            // SOLUCIÓN: Intentar múltiples ubicaciones para Data Protection
            DirectoryInfo keysDirectory = null;
            var keysPaths = new[]
            {
                Path.Combine(AppContext.BaseDirectory, "App_Data", "DataProtectionKeys"),
                Path.Combine(AppContext.BaseDirectory, "DataProtectionKeys"),
                Path.Combine(Path.GetTempPath(), "BERRecepcion", "DataProtectionKeys")
            };
            
            foreach (var path in keysPaths)
            {
                try
                {
                    Directory.CreateDirectory(path);
                    // Intentar escribir un archivo de prueba
                    var testFile = Path.Combine(path, "test.txt");
                    File.WriteAllText(testFile, "test");
                    File.Delete(testFile);
                    
                    keysDirectory = new DirectoryInfo(path);
                    Serilog.Log.Information($"✓ Data Protection Keys configuradas en: {path}");
                    break;
                }
                catch (Exception ex)
                {
                    Serilog.Log.Warning($"✗ No se pudo usar {path}: {ex.Message}");
                }
            }
            
            if (keysDirectory != null)
            {
                services.AddDataProtection()
                    .PersistKeysToFileSystem(keysDirectory)
                    .SetApplicationName("BERRecepcion.Front")
                    .SetDefaultKeyLifetime(TimeSpan.FromDays(90));
            }
            else
            {
                Serilog.Log.Error("⚠ ADVERTENCIA: No se pudo configurar persistencia de claves. Usando protección en memoria (las sesiones se perderán al reciclar App Pool)");
                services.AddDataProtection()
                    .SetApplicationName("BERRecepcion.Front");
            }
            
            // ========================================
            
            services.Configure<CookiePolicyOptions>(options =>
            {
                // Sin UI de consentimiento GDPR, CheckConsentNeeded debe ser false
                // para no bloquear cookies de sesión y AntiForgery.
                // Las cookies de autenticación ya tienen IsEssential=true en el framework.
                options.CheckConsentNeeded = context => false;
                options.MinimumSameSitePolicy = SameSiteMode.Unspecified;
            });

            // ========================================
            // CONFIGURACIÓN SIMPLIFICADA - PATRÓN DEL PROXY (sigec-backend)
            // ========================================
            // El Proxy funciona perfectamente con NetScaler sin ningún middleware
            // ni configuración especial de eventos. La solución para AADSTS54005
            // es Cookie Persistence/Sticky Sessions en NetScaler, NO en código.
            
            // Configurar autenticación con Microsoft Identity Web (Azure Entra ID)
            services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
                    .AddMicrosoftIdentityWebApp(Configuration.GetSection("AzureAd"))
                    .EnableTokenAcquisitionToCallDownstreamApi(new[] { Configuration["AzureAd:Scopes"] })
                    .AddInMemoryTokenCaches();

            // ========================================
            // LÓGICA DE NEGOCIO: Validación de grupos y roles
            // ========================================
            // NOTA: Esto es diferente al Proxy porque BER necesita consultar roles en BD
            services.Configure<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme, options =>
            {
                // IMPORTANTE: Guardar tokens para poder usarlos en llamadas al backend
                options.SaveTokens = true;
                
                // Solicitar scope para llamar al backend API
                var scopes = Configuration["AzureAd:Scopes"];
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
                
                // FIX: Aumentar expiración de la correlation cookie de 15 min (default) a 30 min.
                // Evita "Correlation failed" cuando el usuario tarda en completar el login de Azure AD.
                options.CorrelationCookie.Expiration = TimeSpan.FromMinutes(30);

                // Configurar redirect después de cerrar sesión
                options.SignedOutRedirectUri = "/";
                
                // Validar que el usuario pertenezca al grupo de Azure AD permitido
                var allowedGroups = Configuration["AzureAd:AllowedGroups"]?.Split(',') ?? Array.Empty<string>();
                
                // CRÍTICO: No reemplazar options.Events — MSAL ya registró OnAuthorizationCodeReceived
                // en EnableTokenAcquisitionToCallDownstreamApi. Reemplazarlo vaciaría el caché de tokens
                // y causaría MsalUiRequiredException (user_null) en cada request post-login.
                options.Events ??= new OpenIdConnectEvents();
                var msalOnRemoteFailure  = options.Events.OnRemoteFailure;
                var msalOnTokenValidated = options.Events.OnTokenValidated;

                // ========================================
                // MANEJO DE FALLOS DE AUTENTICACIÓN REMOTA
                // ========================================
                options.Events.OnRemoteFailure = context =>
                {
                    var errorMsg = context.Failure?.Message ?? string.Empty;

                    // AADSTS54005: código de autorización ya canjeado (duplicate POST de NetScaler).
                    // Se puede ELIMINAR una vez que NetScaler tenga Cookie Persistence configurado.
                    if (errorMsg.Contains("AADSTS54005") || errorMsg.Contains("already redeemed"))
                    {
                        Serilog.Log.Warning("⚠ AADSTS54005 detectado - Duplicate POST del NetScaler. Redirigiendo a home.");
                        context.HandleResponse();
                        context.Response.Redirect("/");
                        return Task.CompletedTask;
                    }

                    // FIX: Correlation failed — la correlation cookie expiró o no se encontró
                    // (timeout > 15 min, recarga del browser, o proceso reiniciado).
                    // En lugar de mostrar página de error, reiniciar el flujo de login.
                    if (errorMsg.Contains("Correlation failed"))
                    {
                        Serilog.Log.Warning("⚠ Correlation failed — reiniciando flujo de login.");
                        context.HandleResponse();
                        context.Response.Redirect("/");
                        return Task.CompletedTask;
                    }

                    // Otros errores inesperados — mostrar página de error
                    Serilog.Log.Error($"❌ Error de autenticación: {errorMsg}");
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
            services.AddDistributedMemoryCache();

            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromHours(10);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });
            services.AddControllersWithViews(options =>
            {
                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
                options.Filters.Add(new AuthorizeFilter(policy));
            });//.AddFluentValidation(fluConfiguration => fluConfiguration.RegisterValidatorsFromAssemblyContaining<Startup>());
            services.AddFluentValidationAutoValidation().AddFluentValidationClientsideAdapters();
            services.AddRazorPages().AddMicrosoftIdentityUI();

            services.AddScoped<IRestUtility, RestUtility>();
            services.AddTransient<IGenerals, Generals>();

            // CORS para el servidor de desarrollo de Vue (solo en Development)
            // Permite que http://localhost:4000 llame a /BerFront/Token con credenciales
            if (_env.IsDevelopment())
            {
                services.AddCors(options =>
                {
                    options.AddPolicy("VueDevOrigin", policy =>
                    {
                        policy.WithOrigins("http://localhost:4000", "https://localhost:4000")
                              .AllowAnyMethod()
                              .AllowAnyHeader()
                              .AllowCredentials();
                    });
                });
            }

            // Levantar servidor Vue en desarrollo si está configurada la ruta
            if (_env.IsDevelopment() && !string.IsNullOrWhiteSpace(Configuration["VueApp:FrontPath"]))
            {
                services.AddHostedService<VueDevHostedService>();
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            // Fallback SPA para rutas del app Vue (/BERVueDist/**)
            // Las rutas sin extensión devuelven index.html; los archivos estáticos
            // (_nuxt/*.js, *.css, etc.) ya son servidos por UseStaticFiles arriba.
            app.Use(async (context, next) =>
            {
                var reqPath = context.Request.Path.Value ?? string.Empty;
                if (reqPath.StartsWith("/BERVueDist", StringComparison.OrdinalIgnoreCase)
                    && !Path.HasExtension(reqPath))
                {
                    var indexFile = Path.Combine(env.WebRootPath, "BERVueDist", "index.html");
                    if (File.Exists(indexFile))
                    {
                        context.Response.ContentType = "text/html";
                        await context.Response.SendFileAsync(indexFile);
                        return;
                    }
                }
                await next();
            });

            // ========================================
            // MIDDLEWARE DESHABILITADO - NO FUNCIONA EN LOAD BALANCER
            // ========================================
            // PreventDoublePostMiddleware NO puede funcionar en entorno de load balancer
            // porque el ConcurrentDictionary es en memoria y no se comparte entre servidores.
            // Cuando NetScaler envía el duplicado POST, puede llegar a un servidor IIS diferente.
            //
            // SOLUCIÓN CORRECTA: Cookie Persistence/Sticky Sessions en NetScaler
            // El proyecto Proxy (sigec-backend) funciona perfectamente sin middleware
            // porque NetScaler está configurado para rutear al mismo servidor durante OAuth.
            //
            // app.UseMiddleware<PreventDoublePostMiddleware>();

            app.UseRouting();

            // ========================================
            // FIX CRÍTICO: UseSession DEBE ir ANTES de UseAuthentication/UseAuthorization
            // ========================================
            // ValidateUserAttribute lee HttpContext.Session["UserMenu"].
            // Si UseSession() está después de UseAuthorization(), la sesión NO existe
            // cuando los filtros de autorización se ejecutan, causando comportamiento impredecible.
            app.UseSession();
            // if (env.IsDevelopment())
            // {
            //     app.UseCors("VueDevOrigin");
            // }

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });

            var cultureInfo = new CultureInfo("es-MX");
            CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
            CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
        }
    }
}
