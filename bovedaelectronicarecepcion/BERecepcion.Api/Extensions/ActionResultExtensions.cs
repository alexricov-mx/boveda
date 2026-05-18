using BERecepcion.Core.Common.Enums;
using BERecepcion.Core.Common.Results;
using BERecepcion.Core.Dto;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace BERecepcion.Api.Extensions
{
    /// <summary>
    /// Extension methods para convertir DataResult a ActionResult con códigos HTTP apropiados.
    /// </summary>
    public static class ActionResultExtensions
    {
        /// <summary>
        /// Convierte un DataResult en el ActionResult apropiado basado en el HttpStatusCode.
        /// </summary>
        public static IActionResult ToActionResult<T>(this DataResult<T> result)
        {
            return result.Status switch
            {
                HttpStatusCode.OK => new OkObjectResult(new
                {
                    message = result.Message,
                    data = result.Data,
                    status = result.Status
                }),

                HttpStatusCode.Created => new CreatedResult(string.Empty, new
                {
                    message = result.Message,
                    data = result.Data,
                    status = result.Status
                }),

                HttpStatusCode.NoContent => new NoContentResult(),

                HttpStatusCode.BadRequest => new BadRequestObjectResult(new
                {
                    message = result.Message
                }),

                HttpStatusCode.NotFound => new NotFoundObjectResult(new
                {
                    message = result.Message
                }),

                HttpStatusCode.Conflict => new ConflictObjectResult(new
                {
                    message = result.Message
                }),

                HttpStatusCode.Unauthorized => new UnauthorizedObjectResult(new
                {
                    message = result.Message
                }),

                HttpStatusCode.Forbidden => new ObjectResult(new
                {
                    message = result.Message
                })
                {
                    StatusCode = (int)HttpStatusCode.Forbidden
                },

                HttpStatusCode.UnprocessableEntity => new UnprocessableEntityObjectResult(new
                {
                    message = result.Message
                }),

                _ => new ObjectResult(new
                {
                    message = result.Message ?? "Error al procesar la solicitud"
                })
                {
                    StatusCode = (int)result.Status
                }
            };
        }

        /// <summary>
        /// Convierte un DataResult a ActionResult solo con el dato (sin wrapper).
        /// </summary>
        public static IActionResult ToActionResultData<T>(this DataResult<T> result)
        {
            return result.Status switch
            {
                HttpStatusCode.OK => new OkObjectResult(result.Data),
                HttpStatusCode.Created => new CreatedResult(string.Empty, result.Data),
                HttpStatusCode.NoContent => new NoContentResult(),
                HttpStatusCode.BadRequest => new BadRequestObjectResult(new { message = result.Message }),
                HttpStatusCode.NotFound => new NotFoundObjectResult(new { message = result.Message }),
                HttpStatusCode.Conflict => new ConflictObjectResult(new { message = result.Message }),
                _ => new ObjectResult(new { message = result.Message }) { StatusCode = (int)result.Status }
            };
        }
    }

    /// <summary>
    /// Extensiones para traducir <see cref="Result{T}"/> de Core a <see cref="IActionResult"/>
    /// con cuerpo <see cref="ProblemDetails"/> RFC 7807.
    /// SOLID SRP: único lugar que conoce el mapeo ErrorType → HTTP status.
    /// SOLID OCP: nuevo ErrorType = agregar una rama aquí, sin tocar controllers.
    /// </summary>
    public static class ResultExtensions
    {
        /// <summary>
        /// Éxito → 200 OK con el valor.<br/>
        /// Fallo → <see cref="ProblemDetails"/> con status HTTP derivado de <see cref="ErrorType"/>.
        /// </summary>
        public static IActionResult ToActionResult<T>(this Result<T> result, ControllerBase controller)
        {
            if (result.IsSuccess)
                return controller.Ok(result.Value);

            return result.Error.Type switch
            {
                ErrorType.Validation => controller.BadRequest(BuildProblem(result.Error, 400, ProblemTypes.Validation)),
                ErrorType.NotFound => controller.NotFound(BuildProblem(result.Error, 404, ProblemTypes.NotFound)),
                ErrorType.Conflict => controller.Conflict(BuildProblem(result.Error, 409, ProblemTypes.Conflict)),
                ErrorType.Unauthorized => new ObjectResult(BuildProblem(result.Error, 401, ProblemTypes.Unauthorized)) { StatusCode = 401 },
                ErrorType.Forbidden => new ObjectResult(BuildProblem(result.Error, 403, ProblemTypes.Forbidden)) { StatusCode = 403 },
                _ => new ObjectResult(BuildProblem(result.Error, 500, ProblemTypes.ServerError)) { StatusCode = 500 }
            };
        }

        private static ProblemDetails BuildProblem(Error error, int status, string type) => new()
        {
            Type = type,
            Title = error.Code,
            Detail = error.Description,
            Status = status,
        };

        /// <summary>
        /// URIs canónicas RFC 7807 §3.1 + RFC 9110.
        /// Referencia: https://datatracker.ietf.org/doc/html/rfc9110
        /// </summary>
        internal static class ProblemTypes
        {
            public const string Validation = "https://tools.ietf.org/html/rfc9110#section-15.5.1";   // 400
            public const string Unauthorized = "https://tools.ietf.org/html/rfc9110#section-15.5.2";   // 401
            public const string Forbidden = "https://tools.ietf.org/html/rfc9110#section-15.5.4";   // 403
            public const string NotFound = "https://tools.ietf.org/html/rfc9110#section-15.5.5";   // 404
            public const string Conflict = "https://tools.ietf.org/html/rfc9110#section-15.5.10";  // 409
            public const string Unprocessable = "https://tools.ietf.org/html/rfc9110#section-15.5.21";  // 422
            public const string ServerError = "https://tools.ietf.org/html/rfc9110#section-15.6.1";   // 500
        }
    }
}
