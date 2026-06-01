using BERRecepcion.Front.Models.Dto;

namespace BERRecepcion.Front.Infrastructure.Auth;

/// <summary>
/// Servicio para acceso al usuario autenticado actualmente en la request.
/// Separa la responsabilidad de acceso al usuario de IGenerals (SRP).
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Obtiene el usuario autenticado actual desde los claims del ClaimsPrincipal.
    /// </summary>
    /// <returns>
    /// El DTO del usuario autenticado o null si no hay usuario autenticado
    /// o el claim User no existe.
    /// </returns>
    /// <remarks>
    /// Este método deserializa el claim "User" (JSON de UsersDto) una sola vez
    /// por request (lazy-init con caché en memoria) para mejorar performance.
    /// </remarks>
    UsersDto? GetCurrentUser();

    /// <summary>
    /// Verifica si el usuario actual tiene un rol específico.
    /// </summary>
    /// <param name="role">Nombre del rol a verificar (case-sensitive)</param>
    /// <returns>
    /// true si el usuario tiene el rol especificado, false en caso contrario
    /// o si no hay usuario autenticado.
    /// </returns>
    /// <remarks>
    /// Los roles se almacenan como CSV en el claim "Roles" (e.g. "Admin,Editor,Viewer").
    /// </remarks>
    bool HasRole(string role);

    /// <summary>
    /// Verifica si el usuario actual tiene alguno de los roles especificados.
    /// </summary>
    /// <param name="roles">Array de roles a verificar</param>
    /// <returns>
    /// true si el usuario tiene al menos uno de los roles, false en caso contrario.
    /// </returns>
    bool HasAnyRole(params string[] roles);
}
