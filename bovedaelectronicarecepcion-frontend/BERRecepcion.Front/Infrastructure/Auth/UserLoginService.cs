using BERRecepcion.Front.Models;
using BERRecepcion.Front.Models.Dto;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using Newtonsoft.Json;
using RestSharp;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BERRecepcion.Front.Infrastructure.Auth;

/// <summary>
/// Implementación del servicio de enriquecimiento de claims post-autenticación.
/// Consulta el backend para obtener roles, permisos y validaciones de negocio.
/// </summary>
public class UserLoginService : IUserLoginService
{
    private readonly IConfiguration _configuration;

    public UserLoginService(IConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    public async Task EnrichPrincipalAsync(TokenValidatedContext context)
    {
        var email = ExtractUserEmail(context);
        var accessToken = context.TokenEndpointResponse?.AccessToken ?? context.SecurityToken?.RawData;

        Log.Information($"========== CLAIMS DEL USUARIO: {email} ==========");
        foreach (var claim in context.Principal.Claims)
        {
            Log.Information($"  - {claim.Type}: {claim.Value}");
        }
        Log.Information("========================================");

        // Validar grupo de Azure AD (si está configurado)
        await ValidateAzureAdGroupAsync(context, email);

        // Consultar backend para obtener roles y validaciones
        await EnrichWithBackendDataAsync(context, email, accessToken);
    }

    /// <summary>
    /// Extrae el email del usuario desde los claims del token de Azure AD.
    /// </summary>
    private static string ExtractUserEmail(TokenValidatedContext context)
    {
        return context.Principal.Claims.FirstOrDefault(c => c.Type == "preferred_username")?.Value
               ?? context.Principal.Claims.FirstOrDefault(c => c.Type == "email")?.Value
               ?? "Unknown";
    }

    /// <summary>
    /// Valida que el usuario pertenezca al grupo permitido de Azure AD.
    /// </summary>
    private async Task ValidateAzureAdGroupAsync(TokenValidatedContext context, string email)
    {
        var allowedGroups = _configuration["AzureAd:AllowedGroups"]?.Split(',') ?? Array.Empty<string>();

        if (allowedGroups.Length == 0 || allowedGroups.Any(string.IsNullOrWhiteSpace))
        {
            return; // Sin restricción de grupos configurada
        }

        var userGroups = context.Principal.Claims
            .Where(c => c.Type == "groups" || c.Type == "group")
            .Select(c => c.Value)
            .ToList();

        var hasGroupsOverage = context.Principal.Claims.Any(c => c.Type == "hasgroups" || c.Type == "_claim_names");

        if (hasGroupsOverage)
        {
            Log.Warning($"Usuario {email} tiene demasiados grupos (overage). Permitiendo acceso y validando en la BD.");
        }
        else if (userGroups.Count > 0)
        {
            Log.Information($"Usuario {email} tiene los siguientes grupos en el token: {string.Join(", ", userGroups)}");

            if (!userGroups.Any(ug => allowedGroups.Contains(ug, StringComparer.OrdinalIgnoreCase)))
            {
                Log.Warning($"Usuario {email} sin acceso - no pertenece a grupos permitidos. Grupos requeridos: {string.Join(", ", allowedGroups)}");
                context.Fail("No tiene permisos para acceder a esta aplicación. Su usuario no pertenece al grupo autorizado.");
                return;
            }

            Log.Information($"Usuario {email} validado correctamente en grupo de Azure AD");
        }
        else
        {
            Log.Warning($"Usuario {email} no tiene grupos en el token. Permitiendo acceso y validando en la BD.");
        }

        await Task.CompletedTask;
    }

    /// <summary>
    /// Consulta el backend /Login y enriquece el principal con roles y validaciones.
    /// </summary>
    private async Task EnrichWithBackendDataAsync(TokenValidatedContext context, string email, string? accessToken)
    {
        try
        {
            var loginDto = await CallBackendLoginAsync(email, accessToken);

            if (loginDto.Status == HttpStatusCode.OK && loginDto.Data != null)
            {
                AddSuccessfulLoginClaims(context, loginDto.Data, email);
            }
            else
            {
                await AddFailedLoginClaimsAsync(context, loginDto, email, accessToken);
            }

            context.Success();
        }
        catch (Exception ex)
        {
            AddLoginErrorClaim(context, email, ex);
            context.Success();
        }
    }

    /// <summary>
    /// Realiza la llamada HTTP al backend /Login.
    /// </summary>
    private async Task<DataResult<UsersDto>> CallBackendLoginAsync(string email, string? accessToken)
    {
        var handler = new System.Net.Http.HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) => true
        };

        var options = new RestClientOptions(_configuration["ApiUrl"])
        {
            ConfigureMessageHandler = _ => handler
        };

        var client = new RestClient(options);
        var request = new RestRequest("/Login", Method.Get);

        request.AddHeader("ApiKey", _configuration.GetSection("Seguridad:ApiKey").Value);

        if (!string.IsNullOrEmpty(accessToken))
        {
            request.AddHeader("Authorization", $"Bearer {accessToken}");
            Log.Information($"Token enviado en petición /Login para usuario {email}");
        }

        request.AddParameter("Email", email);

        var response = await client.ExecuteAsync(request);
        Log.Information($"Respuesta de API /Login - StatusCode: {response.StatusCode}, ContentLength: {response.Content?.Length ?? 0}");

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var loginDto = JsonConvert.DeserializeObject<DataResult<UsersDto>>(response.Content);
            Log.Information($"API /Login deserializado correctamente - Status: {loginDto.Status}, Message: {loginDto.Message}");

            if (loginDto.Data != null)
            {
                Log.Information($"API /Login Data - UserExists: {loginDto.Data.UserExists}, UserID: {loginDto.Data.UserID}, Email: {loginDto.Data.Email}");
            }
            else
            {
                Log.Warning("API /Login retornó Data = null");
            }

            return loginDto;
        }

        Log.Error($"Error en petición /Login: {response.StatusCode} - {response.Content}");
        return new DataResult<UsersDto>
        {
            Status = response.StatusCode,
            Message = $"Error al consultar usuario: {response.StatusDescription}",
            Data = new UsersDto { UserExists = false }
        };
    }

    /// <summary>
    /// Agrega claims de usuario exitoso (roles, permisos, datos personales).
    /// </summary>
    private void AddSuccessfulLoginClaims(TokenValidatedContext context, UsersDto userData, string email)
    {
        var claims = new List<Claim>
        {
            // Claims de usuario
            new Claim(ClaimConstants.UserClaim, JsonConvert.SerializeObject(new UsersDto
            {
                UserID = userData.UserID,
                UserName = userData.UserName,
                Name = userData.Name,
                UserType = userData.UserType,
                Token = userData.UserType.Equals("UserTypeP") ? userData.CreditorNumber : userData.Token,
                RFC = userData.RFC,
                IsBlocked = userData.IsBlocked,
                IsDeleted = userData.IsDeleted,
                ProfileID = userData.ProfileID,
                Email = userData.Email,
                Company = userData.Company,
                PhoneNumber = userData.PhoneNumber,
                CreditorNumber = userData.CreditorNumber,
                CreditorRFC = userData.CreditorRFC,
                DateInitialValid = userData.DateInitialValid,
                DateEndValid = userData.DateEndValid,
                Organisms = userData.Organisms
            })),
            new Claim(ClaimTypes.Name, email),
            new Claim(ClaimTypes.Email, email),

            // Claims de roles
            new Claim(ClaimConstants.RolesClaim, string.Join(",", userData.Profile.RolesCatalogo.Select(x => x.Rol))),

            // ValidationClaims necesarios para ValidateUserAttribute
            new Claim(ClaimConstants.UserExistsClaim, userData.UserExists.ToString()),
            new Claim(ClaimConstants.UserIsBlockedClaim, userData.IsBlocked.ToString()),
            new Claim(ClaimConstants.UserIsDeletedClaim, userData.IsDeleted.ToString()),
            new Claim(ClaimConstants.UserdateIsValidClaim, userData.UserdateIsValid.ToString()),
            new Claim(ClaimConstants.IsSapInterfaceEnabledClaim, userData.IsSapInterfaceEnabled.ToString())
        };

        var identity = new ClaimsIdentity(claims, ClaimConstants.ValidationClaimsAuthType);
        context.Principal.AddIdentity(identity);

        Log.Information($"Claims de validación creados: UserExists={userData.UserExists}, UserIsBlocked={userData.IsBlocked}, UserIsDeleted={userData.IsDeleted}, UserdateIsValid={userData.UserdateIsValid}, IsSapInterfaceEnabled={userData.IsSapInterfaceEnabled}");
        Log.Information($"Usuario {email} autenticado correctamente en la aplicación");
    }

    /// <summary>
    /// Agrega claims de validación para usuarios que existen pero no pueden acceder.
    /// </summary>
    private async Task AddFailedLoginClaimsAsync(TokenValidatedContext context, DataResult<UsersDto> loginDto, string email, string? accessToken)
    {
        if (loginDto.Data.UserExists)
        {
            Log.Information($"El {email}, si se encuentra en Base de datos y se encuentra con el siguiente estatus: {loginDto.Message}");
        }
        else
        {
            Log.Information($"El {email}, no se encuentra en Base de datos y se encuentra con el siguiente estatus: {loginDto.Message}");
        }

        var validationClaims = new List<Claim>
        {
            new Claim(ClaimConstants.UserExistsClaim, loginDto.Data.UserExists.ToString()),
            new Claim(ClaimConstants.UserIsBlockedClaim, loginDto.Data.UserIsBlocked.ToString()),
            new Claim(ClaimConstants.UserIsDeletedClaim, loginDto.Data.UserIsDeleted.ToString()),
            new Claim(ClaimConstants.UserdateIsValidClaim, loginDto.Data.UserdateIsValid.ToString()),
            new Claim(ClaimConstants.IsSapInterfaceEnabledClaim, loginDto.Data.IsSapInterfaceEnabled.ToString())
        };

        var appIdentityValidations = new ClaimsIdentity(validationClaims, ClaimConstants.ValidationClaimsAuthType);
        context.Principal.AddIdentity(appIdentityValidations);

        // Registrar en bitácora
        await LogFailedLoginToBitacoraAsync(loginDto, email, accessToken);
    }

    /// <summary>
    /// Registra un intento de login fallido en la bitácora del backend.
    /// </summary>
    private async Task LogFailedLoginToBitacoraAsync(DataResult<UsersDto> loginDto, string email, string? accessToken)
    {
        try
        {
            var handler = new System.Net.Http.HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) => true
            };

            var options = new RestClientOptions(_configuration["ApiUrl"])
            {
                ConfigureMessageHandler = _ => handler
            };

            var client = new RestClient(options);
            var bitacoraRequest = new RestRequest("Bitacora/InsertaBitacoraAsync", Method.Post);

            bitacoraRequest.AddHeader("ApiKey", _configuration.GetSection("Seguridad:ApiKey").Value);

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
        }
        catch (Exception ex)
        {
            Log.Warning($"No se pudo registrar en bitácora el login fallido: {ex.Message}");
        }
    }

    /// <summary>
    /// Agrega claim de error cuando falla la consulta al backend.
    /// </summary>
    private void AddLoginErrorClaim(TokenValidatedContext context, string email, Exception ex)
    {
        var errorClaims = new List<Claim> { new Claim(ClaimConstants.LoginErrorClaim, "True") };
        var errorIdentity = new ClaimsIdentity(errorClaims, ClaimConstants.LoginErrorAuthType);
        context.Principal.AddIdentity(errorIdentity);
        Log.Error($"Se ha producido un error al consultar al usuario {email} en la BD: {ex.Message}");
    }
}
