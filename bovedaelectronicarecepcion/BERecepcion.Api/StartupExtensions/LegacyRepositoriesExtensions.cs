using BERecepcion.Core.Admin.Interfaces.Repositories;
using BERecepcion.Core.Cancelaciones.Interfaces.Repositories;
using BERecepcion.Core.Catalogos.Interfaces.Repositories;
using BERecepcion.Core.Consulta.Interfaces.Repositories;
using BERecepcion.Core.Copades.Interfaces.Repositories;
using BERecepcion.Core.Correos.Interfaces.Repositories;
using BERecepcion.Core.Facturas.Interfaces.Repositories;
using BERecepcion.Core.FirmaDocumentos.Interfaces.Repositories;
using BERecepcion.Core.Instrucciones.Interfaces.Repositories;
using BERecepcion.Core.Interfaces.Repositories;
using BERecepcion.Core.OrdenSurtimiento.Interfaces.Repositories;
using BERecepcion.Core.SAPPI.Interfaces.Repositories;
using BERecepcion.Core.SAT.Interfaces.Repositories;
using BERecepcion.Infraestructura.Admin.Repositories;
using BERecepcion.Infraestructura.Cancelaciones.Repositories;
using BERecepcion.Infraestructura.Catalogos.Repositories;
using BERecepcion.Infraestructura.Consulta.Repositories;
using BERecepcion.Infraestructura.Copades.Repositories;
using BERecepcion.Infraestructura.Correos.Repositories;
using BERecepcion.Infraestructura.Facturas.Repositories;
using BERecepcion.Infraestructura.FirmaDocumentos.Repositories;
using BERecepcion.Infraestructura.Instrucciones.Repositories;
using BERecepcion.Infraestructura.OrdenSurtimiento.Repositories;
using BERecepcion.Infraestructura.Repositories;
using BERecepcion.Infraestructura.SAPPI.Repositories;
using BERecepcion.Infraestructura.SAT.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BERecepcion.Api.StartupExtensions;

public static class LegacyRepositoriesExtensions
{
    public static IServiceCollection AddLegacyRepositories(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var cs = configuration["ConnectionStrings:SQLServerSQLDEV002"];

        services.AddTransient<IAdefasRepository>(_ => new AdefasRepository(cs));
        services.AddTransient<IDesvioFirmasRepository>(_ => new DesvioFirmasRepository(cs));

        services.AddTransient<IOldUsuariosRepository>(_ =>
            new OldUsuariosRepository(cs, configuration, new BitacoraAdmonRepository(cs)));

        services.AddTransient<ILoginRepository>(_ => new LoginRepository(cs, configuration));
        services.AddTransient<ICorreoRepository>(_ =>
            new CorreoRepository(cs, configuration, new BitacoraRepository(cs)));

        services.AddTransient<IAdmonGRMRepository>(_ => new AdmonGRMRepository(cs));
        services.AddTransient<ICentrosGestoresRepository>(_ => new CentrosGestoresRepository(cs));
        services.AddTransient<ICatalogosRepository>(_ => new CatalogosRepository(cs));
        services.AddTransient<IPerfilesRepository>(_ => new PerfilesRepository(cs));
        services.AddTransient<IRolesCatalogoRepository>(_ => new RolesCatalogoRepository(cs));
        services.AddTransient<IAltaContratosRepository>(_ => new AltaContratosRepository(cs));
        services.AddTransient<ISAPPIRepository>(_ => new SAPPIRepository(cs, configuration));
        services.AddTransient<IInterfacesRepository>(_ => new InterfacesRepository(cs));
        services.AddTransient<IBitacoraRepository>(_ => new BitacoraRepository(cs));
        services.AddTransient<IBitacoraAdmonRepository>(_ => new BitacoraAdmonRepository(cs));

        // ISOEstimationRepository registrado en AddInfrastructure()
        services.AddTransient<IDocumentosRepository>(_ => new DocumentosRepository(cs, configuration));
        services.AddTransient<IESignRepository>(_ =>
            new ESignRepository(cs, configuration, new DocumentoFirmadoRepository(cs)));

        services.AddTransient<IReceptionAlmacenRepository>(_ => new ReceptionRepository(cs));
        services.AddTransient<IDocumentoFirmadoRepository>(_ => new DocumentoFirmadoRepository(cs));
        services.AddTransient<ISATRepository>(_ => new SATRepository(cs));
        services.AddTransient<IAnaliticoPagoRepository>(_ => new AnaliticoPagoRepository(cs));
        services.AddTransient<IFacturaPDFRepository>(_ => new FacturaPDFRepository(cs));
        services.AddTransient<ICFDIValidationCartaPorteRepository>(_ => new CFDIValidationCartaPorteRepository(cs));

        services.AddTransient<ICFDIValidationRepository>(_ =>
            new CFDIValidationRepository(cs, configuration,
                new CFDIValidationCartaPorteRepository(cs),
                new CFDIValidation40Repository(cs)));

        services.AddTransient<ICFDIRepository>(_ =>
            new CFDIRepository(cs, configuration,
                new BitacoraRepository(cs),
                new SAPPIRepository(cs, configuration),
                new FacturaElectronicaRepository(cs),
                new AdefasRepository(cs),
                new SATRepository(cs),
                new CorreoRepository(cs, configuration, new BitacoraRepository(cs)),
                new AnaliticoPagoRepository(cs),
                new InvoiceRepository(cs),
                new CFDIValidationRepository(cs, configuration,
                    new CFDIValidationCartaPorteRepository(cs),
                    new CFDIValidation40Repository(cs))));

        services.AddTransient<IFacturaElectronicaRepository>(_ => new FacturaElectronicaRepository(cs));
        services.AddTransient<IInvoiceCxPRepository>(_ => new InvoiceCxPRepository(cs));
        services.AddTransient<IInvoiceAPCxPRepository>(_ => new InvoiceAPCxPRepository(cs));
        services.AddTransient<IInvoiceRepository>(_ => new InvoiceRepository(cs));
        services.AddTransient<IRecepcionEPRepository>(_ => new RecepcionEPRepository(cs));
        services.AddTransient<IConsultaCopadeRepository>(_ => new ConsultaCopadeRepository(cs));
        services.AddTransient<IOrganismRepository>(_ => new OrganismRepository(cs));
        services.AddTransient<IPreFacturaRepository>(_ => new PreFacturaRepository(cs));
        services.AddTransient<IPrefacturaAPRepository>(_ => new PrefacturaAPRepository(cs));
        services.AddTransient<ICancelacionesRepository>(_ => new CancelacionesRepository(cs));

        services.AddTransient<IFacturaDocumentalAPSinCFDIRepository>(_ =>
            new FacturaDocumentalAPSinCFDIRepository(cs, configuration,
                new BitacoraRepository(cs),
                new SAPPIRepository(cs, configuration),
                new FacturaElectronicaRepository(cs),
                new AdefasRepository(cs),
                new SATRepository(cs),
                new InvoiceRepository(cs),
                new CFDIRepository(cs, configuration,
                    new BitacoraRepository(cs),
                    new SAPPIRepository(cs, configuration),
                    new FacturaElectronicaRepository(cs),
                    new AdefasRepository(cs),
                    new SATRepository(cs),
                    new CorreoRepository(cs, configuration, new BitacoraRepository(cs)),
                    new AnaliticoPagoRepository(cs),
                    new InvoiceRepository(cs),
                    new CFDIValidationRepository(cs, configuration,
                        new CFDIValidationCartaPorteRepository(cs),
                        new CFDIValidation40Repository(cs))),
                new AnaliticoPagoRepository(cs),
                new CorreoRepository(cs, configuration, new BitacoraRepository(cs)),
                new CatalogosRepository(cs)));

        services.AddTransient<IFacturaDocumentalAPConCFDIRepository>(_ =>
            new FacturaDocumentalAPConCFDIRepository(cs, configuration,
                new BitacoraRepository(cs),
                new SAPPIRepository(cs, configuration),
                new FacturaElectronicaRepository(cs),
                new AdefasRepository(cs),
                new SATRepository(cs),
                new InvoiceRepository(cs),
                new CFDIRepository(cs, configuration,
                    new BitacoraRepository(cs),
                    new SAPPIRepository(cs, configuration),
                    new FacturaElectronicaRepository(cs),
                    new AdefasRepository(cs),
                    new SATRepository(cs),
                    new CorreoRepository(cs, configuration, new BitacoraRepository(cs)),
                    new AnaliticoPagoRepository(cs),
                    new InvoiceRepository(cs),
                    new CFDIValidationRepository(cs, configuration,
                        new CFDIValidationCartaPorteRepository(cs),
                        new CFDIValidation40Repository(cs))),
                new AnaliticoPagoRepository(cs),
                new CorreoRepository(cs, configuration, new BitacoraRepository(cs)),
                new CFDIValidationCartaPorteRepository(cs)));

        services.AddTransient<IFacturaDocumentalSinCFDIRepository>(_ =>
            new FacturaDocumentalSinCFDIRepository(cs, configuration,
                new BitacoraRepository(cs),
                new SAPPIRepository(cs, configuration),
                new FacturaElectronicaRepository(cs),
                new AdefasRepository(cs),
                new SATRepository(cs),
                new InvoiceRepository(cs),
                new CFDIRepository(cs, configuration,
                    new BitacoraRepository(cs),
                    new SAPPIRepository(cs, configuration),
                    new FacturaElectronicaRepository(cs),
                    new AdefasRepository(cs),
                    new SATRepository(cs),
                    new CorreoRepository(cs, configuration, new BitacoraRepository(cs)),
                    new AnaliticoPagoRepository(cs),
                    new InvoiceRepository(cs),
                    new CFDIValidationRepository(cs, configuration,
                        new CFDIValidationCartaPorteRepository(cs),
                        new CFDIValidation40Repository(cs))),
                new AnaliticoPagoRepository(cs),
                new CorreoRepository(cs, configuration, new BitacoraRepository(cs))));

        services.AddTransient<IFacturaDocumentalConCFDIRepository>(_ =>
            new FacturaDocumentalConCFDIRepository(cs, configuration,
                new BitacoraRepository(cs),
                new SAPPIRepository(cs, configuration),
                new FacturaElectronicaRepository(cs),
                new AdefasRepository(cs),
                new SATRepository(cs),
                new InvoiceRepository(cs),
                new CFDIRepository(cs, configuration,
                    new BitacoraRepository(cs),
                    new SAPPIRepository(cs, configuration),
                    new FacturaElectronicaRepository(cs),
                    new AdefasRepository(cs),
                    new SATRepository(cs),
                    new CorreoRepository(cs, configuration, new BitacoraRepository(cs)),
                    new AnaliticoPagoRepository(cs),
                    new InvoiceRepository(cs),
                    new CFDIValidationRepository(cs, configuration,
                        new CFDIValidationCartaPorteRepository(cs),
                        new CFDIValidation40Repository(cs))),
                new AnaliticoPagoRepository(cs),
                new CorreoRepository(cs, configuration, new BitacoraRepository(cs))));

        services.AddTransient<ICFDIAPRepository>(_ =>
            new CFDIAPRepository(cs, configuration,
                new BitacoraRepository(cs),
                new SAPPIRepository(cs, configuration),
                new FacturaElectronicaRepository(cs),
                new AdefasRepository(cs),
                new SATRepository(cs),
                new CorreoRepository(cs, configuration, new BitacoraRepository(cs)),
                new AnaliticoPagoRepository(cs),
                new CFDIRepository(cs, configuration,
                    new BitacoraRepository(cs),
                    new SAPPIRepository(cs, configuration),
                    new FacturaElectronicaRepository(cs),
                    new AdefasRepository(cs),
                    new SATRepository(cs),
                    new CorreoRepository(cs, configuration, new BitacoraRepository(cs)),
                    new AnaliticoPagoRepository(cs),
                    new InvoiceRepository(cs),
                    new CFDIValidationRepository(cs, configuration,
                        new CFDIValidationCartaPorteRepository(cs),
                        new CFDIValidation40Repository(cs)))));

        services.AddTransient<IPaymentScheduleRepository>(_ => new PaymentScheduleRepository(cs));
        services.AddTransient<IPaymentListRepository>(_ => new PaymentListRepository(cs));
        services.AddTransient<IProveedoresEmailRepository>(_ => new ProveedoresEmailRepository(cs));
        services.AddTransient<ICartaPorteRepository>(_ => new CartaPorteRepository(cs, configuration));

        return services;
    }
}
