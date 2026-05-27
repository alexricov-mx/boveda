using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using BERRecepcion.Front.Models;
using BERRecepcion.Front.Models.Dto;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using RestSharp;

namespace BERRecepcion.Front.Modules.RolesPermissions;

public static class RolesPermissionsExtensions
{
    public static IServiceCollection AddRolesPermissionsExtensions(this IServiceCollection services,
        IConfiguration configuration)
    {
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

            options.Events = new OpenIdConnectEvents
            {
                // ========================================
                // MANEJO TEMPORAL de AADSTS54005
                // ========================================
                // Este evento se puede ELIMINAR una vez que NetScaler tenga Cookie Persistence configurado.
                // Actualmente maneja el duplicado POST que NetScaler sigue enviando.
                OnRemoteFailure = context =>
                {
                    if (context.Failure?.Message != null &&
                        (context.Failure.Message.Contains("AADSTS54005") ||
                         context.Failure.Message.Contains("already redeemed")))
                    {
                        Serilog.Log.Warning($"⚠ AADSTS54005 detectado - Duplicate POST del NetScaler");

                        // IMPORTANTE: Este duplicado es inevitable sin Cookie Persistence en NetScaler.
                        // Simplemente ignorarlo y dejar que el primer POST complete la autenticación.
                        Serilog.Log.Information(
                            $"→ Ignorando POST duplicado. Usuario debería ser autenticado por el primer POST.");
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
                },

                // ========================================
                // LÓGICA DE NEGOCIO (permanente)
                // ========================================
                OnTokenValidated = async ctx =>
                {
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

                    // Obtener el access token del contexto de autenticación
                    var accessToken = ctx.SecurityToken?.RawData;

                    // Validar grupo de Azure AD
                    if (allowedGroups.Length > 0 && !allowedGroups.Any(string.IsNullOrWhiteSpace))
                    {
                        // Los grupos pueden venir en diferentes claims: "groups", "group", o dentro de roles
                        var userGroups = ctx.Principal.Claims
                            .Where(c => c.Type == "groups" || c.Type == "group")
                            .Select(c => c.Value)
                            .ToList();

                        // Si no hay grupos en el token, verificar si hay un claim de "hasgroups" o "_claim_names"
                        var hasGroupsOverage =
                            ctx.Principal.Claims.Any(c => c.Type == "hasgroups" || c.Type == "_claim_names");

                        if (hasGroupsOverage)
                        {
                            Serilog.Log.Warning(
                                $"Usuario {email} tiene demasiados grupos (overage). Permitiendo acceso y validando en la BD.");
                            // Cuando hay overage, permitimos el acceso y dejamos que la validación posterior en la BD lo maneje
                            // En producción, aquí podrías llamar a Microsoft Graph API para obtener los grupos
                        }
                        else if (userGroups.Count > 0)
                        {
                            Serilog.Log.Information(
                                $"Usuario {email} tiene los siguientes grupos en el token: {string.Join(", ", userGroups)}");

                            if (!userGroups.Any(ug => allowedGroups.Contains(ug, StringComparer.OrdinalIgnoreCase)))
                            {
                                Serilog.Log.Warning(
                                    $"Usuario {email} sin acceso - no pertenece a grupos permitidos. Grupos requeridos: {string.Join(", ", allowedGroups)}");
                                ctx.Fail(
                                    "No tiene permisos para acceder a esta aplicación. Su usuario no pertenece al grupo autorizado.");
                                return;
                            }
                            else
                            {
                                Serilog.Log.Information($"Usuario {email} validado correctamente en grupo de Azure AD");
                            }
                        }
                        else
                        {
                            Serilog.Log.Warning(
                                $"Usuario {email} no tiene grupos en el token. Permitiendo acceso y validando en la BD.");
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
                        var options = new RestClientOptions(configuration["ApiUrl"])
                        {
                            ConfigureMessageHandler = _ => handler
                        };
                        var client = new RestClient(options);

                        var request = new RestRequest("/Login", Method.Get);
                        request.AddHeader("ApiKey", configuration.GetSection("Seguridad:ApiKey").Value);

                        // Agregar el JWT token de Azure AD
                        if (!string.IsNullOrEmpty(accessToken))
                        {
                            request.AddHeader("Authorization", $"Bearer {accessToken}");
                            Serilog.Log.Information($"Token enviado en petición /Login para usuario {email}");
                        }

                        request.AddParameter("Email", email);

                        var response = await client.ExecuteAsync(request);
                        DataResult<UsersDto> loginDto = null;

                        Serilog.Log.Information(
                            $"Respuesta de API /Login - StatusCode: {response.StatusCode}, ContentLength: {response.Content?.Length ?? 0}");

                        if (response.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            loginDto = JsonConvert.DeserializeObject<DataResult<UsersDto>>(response.Content);
                            Serilog.Log.Information(
                                $"API /Login deserializado correctamente - loginDto.Status: {loginDto.Status}, loginDto.Message: {loginDto.Message}");

                            // Verificar si Data existe
                            if (loginDto.Data != null)
                            {
                                Serilog.Log.Information(
                                    $"API /Login Data - UserExists: {loginDto.Data.UserExists}, UserID: {loginDto.Data.UserID}, Email: {loginDto.Data.Email}");
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
                                Token = loginDto.Data.UserType.Equals("UserTypeP")
                                    ? loginDto.Data.CreditorNumber
                                    : loginDto.Data.Token,
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
                                new Claim("Roles",
                                    string.Join(",", loginDto.Data.Profile.RolesCatalogo.Select(x => x.Rol))),

                                // ValidationClaims necesarios para ValidateUserAttribute
                                new Claim("UserExists", loginDto.Data.UserExists.ToString()),
                                new Claim("UserIsBlocked", loginDto.Data.IsBlocked.ToString()),
                                new Claim("UserIsDeleted", loginDto.Data.IsDeleted.ToString()),
                                new Claim("UserdateIsValid", loginDto.Data.UserdateIsValid.ToString()),
                                new Claim("IsSapInterfaceEnabled", loginDto.Data.IsSapInterfaceEnabled.ToString()),
                            };

                            // Crear identidad principal autenticada con TODOS los claims
                            // AuthenticationType "Cookies" es CRÍTICO para User.Identity.IsAuthenticated
                            var identity = new ClaimsIdentity(claims, "Cookies");

                            // Reemplazar el principal completamente
                            ctx.Principal = new ClaimsPrincipal(identity);

                            // LOGS DE DIAGNÓSTICO - ver valores de validación
                            Serilog.Log.Information(
                                $"Claims de validación creados: UserExists={loginDto.Data.UserExists}, UserIsBlocked={loginDto.Data.IsBlocked}, UserIsDeleted={loginDto.Data.IsDeleted}, UserdateIsValid={loginDto.Data.UserdateIsValid}, IsSapInterfaceEnabled={loginDto.Data.IsSapInterfaceEnabled}");

                            Serilog.Log.Information($"Usuario {email} autenticado correctamente en la aplicación");
                            ctx.Success();
                        }
                        else
                        {
                            if (loginDto.Data.UserExists)
                            {
                                Serilog.Log.Information(
                                    $"El {email}, si se encuentra en Base de datos y se encuentra con el siguiente estatus: {loginDto.Message}");
                            }
                            else
                            {
                                Serilog.Log.Information(
                                    $"El {email}, no se encuentra en Base de datos y se encuentra con el siguiente estatus: {loginDto.Message}");
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
                            bitacoraRequest.AddHeader("ApiKey", configuration.GetSection("Seguridad:ApiKey").Value);
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
                        Serilog.Log.Error(
                            $"Se ha producido un error al consultar al usuario {email} en la BD: {ex.Message}");
                        ctx.Success();
                    }
                },
            };
        });

        return services;
    }
}