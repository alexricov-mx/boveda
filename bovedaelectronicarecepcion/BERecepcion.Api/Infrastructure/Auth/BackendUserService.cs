using BERecepcion.Core.Admin.Dto;
using BERecepcion.Core.Interfaces.Auth;
using BERecepcion.Core.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace BERecepcion.Api.Infrastructure.Auth
{
    /// <summary>
    /// Obtiene los roles de BD del usuario autenticado consultando <see cref="ILoginRepository"/>.
    /// Se registra como Scoped para que el ciclo de vida coincida con el de la request,
    /// garantizando que la consulta a BD ocurra una sola vez por request (caché por request).
    /// </summary>
    public class BackendUserService : IBackendUserService
    {
        private readonly ILoginRepository _loginRepository;
        private readonly ILogger<BackendUserService> _logger;

        // Caché por request: evita múltiples consultas a BD si IClaimsTransformation
        // se invoca más de una vez dentro de la misma request.
        private IEnumerable<string> _cachedRoles;
        private UsersDto? _cachedUser;
        private string _cachedEmail;

        public BackendUserService(ILoginRepository loginRepository, ILogger<BackendUserService> logger)
        {
            _loginRepository = loginRepository;
            _logger          = logger;
        }

        public async Task<UsersDto?> GetUserDataAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return null;

            // Retornar caché si ya se consultó en esta request
            if (_cachedEmail == email && _cachedUser is not null)
                return _cachedUser;

            try
            {
                var result = await _loginRepository.GetUsuarioAsync(email);

                if (result?.Status != HttpStatusCode.OK || result.Data == null)
                {
                    _logger.LogWarning("BackendUserService: usuario {Email} no válido en BD (Status={Status}).",
                        email, result?.Status);

                    _cachedEmail = email;
                    _cachedUser = null;
                    _cachedRoles = Enumerable.Empty<string>();
                    return null;
                }

                _cachedUser = result.Data;
                _cachedEmail = email;
                _cachedRoles = result.Data.Profile?.RolesCatalogo?
                    .Select(r => r.Rol)
                    .Where(r => !string.IsNullOrWhiteSpace(r))
                    .ToList()
                    ?? [];

                _logger.LogDebug("BackendUserService: {Email} → DbUserId={DbUserId}, Roles={Count}.",
                    email, _cachedUser.UserID, _cachedRoles.Count());

                return _cachedUser;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "BackendUserService: error consultando BD para {Email}.", email);
                _cachedEmail = email;
                _cachedUser = null;
                _cachedRoles = Enumerable.Empty<string>();
                return null;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<string>> GetUserRolesAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return Enumerable.Empty<string>();

            // Retornar caché si el email ya fue consultado en esta request
            if (_cachedEmail == email && _cachedRoles != null)
                return _cachedRoles;

            try
            {
                var result = await _loginRepository.GetUsuarioAsync(email);

                if (result?.Status != HttpStatusCode.OK || result.Data == null)
                {
                    _logger.LogWarning("BackendUserService: usuario {Email} no válido en BD (Status={Status}).", email, result?.Status);
                    _cachedEmail = email;
                    _cachedRoles = Enumerable.Empty<string>();
                    return _cachedRoles;
                }

                var roles = result.Data.Profile?.RolesCatalogo?
                    .Select(r => r.Rol)
                    .Where(r => !string.IsNullOrWhiteSpace(r))
                    .ToList()
                    ?? new List<string>();

                _logger.LogDebug("BackendUserService: {Email} tiene {Count} roles en BD.", email, roles.Count);

                _cachedEmail = email;
                _cachedRoles = roles;
                return _cachedRoles;
            }
            catch (Exception ex)
            {
                // No propagar la excepción — si la BD falla, el usuario simplemente
                // no tendrá claims de rol y las políticas [Authorize] lo rechazarán correctamente.
                _logger.LogError(ex, "BackendUserService: error consultando roles para {Email}.", email);
                _cachedEmail = email;
                _cachedRoles = Enumerable.Empty<string>();
                return _cachedRoles;
            }
        }
    }
}
