using System.Collections.Generic;

namespace BERecepcion.Core.Interfaces.Auth
{
    /// <summary>
    /// Abstracción del usuario autenticado actual.
    /// Permite acceder a los datos del usuario desde el JWT + claims enriquecidos (roles BD)
    /// sin acoplar los controllers/servicios a HttpContext.User directamente.
    /// 
    /// Principios SOLID aplicados:
    /// - SRP: Solo responsable de exponer datos del usuario autenticado.
    /// - DIP: Controllers dependen de esta abstracción, no de infraestructura HTTP.
    /// - ISP: Expone únicamente lo que los consumers necesitan.
    /// </summary>
    public interface ICurrentUserService
    {
        /// <summary>
        /// ID único global del usuario en Azure AD (claim 'oid').
        /// Null si el usuario no está autenticado o el claim no existe.
        /// </summary>
        string? UserId { get; }

        /// <summary>
        /// UserID real del usuario en la base de datos (tabla Usuarios).
        /// Es el que debes usar en los parámetros @userId de los SPs.
        /// Null si el usuario no existe en BD o no está autenticado.
        /// </summary>
        string? DbUserId { get; }

        /// <summary>
        /// Email corporativo del usuario (claim 'preferred_username' o 'email').
        /// Null si el usuario no está autenticado.
        /// </summary>
        string? Email { get; }

        /// <summary>
        /// Nombre completo del usuario (claim 'name').
        /// Null si el usuario no está autenticado o el claim no existe.
        /// </summary>
        string? FullName { get; }

        /// <summary>
        /// Indica si el usuario actual está autenticado (JWT válido).
        /// </summary>
        bool IsAuthenticated { get; }

        /// <summary>
        /// Lista de roles de BD asignados al usuario (claims 'role_bd').
        /// Lista vacía si no tiene roles o no está autenticado.
        /// Inyectados por <see cref="UserClaimsTransformation"/> desde la BD.
        /// </summary>
        IReadOnlyList<string> Roles { get; }

        /// <summary>
        /// Verifica si el usuario tiene un rol específico de BD.
        /// </summary>
        /// <param name="role">Nombre del rol a verificar (debe coincidir con <see cref="RoleConstants"/>).</param>
        /// <returns>True si el usuario tiene el rol; false en caso contrario.</returns>
        bool IsInRole(string role);
    }
}
