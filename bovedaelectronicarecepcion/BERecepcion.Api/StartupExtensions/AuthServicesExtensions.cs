using BERecepcion.Api.Infrastructure.Auth;
using BERecepcion.Core.Interfaces.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace BERecepcion.Api.StartupExtensions;

public static class AuthServicesExtensions
{
    // Scoped: ciclo de vida por request, alineado con IClaimsTransformation y los repositorios
    public static IServiceCollection AddAuthServicesConfig(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IBackendUserService, BackendUserService>();
        services.AddScoped<IClaimsTransformation, UserClaimsTransformation>();

        return services;
    }
}
