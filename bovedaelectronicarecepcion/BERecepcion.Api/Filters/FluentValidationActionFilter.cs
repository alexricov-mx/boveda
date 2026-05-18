using BERecepcion.Api.Extensions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace BERecepcion.Api.Filters;

/// <summary>
/// Filtro de acción que ejecuta los validadores de FluentValidation 12 registrados en DI
/// para los argumentos del action. Devuelve <see cref="ValidationProblemDetails"/> RFC 7807,
/// coherente con <see cref="GlobalExceptionFilter"/> y <c>InvalidModelStateResponseFactory</c>.
/// </summary>
public class FluentValidationActionFilter : IAsyncActionFilter
{
    private readonly IServiceProvider _serviceProvider;

    public FluentValidationActionFilter(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument is null) continue;

            var validatorType = typeof(IValidator<>).MakeGenericType(argument.GetType());
            if (_serviceProvider.GetService(validatorType) is IValidator validator)
            {
                var validationContext = new ValidationContext<object>(argument);
                var result = await validator.ValidateAsync(validationContext);

                if (!result.IsValid)
                {
                    var errors = result.Errors
                        .GroupBy(e => e.PropertyName)
                        .ToDictionary(
                            g => g.Key,
                            g => g.Select(e => e.ErrorMessage).ToArray());

                    context.Result = new BadRequestObjectResult(new ValidationProblemDetails(errors)
                    {
                        Type   = ResultExtensions.ProblemTypes.Validation,
                        Title  = "Errores de validación",
                        Detail = "Uno o más errores de validación ocurrieron.",
                        Status = (int)HttpStatusCode.BadRequest
                    });
                    return;
                }
            }
        }

        await next();
    }
}
