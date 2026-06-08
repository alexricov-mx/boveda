using BERecepcion.Api.ModelBinding;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Reflection;

namespace BERecepcion.Api.StartupExtensions;

public static class ControllersExtensions
{
    public static IServiceCollection AddControllersConfig(this IServiceCollection services)
    {
        services.AddControllers(options =>
        {
            options.Filters.Add<Filters.GlobalExceptionFilter>();
            // [FV12] Registrar filtro de auto-validación (reemplaza AddFluentValidationAutoValidation de FV11)
            options.Filters.Add<Filters.FluentValidationActionFilter>();
        });

        // [FV12] Escaneo de validators — BERecepcion.Api
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        // [FV12] Escaneo de validators — BERecepcion.Core completo (auto-descubre todo AbstractValidator<T>)
        // Tipo ancla: ValidationException vive en BERecepcion.Core.Exceptions
        services.AddValidatorsFromAssemblyContaining<BERecepcion.Core.Exceptions.ValidationException>();

        // Unificar el formato de errores de model-state con GlobalExceptionFilter y FluentValidationActionFilter.
        // Los tres caminos retornan ValidationProblemDetails RFC 7807.
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var errors = context.ModelState
                    .Where(e => e.Value?.Errors.Count > 0)
                    .ToDictionary(
                        e => e.Key,
                        e => e.Value!.Errors.Select(err => err.ErrorMessage).ToArray());

                return new BadRequestObjectResult(new ValidationProblemDetails(errors)
                {
                    Title = "Errores de validación",
                    Detail = "Uno o más errores de validación ocurrieron.",
                    Status = 400
                });
            };
        });

        // ModelBinder personalizado para JSON
        services.AddMvc(properties =>
        {
            properties.ModelBinderProviders.Insert(0, new JsonModelBinderProvider());
        });

        return services;
    }
}
