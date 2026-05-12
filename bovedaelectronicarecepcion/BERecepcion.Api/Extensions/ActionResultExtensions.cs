using Microsoft.AspNetCore.Mvc;
using System.Net;
using BERecepcion.Core.Models;
using BERecepcion.Core.Dto;

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
}
