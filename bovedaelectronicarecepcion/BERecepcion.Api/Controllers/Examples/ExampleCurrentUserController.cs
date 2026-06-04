using BERecepcion.Api.Infrastructure.Auth;
using BERecepcion.Core.Interfaces.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BERecepcion.Api.Controllers.Examples
{
    /// <summary>
    /// 📘 EJEMPLO DE USO: ICurrentUserService en Controllers
    /// 
    /// Este controlador demuestra cómo usar el servicio ICurrentUserService para:
    /// - Acceder a los datos del usuario autenticado sin depender directamente de HttpContext.User
    /// - Implementar validaciones basadas en roles de BD
    /// - Respetar SOLID: SRP (Single Responsibility) + DIP (Dependency Inversion)
    /// 
    /// ✅ VENTAJAS:
    /// - Testeable: se puede mockear fácilmente en pruebas unitarias
    /// - Desacoplado: no depende de HttpContext directamente
    /// - Tipado: propiedades fuertemente tipadas (UserId, Email, Roles, etc.)
    /// - Caché: los claims se leen una vez por request
    /// 
    /// 🔐 AUTENTICACIÓN + AUTORIZACIÓN:
    /// - [Authorize]: valida que el JWT sea válido (autenticación)
    /// - Policy: valida que el usuario tenga el rol de BD requerido (autorización)
    /// - ICurrentUserService: permite leer los datos del usuario en la lógica del endpoint
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Requiere JWT válido (autenticación mínima)
    public class ExampleCurrentUserController : ControllerBase
    {
        private readonly ICurrentUserService _currentUser;

        /// <summary>
        /// Constructor con inyección de dependencias.
        /// ICurrentUserService se registra como Scoped en Program.cs.
        /// </summary>
        public ExampleCurrentUserController(ICurrentUserService currentUser)
        {
            _currentUser = currentUser;
        }

        /// <summary>
        /// ✅ Ejemplo 1: Endpoint protegido con política de autorización específica.
        /// Solo usuarios con el rol "AdministrationUsers" pueden acceder.
        /// </summary>
        [HttpGet("admin-only")]
        [Authorize(Policy = PolicyConstants.RequireAdministrationUsers)]
        public IActionResult GetAdminData()
        {
            // La política ya validó que el usuario tiene el rol requerido;
            // ahora usamos ICurrentUserService para leer sus datos
            return Ok(new
            {
                Message = "Acceso autorizado para administradores",
                UserId = _currentUser.UserId,
                Email = _currentUser.Email,
                FullName = _currentUser.FullName,
                Roles = _currentUser.Roles
            });
        }

        /// <summary>
        /// ✅ Ejemplo 2: Endpoint con autorización base (solo autenticado).
        /// Verifica manualmente el rol dentro del endpoint.
        /// </summary>
        [HttpGet("check-role")]
        [Authorize(Policy = PolicyConstants.RequireAuthenticatedUser)]
        public IActionResult CheckRole([FromQuery] string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
                return BadRequest("El parámetro 'roleName' es requerido.");

            var hasRole = _currentUser.IsInRole(roleName);

            return Ok(new
            {
                UserId = _currentUser.UserId,
                Email = _currentUser.Email,
                QueriedRole = roleName,
                HasRole = hasRole,
                AllRoles = _currentUser.Roles
            });
        }

        /// <summary>
        /// ✅ Ejemplo 3: Endpoint que retorna el perfil del usuario autenticado.
        /// Útil para que el frontend muestre datos del usuario en la UI.
        /// </summary>
        [HttpGet("me")]
        [Authorize(Policy = PolicyConstants.RequireAuthenticatedUser)]
        public IActionResult GetCurrentUserProfile()
        {
            if (!_currentUser.IsAuthenticated)
                return Unauthorized("No hay usuario autenticado.");

            return Ok(new
            {
                UserId = _currentUser.UserId,
                Email = _currentUser.Email,
                FullName = _currentUser.FullName,
                IsAuthenticated = _currentUser.IsAuthenticated,
                Roles = _currentUser.Roles
            });
        }

        /// <summary>
        /// ✅ Ejemplo 4: Validación condicional basada en múltiples roles.
        /// La lógica de negocio decide qué permitir según los roles del usuario.
        /// </summary>
        [HttpPost("conditional-action")]
        [Authorize(Policy = PolicyConstants.RequireAuthenticatedUser)]
        public IActionResult ConditionalAction()
        {
            // Validación multi-rol
            var canExecuteCxp = _currentUser.IsInRole(RoleConstants.AdministrationCxpExecution);
            var canExecuteCxpAP = _currentUser.IsInRole(RoleConstants.AdministrationCxpExecutionAP);

            if (!canExecuteCxp && !canExecuteCxpAP)
            {
                return Forbid(); // 403 Forbidden
            }

            // Lógica de negocio que requiere uno de estos roles
            return Ok(new
            {
                Message = "Acción autorizada",
                ExecutedBy = _currentUser.Email,
                AvailableRoles = _currentUser.Roles
            });
        }

        /// <summary>
        /// ✅ Ejemplo 5: Endpoint donde se pasa el UserId a un servicio de dominio.
        /// El controlador NO accede a HttpContext.User directamente.
        /// </summary>
        [HttpPost("business-action")]
        [Authorize(Policy = PolicyConstants.RequireReceptionElectronicInvoice)]
        public IActionResult BusinessAction([FromBody] BusinessActionRequest request)
        {
            // Pasar el UserId del usuario autenticado al servicio de dominio
            // El servicio NO debe depender de HttpContext ni de infraestructura HTTP
            var userId = _currentUser.UserId;
            var userEmail = _currentUser.Email;

            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized("El usuario autenticado no tiene un ID válido.");

            // Simular llamada a un servicio de negocio
            // var result = await _someBusinessService.ProcessAsync(request, userId, userEmail);

            return Ok(new
            {
                Message = "Acción de negocio ejecutada",
                ProcessedBy = userEmail,
                UserId = userId,
                Request = request
            });
        }
    }

    /// <summary>Ejemplo de DTO para el Ejemplo 5.</summary>
    public class BusinessActionRequest
    {
        public string Data { get; set; } = string.Empty;
    }
}
