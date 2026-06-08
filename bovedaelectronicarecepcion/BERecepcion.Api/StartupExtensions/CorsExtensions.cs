using Microsoft.Extensions.DependencyInjection;

namespace BERecepcion.Api.StartupExtensions;

public static class CorsExtensions
{
    public const string PolicyName = "_myAllowSpecificOrigins";

    public static IServiceCollection AddCorsConfig(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy(name: PolicyName, builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            });
        });

        return services;
    }
}
