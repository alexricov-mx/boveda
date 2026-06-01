using System.Collections.Generic;
using System.Threading.Tasks;

namespace BERecepcion.Api.Infrastructure.Auth
{
    /// <summary>
    /// Abstracción para obtener los roles de BD de un usuario autenticado.
    /// Permite que <see cref="UserClaimsTransformation"/> sea independiente de la infraestructura concreta.
    /// </summary>
    public interface IBackendUserService
    {
        /// <summary>
        /// Retorna los roles asignados al usuario en la BD según su email.
        /// Retorna lista vacía si el usuario no existe, está inactivo o no tiene roles.
        /// </summary>
        /// <param name="email">Email corporativo del usuario (preferred_username de Azure AD).</param>
        Task<IEnumerable<string>> GetUserRolesAsync(string email);
    }
}
