using BERRecepcion.Front.Interfaces;
using BERRecepcion.Front.Models;
using BERRecepcion.Front.Models.Dto;
using BERRecepcion.Front.Utilities;
using BERRecepcion.Front.Middleware;
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
        
        public Startup(IConfiguration configuration)
        {
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration).CreateLogger();
            Configuration = configuration;
        }
            

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            
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
                options.Events.OnTokenValidated = async ctx =>
                {
                    // MSAL primero: puebla el caché de tokens antes de nuestra lógica de negocio.
                    // Sin esto, GetAccessTokenForUserAsync falla con user_null en cada request.
                    if (msalOnTokenValidated != null)
                        await msalOnTokenValidated(ctx);

                        Serilog.Log.Information($"✓ Token validado exitosamente");

                        var email = ctx.Principal.Claims.FirstOrDefault(c => c.Type == "preferred_username")?.Value 
                                    ?? ctx.Principal.Claims.FirstOrDefault(c => c.Type == "email")?.Value 
                                    ?? "Unknown";
                        
                        Serilog.Log.Information($"========== CLAIMS DEL USUARIO: {email} ==========");
                        foreach (var claim in ctx.Principal.Claims)
                        {
                            Serilog.Log.Information($"  - {claim.Type}: {claim.Value}");
                        }
                        Serilog.Log.Information("========================================");

                        // Obtener el access token del token endpoint (no el ID token de ctx.SecurityToken)
                        var accessToken = ctx.TokenEndpointResponse?.AccessToken ?? ctx.SecurityToken?.RawData;

                        // Validar grupo de Azure AD
                        if (allowedGroups.Length > 0 && !allowedGroups.Any(string.IsNullOrWhiteSpace))
                        {
                            // Los grupos pueden venir en diferentes claims: "groups", "group", o dentro de roles
                            var userGroups = ctx.Principal.Claims
                                .Where(c => c.Type == "groups" || c.Type == "group")
                                .Select(c => c.Value)
                                .ToList();

                            // Si no hay grupos en el token, verificar si hay un claim de "hasgroups" o "_claim_names"
                            var hasGroupsOverage = ctx.Principal.Claims.Any(c => c.Type == "hasgroups" || c.Type == "_claim_names");
                            
                            if (hasGroupsOverage)
                            {
                                Serilog.Log.Warning($"Usuario {email} tiene demasiados grupos (overage). Permitiendo acceso y validando en la BD.");
                                // Cuando hay overage, permitimos el acceso y dejamos que la validación posterior en la BD lo maneje
                                // En producción, aquí podrías llamar a Microsoft Graph API para obtener los grupos
                            }
                            else if (userGroups.Count > 0)
                            {
                                Serilog.Log.Information($"Usuario {email} tiene los siguientes grupos en el token: {string.Join(", ", userGroups)}");
                                
                                if (!userGroups.Any(ug => allowedGroups.Contains(ug, StringComparer.OrdinalIgnoreCase)))
                                {
                                    Serilog.Log.Warning($"Usuario {email} sin acceso - no pertenece a grupos permitidos. Grupos requeridos: {string.Join(", ", allowedGroups)}");
                                    ctx.Fail("No tiene permisos para acceder a esta aplicación. Su usuario no pertenece al grupo autorizado.");
                                    return;
                                }
                                else
                                {
                                    Serilog.Log.Information($"Usuario {email} validado correctamente en grupo de Azure AD");
                                }
                            }
                            else
                            {
                                Serilog.Log.Warning($"Usuario {email} no tiene grupos en el token. Permitiendo acceso y validando en la BD.");
                            }
                        }

                        // Consulta los roles de los usuarios en la base de datos
                        try
                        {
                            // Configurar cliente HTTP con el token
                            var handler = new System.Net.Http.HttpClientHandler
                            {
                                ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) => true
                            };
                            var httpClient = new System.Net.Http.HttpClient(handler);
                            var options = new RestClientOptions(Configuration["ApiUrl"])
                            {
                                ConfigureMessageHandler = _ => handler
                            };
                            var client = new RestClient(options);

                            var request = new RestRequest("/Login", Method.Get);
                            request.AddHeader("ApiKey", Configuration.GetSection("Seguridad:ApiKey").Value);
                            
                            // Agregar el JWT token de Azure AD
                            if (!string.IsNullOrEmpty(accessToken))
                            {
                                request.AddHeader("Authorization", $"Bearer {accessToken}");
                                Serilog.Log.Information($"Token enviado en petición /Login para usuario {email}");
                            }
                            
                            request.AddParameter("Email", email);
                            
                            var response = await client.ExecuteAsync(request);
                            DataResult<UsersDto> loginDto = null;

                            Serilog.Log.Information($"Respuesta de API /Login - StatusCode: {response.StatusCode}, ContentLength: {response.Content?.Length ?? 0}");

                            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                            {
                                loginDto = JsonConvert.DeserializeObject<DataResult<UsersDto>>(response.Content);
                                Serilog.Log.Information($"API /Login deserializado correctamente - loginDto.Status: {loginDto.Status}, loginDto.Message: {loginDto.Message}");
                                
                                // Verificar si Data existe
                                if (loginDto.Data != null)
                                {
                                    Serilog.Log.Information($"API /Login Data - UserExists: {loginDto.Data.UserExists}, UserID: {loginDto.Data.UserID}, Email: {loginDto.Data.Email}");
                                }
                                else
                                {
                                    Serilog.Log.Warning($"API /Login retornó Data = null");
                                }
                            }
                            else
                            {
                                Serilog.Log.Error($"Error en petición /Login: {response.StatusCode} - {response.Content}");
                                loginDto = new DataResult<UsersDto>
                                {
                                    Status = response.StatusCode,
                                    Message = $"Error al consultar usuario: {response.StatusDescription}",
                                    Data = new UsersDto { UserExists = false }
                                };
                            }

                            if (loginDto.Status == System.Net.HttpStatusCode.OK)
                            {
                                var userData = new UsersDto
                                {
                                    UserID = loginDto.Data.UserID,
                                    UserName = loginDto.Data.UserName,
                                    Name = loginDto.Data.Name,
                                    UserType = loginDto.Data.UserType,
                                    Token = loginDto.Data.UserType.Equals("UserTypeP") ? loginDto.Data.CreditorNumber : loginDto.Data.Token,
                                    RFC = loginDto.Data.RFC,
                                    IsBlocked = loginDto.Data.IsBlocked,
                                    IsDeleted = loginDto.Data.IsDeleted,
                                    ProfileID = loginDto.Data.ProfileID,
                                    Email = loginDto.Data.Email,
                                    Company = loginDto.Data.Company,
                                    PhoneNumber = loginDto.Data.PhoneNumber,
                                    CreditorNumber = loginDto.Data.CreditorNumber,
                                    CreditorRFC = loginDto.Data.CreditorRFC,
                                    DateInitialValid = loginDto.Data.DateInitialValid,
                                    DateEndValid = loginDto.Data.DateEndValid,
                                    Organisms = loginDto.Data.Organisms
                                };

                                // Crear TODOS los claims en una sola identidad principal
                                // Esto es compatible con el código existente que usa FindFirst()
                                var claims = new List<Claim>
                                {
                                    // Claims de usuario
                                    new Claim("User", JsonConvert.SerializeObject(userData)),
                                    new Claim(ClaimTypes.Name, email),
                                    new Claim(ClaimTypes.Email, email),
                                    
                                    // Claims de roles
                                    new Claim("Roles", string.Join(",", loginDto.Data.Profile.RolesCatalogo.Select(x => x.Rol))),
                                    
                                    // ValidationClaims necesarios para ValidateUserAttribute
                                    new Claim("UserExists", loginDto.Data.UserExists.ToString()),
                                    new Claim("UserIsBlocked", loginDto.Data.IsBlocked.ToString()),
                                    new Claim("UserIsDeleted", loginDto.Data.IsDeleted.ToString()),
                                    new Claim("UserdateIsValid", loginDto.Data.UserdateIsValid.ToString()),
                                    new Claim("IsSapInterfaceEnabled", loginDto.Data.IsSapInterfaceEnabled.ToString()),
                                };
                                
                                // "ValidationClaims" es el AuthenticationType que ValidateUserAttribute busca.
                                // IsAuthenticated la provee la identidad Azure AD (primera en el principal).
                                var identity = new ClaimsIdentity(claims, "ValidationClaims");

                                // AGREGAR al principal en vez de reemplazarlo: reemplazar pierde OID/sub/tid
                                // que MSAL necesita como clave del caché de tokens para adquisición silenciosa.
                                ctx.Principal.AddIdentity(identity);
                                
                                // LOGS DE DIAGNÓSTICO - ver valores de validación
                                Serilog.Log.Information($"Claims de validación creados: UserExists={loginDto.Data.UserExists}, UserIsBlocked={loginDto.Data.IsBlocked}, UserIsDeleted={loginDto.Data.IsDeleted}, UserdateIsValid={loginDto.Data.UserdateIsValid}, IsSapInterfaceEnabled={loginDto.Data.IsSapInterfaceEnabled}");
                                
                                Serilog.Log.Information($"Usuario {email} autenticado correctamente en la aplicación");
                                ctx.Success();
                            }
                            else
                            {
                                if (loginDto.Data.UserExists)
                                {
                                    Serilog.Log.Information($"El {email}, si se encuentra en Base de datos y se encuentra con el siguiente estatus: {loginDto.Message}");
                                }
                                else
                                {
                                    Serilog.Log.Information($"El {email}, no se encuentra en Base de datos y se encuentra con el siguiente estatus: {loginDto.Message}");
                                }

                                var validationClaims = new List<Claim>()
                                    {
                                        new Claim("UserExists", loginDto.Data.UserExists.ToString()),
                                        new Claim("UserIsBlocked", loginDto.Data.UserIsBlocked.ToString()),
                                        new Claim("UserIsDeleted", loginDto.Data.UserIsDeleted.ToString()),
                                        new Claim("UserdateIsValid", loginDto.Data.UserdateIsValid.ToString()),
                                        new Claim("IsSapInterfaceEnabled", loginDto.Data.IsSapInterfaceEnabled.ToString()),
                                    };
                                var appIdentityValidations = new ClaimsIdentity(validationClaims, "ValidationClaims");
                                
                                // Registrar en bitácora con token
                                var bitacoraRequest = new RestRequest("Bitacora/InsertaBitacoraAsync", Method.Post);
                                bitacoraRequest.AddHeader("ApiKey", Configuration.GetSection("Seguridad:ApiKey").Value);
                                if (!string.IsNullOrEmpty(accessToken))
                                {
                                    bitacoraRequest.AddHeader("Authorization", $"Bearer {accessToken}");
                                }
                                var bitacoraData = new DataResult<BitacoraDto> 
                                { 
                                    Data = new BitacoraDto 
                                    { 
                                        Accion = "Login", 
                                        Descripcion = loginDto.Message + ":email " + email, 
                                        Seccion = "Login" 
                                    } 
                                };
                                bitacoraRequest.AddParameter("application/json", bitacoraData, ParameterType.RequestBody);
                                await client.ExecuteAsync(bitacoraRequest);
                                
                                ctx.Principal.AddIdentity(appIdentityValidations);
                            }
                            ctx.Success();
                        }
                        catch (Exception ex)
                        {
                            var errorClaims = new List<Claim>() { new Claim("LoginError", "True") };
                            var errorIdentity = new ClaimsIdentity(errorClaims, "LoginError");
                            ctx.Principal.AddIdentity(errorIdentity);
                            Serilog.Log.Error($"Se ha producido un error al consultar al usuario {email} en la BD: {ex.Message}");
                            ctx.Success();
                        }
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
           
            app.UseAuthentication();
            app.UseAuthorization();
            
            app.UseSession();

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
