using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace BERRecepcion.Front.Controllers
{
    /// <summary>
    /// Endpoints para la app Vue (BER.Front).
    /// Solo requiere autenticación Azure AD — no aplica ValidateUserAttribute
    /// para no bloquear la obtención del token por estado de BD.
    /// </summary>
    // [Authorize]
    public class BerFrontController : Controller
    {
        private readonly ITokenAcquisition _tokenAcquisition;
        private readonly IConfiguration _configuration;

        public BerFrontController(ITokenAcquisition tokenAcquisition, IConfiguration configuration)
        {
            _tokenAcquisition = tokenAcquisition;
            _configuration = configuration;
        }

        /// <summary>
        /// Devuelve el access token del usuario autenticado en el MVC.
        /// La app Vue llama este endpoint (mismo origen) en lugar de usar
        /// ssoSilent(), que falla en Chrome por restricciones de cookies en iframe.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Token()
        {
            try
            {
                var scopes = (_configuration["AzureAd:Scopes"] ?? string.Empty)
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries);

                var accessToken = await _tokenAcquisition.GetAccessTokenForUserAsync(scopes);

                var name = User.FindFirst("name")?.Value
                    ?? User.FindFirst(ClaimTypes.GivenName)?.Value
                    ?? User.FindFirst(ClaimTypes.Name)?.Value
                    ?? string.Empty;

                var email = User.FindFirst("preferred_username")?.Value
                    ?? User.FindFirst(ClaimTypes.Email)?.Value
                    ?? string.Empty;

                return Json(new { accessToken, name, email });
            }
            catch (MicrosoftIdentityWebChallengeUserException)
            {
                // Token vencido sin refresh token — el usuario debe volver a loguearse en MVC
                return Unauthorized(new { error = "session_expired" });
            }
            catch (Exception ex)
            {
                Serilog.Log.Warning(ex, "[BerFront] No se pudo adquirir token para la app Vue");
                return Unauthorized(new { error = "token_unavailable" });
            }
        }
    }
}

