using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Serilog;
using System;
using System.Net;
using BERecepcion.Core.Exceptions;

namespace BERecepcion.Api.Filters
{
    /// <summary>
    /// Filtro global que captura excepciones y las convierte en respuestas HTTP apropiadas.
    /// </summary>
    public class GlobalExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            var exception = context.Exception;
            
            // Log de la excepción
            Log.Error(exception, "Error en {Controller}.{Action}: {Message}", 
                context.RouteData.Values["controller"],
                context.RouteData.Values["action"],
                exception.Message);

            // Convertir excepción a respuesta HTTP apropiada
            context.Result = exception switch
            {
                NotFoundException notFoundEx => new NotFoundObjectResult(new
                {
                    message = notFoundEx.Message,
                    resourceName = notFoundEx.ResourceName,
                    resourceId = notFoundEx.ResourceId
                }),

                ValidationException validationEx => new BadRequestObjectResult(new
                {
                    message = validationEx.Message,
                    errors = validationEx.Errors
                }),

                ConflictException conflictEx => new ConflictObjectResult(new
                {
                    message = conflictEx.Message,
                    resourceName = conflictEx.ResourceName,
                    conflictValue = conflictEx.ConflictValue
                }),

                UnauthorizedException unauthorizedEx => new ObjectResult(new
                {
                    message = unauthorizedEx.Message,
                    resource = unauthorizedEx.Resource,
                    action = unauthorizedEx.Action
                })
                {
                    StatusCode = (int)HttpStatusCode.Forbidden
                },

                BusinessRuleException businessEx => new UnprocessableEntityObjectResult(new
                {
                    message = businessEx.Message,
                    ruleName = businessEx.RuleName,
                    ruleValue = businessEx.RuleValue
                }),

                _ => new ObjectResult(new
                {
                    message = "Error interno del servidor. Por favor contacte al administrador.",
                    requestId = context.HttpContext.TraceIdentifier,
                    timestamp = DateTime.UtcNow
                })
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError
                }
            };

            context.ExceptionHandled = true;
        }
    }
}
