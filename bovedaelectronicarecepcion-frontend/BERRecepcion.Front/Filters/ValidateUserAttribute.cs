using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Serilog;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BERRecepcion.Front.Filters
{
    public class ValidateUserAttribute : Attribute, IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var controller = context.RouteData.Values["controller"]?.ToString();
            var action = context.RouteData.Values["action"]?.ToString();
            var isAuthenticated = context.HttpContext.User.Identity.IsAuthenticated;
            var hasUserClaim = context.HttpContext.User.FindFirst("User") != null;
            var hasSession = context.HttpContext.Session.Keys.Any(x => x.Equals("UserMenu"));
            
            Serilog.Log.Information($"ValidateUserAttribute ejecutándose - Controller={controller}, Action={action}, IsAuthenticated={isAuthenticated}, HasUserClaim={hasUserClaim}, HasSession={hasSession}");
            
            //Valida si la sesión del usuario caducó y se encuentra fuera del Home
            if (!context.RouteData.Values.FirstOrDefault().Value.Equals("Home") && context.HttpContext.Session.Keys.Where(x => x.Equals("UserMenu")).FirstOrDefault() == null)
            {
                Serilog.Log.Warning($"ValidateUserAttribute - Sesión caducada, redirigiendo a Home");
                context.Result = new RedirectToActionResult("Index", "Home", null);
                return;
            }
            //Valida que existan los identities del usuario
            if (context.HttpContext.User.Identities.FirstOrDefault() != null && string.IsNullOrWhiteSpace(context.HttpContext.User.Identities.Select(x => x.AuthenticationType).FirstOrDefault()))
            {
                Serilog.Log.Information($"ValidateUserAttribute - Sin AuthenticationType, permitiendo acceso");
                await next();
                return;
            }
            if (!context.HttpContext.User.Identity.IsAuthenticated && context.HttpContext.User.FindFirst("User") == null)
            {
                Serilog.Log.Information($"ValidateUserAttribute - No autenticado y sin claim User, permitiendo acceso");
                await next();
                return;
            }

            var loginErrorClaims = context.HttpContext.User.Identities.Where(x => x.AuthenticationType.Equals("LoginError")).FirstOrDefault();
            if(loginErrorClaims != null)
            {
                var error = loginErrorClaims.Claims.Where(x => x.Type.Equals("LoginError")).FirstOrDefault();
                if(error != null && Convert.ToBoolean(error.Value))
                {
                    Serilog.Log.Warning($"ValidateUserAttribute - LoginError detectado, redirigiendo a SignOut");
                    context.Result = new RedirectToRouteResult(new RouteValueDictionary(new { area = "MicrosoftIdentity",  controller = "Account", action = "SignOut" }));
                    return;
                }
            }

            var validationClaims = context.HttpContext.User.Identities.Where(x => x.AuthenticationType.Equals("ValidationClaims")).FirstOrDefault();

            // Si no hay identidad separada ValidationClaims, buscar en la identidad principal
            if (validationClaims == null)
            {
                validationClaims = context.HttpContext.User.Identities.FirstOrDefault();
            }

            if (validationClaims != null)
            {
                var userExists = validationClaims.Claims.Where(x => x.Type.Equals("UserExists")).FirstOrDefault();
                var UserdateIsValid = validationClaims.Claims.Where(x => x.Type.Equals("UserdateIsValid")).FirstOrDefault();

                // DIAGNÓSTICO: Loggear valores de claims
                Serilog.Log.Information($"ValidateUserAttribute - UserExists claim: {(userExists != null ? userExists.Value : "NULL")}, UserdateIsValid claim: {(UserdateIsValid != null ? UserdateIsValid.Value : "NULL")}");

                if (!Convert.ToBoolean(userExists.Value) || !Convert.ToBoolean(UserdateIsValid.Value))
                {
                    Serilog.Log.Warning($"ValidateUserAttribute - Validación FALLIDA: UserExists={userExists.Value}, UserdateIsValid={UserdateIsValid.Value} - Redirigiendo a NoPermissions");
                    context.Result = new RedirectToRouteResult( new RouteValueDictionary(new { controller = "Error", action = "NoPermissions" }));
                    return;
                }
                var userIsBlocked = validationClaims.Claims.Where(x => x.Type.Equals("UserIsBlocked")).FirstOrDefault();
                if (Convert.ToBoolean(userIsBlocked.Value))
                {
                    Serilog.Log.Warning($"ValidateUserAttribute - Usuario bloqueado, redirigiendo a Blocked");
                    context.Result = new RedirectToRouteResult( new RouteValueDictionary(new { controller = "Error", action = "Blocked" }));
                    return;
                }
                var userIsDeleted = validationClaims.Claims.Where(x => x.Type.Equals("UserIsDeleted")).FirstOrDefault();
                if (Convert.ToBoolean(userIsDeleted.Value))
                {
                    Serilog.Log.Warning($"ValidateUserAttribute - Usuario eliminado, redirigiendo a Deleted");
                    context.Result = new RedirectToRouteResult( new RouteValueDictionary(new { controller = "Error", action = "Deleted" }));
                    return;
                }
                var IsSapInterfaceEnabled = validationClaims.Claims.Where(x => x.Type.Equals("IsSapInterfaceEnabled")).FirstOrDefault();
                if (!Convert.ToBoolean(IsSapInterfaceEnabled.Value))
                {
                    Serilog.Log.Warning($"ValidateUserAttribute - Interfaz SAP deshabilitada, redirigiendo a Maintenance");
                    context.Result = new RedirectToRouteResult( new RouteValueDictionary(new { controller = "Error", action = "Maintenance" }));
                    return;
                }
            }
            
            Serilog.Log.Information($"ValidateUserAttribute - Todas las validaciones pasaron, continuando con acción");
            await next();
        }
    }
}
