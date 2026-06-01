using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using System.Threading.Tasks;

namespace BERRecepcion.Front.Infrastructure.Auth;

/// <summary>
/// Servicio encargado de enriquecer el ClaimsPrincipal del usuario con datos de negocio
/// después de la autenticación exitosa con Azure AD.
/// </summary>
/// <remarks>
/// Este servicio se invoca desde el evento OnTokenValidated de OpenID Connect
/// para consultar el backend y agregar claims adicionales (roles, permisos, validaciones).
/// </remarks>
public interface IUserLoginService
{
    /// <summary>
    /// Enriquece el ClaimsPrincipal del usuario autenticado con datos de negocio
    /// consultando el backend y agregando claims de validación.
    /// </summary>
    /// <param name="context">Contexto del evento TokenValidated de OIDC</param>
    /// <returns>Task completado cuando los claims han sido agregados al principal</returns>
    /// <remarks>
    /// - Consulta el endpoint /Login del backend con el email del usuario
    /// - Agrega claims de rol, validaciones de negocio y datos del usuario
    /// - Maneja errores de conexión agregando un claim de LoginError
    /// </remarks>
    Task EnrichPrincipalAsync(TokenValidatedContext context);
}
