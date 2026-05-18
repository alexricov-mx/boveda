using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Serilog;
using System;
using System.Linq;
using System.Net;
using BERecepcion.Core.Exceptions;
using BERecepcion.Api.Extensions;

namespace BERecepcion.Api.Filters
{
    /// <summary>
    /// Filtro global que captura excepciones del dominio y las convierte en respuestas
    /// HTTP con cuerpo <see cref="ProblemDetails"/> RFC 7807.
    /// Los status codes son idénticos a los anteriores; solo cambia el formato del cuerpo.
    /// </summary>
    public class GlobalExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            var exception = context.Exception;

            Log.Error(exception, "Error en {Controller}.{Action}: {Message}",
                context.RouteData.Values["controller"],
                context.RouteData.Values["action"],
                exception.Message);

            context.Result = exception switch
            {
                NotFoundException notFoundEx => new NotFoundObjectResult(new ProblemDetails
                {
                    Type = ResultExtensions.ProblemTypes.NotFound,
                    Title  = "Recurso no encontrado",
                    Detail = notFoundEx.Message,
                    Status = (int)HttpStatusCode.NotFound,
                    Extensions =
                    {
                        ["resourceName"] = notFoundEx.ResourceName,
                        ["resourceId"]   = notFoundEx.ResourceId
                    }
                }),

                ValidationException validationEx => new BadRequestObjectResult(new ValidationProblemDetails
                {               
                    Type = ResultExtensions.ProblemTypes.Validation,
                    Title  = "Errores de validación",
                    Detail = validationEx.Message,
                    Status = (int)HttpStatusCode.BadRequest,
                    Errors = validationEx.Errors
                        .GroupBy(e => e.Field)
                        .ToDictionary(
                            g => g.Key,
                            g => g.Select(e => e.Message).ToArray())
                }),

                ConflictException conflictEx => new ConflictObjectResult(new ProblemDetails
                {
                    Type = ResultExtensions.ProblemTypes.Conflict,
                    Title  = "Conflicto de recurso",
                    Detail = conflictEx.Message,
                    Status = (int)HttpStatusCode.Conflict,
                    Extensions =
                    {
                        ["resourceName"]  = conflictEx.ResourceName,
                        ["conflictValue"] = conflictEx.ConflictValue
                    }
                }),

                UnauthorizedException unauthorizedEx => new ObjectResult(new ProblemDetails
                {
                    Type = ResultExtensions.ProblemTypes.Unauthorized,
                    Title  = "Acceso denegado",
                    Detail = unauthorizedEx.Message,
                    Status = (int)HttpStatusCode.Forbidden,
                    Extensions =
                    {
                        ["resource"] = unauthorizedEx.Resource,
                        ["action"]   = unauthorizedEx.Action
                    }
                })
                {
                    StatusCode = (int)HttpStatusCode.Forbidden
                },

                BusinessRuleException businessEx => new UnprocessableEntityObjectResult(new ProblemDetails
                {
                    Type = ResultExtensions.ProblemTypes.Unprocessable,
                    Title  = "Regla de negocio violada",
                    Detail = businessEx.Message,
                    Status = (int)HttpStatusCode.UnprocessableEntity,
                    Extensions =
                    {
                        ["ruleName"]  = businessEx.RuleName,
                        ["ruleValue"] = businessEx.RuleValue
                    }
                }),

                _ => new ObjectResult(new ProblemDetails
                {
                    Type = ResultExtensions.ProblemTypes.ServerError,
                    Title  = "Error interno del servidor",
                    Detail = "Por favor contacte al administrador.",
                    Status = (int)HttpStatusCode.InternalServerError,
                    Extensions =
                    {
                        ["requestId"] = context.HttpContext.TraceIdentifier,
                        ["timestamp"] = DateTime.UtcNow
                    }
                })
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError
                }
            };

            context.ExceptionHandled = true;
        }
    }
}
