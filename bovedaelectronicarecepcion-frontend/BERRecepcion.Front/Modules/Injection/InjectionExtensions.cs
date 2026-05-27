using BERRecepcion.Front.Interfaces;
using BERRecepcion.Front.Interfaces.Services.BackEndApi.EstimacionObra;
using BERRecepcion.Front.Interfaces.Services.BackEndApi.OrdenSurtimiento;
using BERRecepcion.Front.Services.BackEndApi.EstimacionObra;
using BERRecepcion.Front.Services.BackEndApi.OrdenSurtimiento;
using BERRecepcion.Front.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace BERRecepcion.Front.Modules.Injection;

public static class InjectionExtensions
{
    public static IServiceCollection AddInjection(this IServiceCollection services)
    {
        services.AddTransient<IRestUtility, RestUtility>();
        services.AddSingleton<IGenerals, Generals>();
        services.AddScoped<IOrdenSurtimiento, OrdenSurtimiento>();
        services.AddScoped<IEstimacionObra, EstimacionObra>();

        return services;
    }
}