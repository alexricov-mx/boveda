using BERecepcion.Core.Admin.Interfaces.Repositories;
using BERecepcion.Core.Consulta.Interfaces.Repositories;
using BERecepcion.Core.Interfaces;
using BERecepcion.Core.Interfaces.Repositories;
using BERecepcion.Core.Options;
using BERecepcion.Core.OrdenSurtimiento.Interfaces.Repositories;
using BERecepcion.Core.Services;
using BERecepcion.Infraestructura.Admin.Repositories;
using BERecepcion.Infraestructura.Consulta.Repositories;
using BERecepcion.Infraestructura.OrdenSurtimiento.Repositories;
using BERecepcion.Infraestructura.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BERecepcion.Infraestructura.StartupExtensions;

/// <summary>
/// Extension method para registrar la capa de Infraestructura en el contenedor DI.
/// Reemplaza el patrón new Repositorio(connectionString) disperso en Program.cs.
/// Agregar aquí los repositorios conforme avance el refactor global.
/// </summary>
public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // 1. Bind options tipados desde appsettings.json → sección ConnectionStrings
        services.Configure<ConnectionStringsOptions>(
            configuration.GetSection(ConnectionStringsOptions.SectionName));

        // 2. Factory de conexiones SQL Server — Scoped: una instancia por request HTTP
        services.AddScoped<IDbConnectionFactory, SqlServerConnectionFactory>();

        // 3. UnitOfWork — Scoped: comparte ciclo de vida con la factory
        services.AddScoped<IUnitOfWork, DapperUnitOfWork>();

        // 4. Repositorios migrados — Módulo OrdenSurtimiento
        services.AddScoped<ISOEstimationRepository, SOEstimacionRepository>();
        services.AddScoped<ISupplyOrderRepository, SupplyOrderRepository>();
        services.AddScoped<IConsultasRepository, ConsultasRepository>();
        services.AddScoped<IReactivaProcesosRepository, ReactivaProcesosRepository>();
        services.AddScoped<IExpedienteElectronicoRepositoryAsync, ExpedienteElectronicoRepository>();


        // 5. Services — lógica de negocio OrdenSurtimiento
        services.AddScoped<ISoEstimacionServiceAsync, SoEstimacionServiceAsync>();
        services.AddScoped<IConsultaServiceAsync, ConsultaServiceAsync>();
        services.AddScoped<ISupplyOrderServiceAsync, SupplyOrderServiceAsync>();
        services.AddScoped<IReactivaProcesoServiceAsync, ReactivaProcesoServiceAsync>();

        // 6. Services — Módulo ExpedienteElectronico
        services.AddScoped<IExpedienteElectronicoServiceAsync, ExpedienteElectronicoServiceAsync>();
        return services;
    }
}
