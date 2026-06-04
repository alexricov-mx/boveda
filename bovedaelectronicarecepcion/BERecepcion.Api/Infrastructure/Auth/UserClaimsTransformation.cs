using BERecepcion.Core.Interfaces.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BERecepcion.Api.Infrastructure.Auth
{
    /// <summary>
    /// Enriquece el <see cref="ClaimsPrincipal"/> con los roles de BD después de que el JWT de Azure AD
    /// es validado por el middleware de autenticación.
    ///
    /// ASP.NET Core ejecuta <see cref="IClaimsTransformation"/> automáticamente en cada request
    /// autenticada, antes de que los filtros de autorización evalúen las políticas.
    /// Esto permite usar <c>[Authorize(Policy = PolicyConstants.X)]</c> con roles de BD
    /// sin modificar el JWT original emitido por Azure AD.
    /// </summary>
    public class UserClaimsTransformation : IClaimsTransformation
    {
        private readonly IBackendUserService _userService;
        private readonly ILogger<UserClaimsTransformation> _logger;

        public UserClaimsTransformation(IBackendUserService userService, ILogger<UserClaimsTransformation> logger)
        {
            _userService = userService;
            _logger      = logger;
        }

        public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            // Solo enriquecer si el usuario ya está autenticado con JWT
            if (principal?.Identity?.IsAuthenticated != true)
                return principal;

            // Evitar doble enriquecimiento si los claims de BD ya están presentes
            // (IClaimsTransformation puede ser llamado más de una vez por request en algunos escenarios)
            if (principal.HasClaim(c => c.Type == ApiAuthConstants.RoleBdClaim))
                return principal;

            var email = principal.Claims
                .FirstOrDefault(c => c.Type == ApiAuthConstants.PreferredUsernameClaim)?.Value
                ?? principal.Claims.FirstOrDefault(c => c.Type == ApiAuthConstants.EmailSchemaClaim)?.Value
                ?? principal.Claims.FirstOrDefault(c => c.Type == ApiAuthConstants.EmailClaim)?.Value
                ?? principal.Claims.FirstOrDefault(c => c.Type == ApiAuthConstants.UniqueNameClaim)?.Value;

            if (string.IsNullOrWhiteSpace(email))
            {
                _logger.LogWarning("UserClaimsTransformation: no se encontró claim de email en el principal.");
                return principal;
            }

            var roles = await _userService.GetUserRolesAsync(email);

            if (!roles.Any())
                return principal;

            // Agregar los claims de BD en una nueva identidad (no mutamos el principal original)
            var claimsIdentity = new ClaimsIdentity();
            foreach (var role in roles)
            {
                claimsIdentity.AddClaim(new Claim(ApiAuthConstants.RoleBdClaim, role));
            }

            principal.AddIdentity(claimsIdentity);
            _logger.LogDebug("UserClaimsTransformation: {Count} roles de BD agregados para {Email}.", roles.Count(), email);

            return principal;
        }
    }
}
