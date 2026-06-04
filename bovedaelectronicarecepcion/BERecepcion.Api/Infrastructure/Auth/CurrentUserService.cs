using BERecepcion.Core.Interfaces.Auth;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace BERecepcion.Api.Infrastructure.Auth
{
    /// <summary>
    /// Implementación de <see cref="ICurrentUserService"/> que lee los claims del usuario autenticado
    /// desde el <see cref="ClaimsPrincipal"/> actual del HttpContext.
    /// 
    /// Ciclo de vida: Scoped (una instancia por request HTTP).
    /// 
    /// Principios SOLID aplicados:
    /// - SRP: Solo lee claims del principal, no consulta BD ni transforma datos.
    /// - DIP: Depende de IHttpContextAccessor (abstracción de ASP.NET Core).
    /// - Clean Code: Sin magic strings (usa ApiAuthConstants), lazy initialization.
    /// </summary>
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IBackendUserService _backendUserService;

        // Lazy initialization: solo se leen los claims una vez por request
        private string? _userId;
        private string? _dbUserId;
        private string? _email;
        private string? _fullName;
        private IReadOnlyList<string>? _roles;
        private bool _isInitialized;
        private bool _isDbInitialized;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor
            , IBackendUserService backendUserService
            )
        {
            _httpContextAccessor = httpContextAccessor;
            _backendUserService = backendUserService;
        }

        /// <inheritdoc/>
        public string? UserId
        {
            get
            {
                EnsureInitialized();
                return _userId;
            }
        }

        /// <inheritdoc/>
        public string? Email
        {
            get
            {
                EnsureInitialized();
                return _email;
            }
        }

        /// <inheritdoc/>
        public string? FullName
        {
            get
            {
                EnsureInitialized();
                return _fullName;
            }
        }

        /// <inheritdoc/>
        public bool IsAuthenticated
        {
            get
            {
                var user = _httpContextAccessor.HttpContext?.User;
                return user?.Identity?.IsAuthenticated ?? false;
            }
        }

        /// <inheritdoc/>
        public IReadOnlyList<string> Roles
        {
            get
            {
                EnsureInitialized();
                return _roles ?? [];
            }
        }

        public string DbUserId
        {
            get
            {
                EnsureInitialized();
                if (_isDbInitialized) return _dbUserId;

                // Resolución sincrónica segura: BackendUserService ya tiene el dato
                // en caché porque UserClaimsTransformation lo llamó primero.
                var user = _backendUserService
                    .GetUserDataAsync(_email ?? string.Empty)
                    .GetAwaiter().GetResult();

                _dbUserId = user?.UserID.ToString();
                _isDbInitialized = true;
                return _dbUserId;
            }
        }

        /// <inheritdoc/>
        public bool IsInRole(string role)
        {
            if (string.IsNullOrWhiteSpace(role))
                return false;

            return Roles.Contains(role);
        }

        private void EnsureInitialized()
        {
            if (_isInitialized)
                return;

            var user = _httpContextAccessor.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated != true)
            {
                // Usuario no autenticado: valores por defecto
                _userId = null;
                _email = null;
                _fullName = null;
                _roles = [];
                _isInitialized = true;
                return;
            }

            // UserId: 'oid' claim (Azure AD Object ID)
            // Fallback a claim largo de WS-Federation por compatibilidad con tokens antiguos
            _userId = user.FindFirstValue(ApiAuthConstants.OidClaim)
                      ?? user.FindFirstValue("http://schemas.microsoft.com/identity/claims/objectidentifier");

            // Email: 'preferred_username' (UPN) o 'email'
            _email = user.FindFirstValue(ApiAuthConstants.PreferredUsernameClaim)
                     ?? user.FindFirstValue(ApiAuthConstants.EmailSchemaClaim)
                     ?? user.FindFirstValue(ApiAuthConstants.EmailClaim)
                     ?? user.FindFirstValue(ApiAuthConstants.UniqueNameClaim);
                

            // Nombre completo: 'name' claim
            _fullName = user.FindFirstValue(ApiAuthConstants.NameClaim);

            // Roles de BD: todos los claims 'role_bd' inyectados por UserClaimsTransformation
            _roles = user.Claims
                .Where(c => c.Type == ApiAuthConstants.RoleBdClaim)
                .Select(c => c.Value)
                .ToList()
                .AsReadOnly();

            _isInitialized = true;
        }
    }
}
