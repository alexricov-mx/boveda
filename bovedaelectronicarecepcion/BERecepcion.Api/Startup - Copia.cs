using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using BERecepcion.Api.Controllers;
using BERecepcion.Api.ModelBinding;
using BERecepcion.Api.StartupExtensions;
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
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Serilog;

namespace BERecepcion.Api
{
    public class StartupCopia
    {
        readonly string MyAllowSpecificOrigins = "_myAllowSpecificOrigins"; //Eliminar despues
        public StartupCopia(IConfiguration configuration)
        {
            Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(configuration).CreateLogger();
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //Inicio Eliminar
            services.AddCors(options =>
            {
                options.AddPolicy(name: MyAllowSpecificOrigins,
                                  builder =>
                                  {
                                      builder.AllowAnyOrigin()
                                             .AllowAnyMethod()
                                             .AllowAnyHeader();
                                  });
            });
            //Fin Eliminar

            // Configuracion de HealthChecks
            services.AddHealthChecksConfig(Configuration);

            //agregar fluentValidation a todas las clases.
            services.AddControllers()
                .AddFluentValidation(fluConfiguration => fluConfiguration.RegisterValidatorsFromAssemblyContaining<Startup>());
            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "API - Boveda Electronica Recepcion", Version = "v1" });
            });

            // registramos los controladores
            services.AddTransient<ISupplyOrderRepository>(x => new SupplyOrderRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]));
            services.AddTransient<IAdefasRepository>(x => new AdefasRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]));
            services.AddTransient<IDesvioFirmasRepository>(x => new DesvioFirmasRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]));
            services.AddTransient<IUsuariosRepository>(x => new UsuariosRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"], Configuration, new BitacoraAdmonRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"])));
            services.AddTransient<ILoginRepository>(x => new LoginRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"], Configuration));
            services.AddTransient<ICorreoRepository>(x => new CorreoRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"], Configuration, new BitacoraRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"])));
            services.AddTransient<IAdmonGRMRepository>(x => new AdmonGRMRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]));
            services.AddTransient<ICentrosGestoresRepository>(x => new CentrosGestoresRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]));
            services.AddTransient<ICatalogosRepository>(x => new CatalogosRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]));
            services.AddTransient<IReactivaProcesosRepository>(x => new ReactivaProcesosRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"], new SAPPIRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"], Configuration)));
            services.AddTransient<ICatalogosRepository>(x => new CatalogosRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]));
            services.AddTransient<IPerfilesRepository>(x => new PerfilesRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]));
            services.AddTransient<IRolesCatalogoRepository>(x => new RolesCatalogoRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]));
            services.AddTransient<IAltaContratosRepository>(x => new AltaContratosRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]));
            services.AddTransient<ISAPPIRepository>(x => new SAPPIRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"], Configuration));
            services.AddTransient<IInterfacesRepository>(x => new InterfacesRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]));
            services.AddTransient<IBitacoraRepository>(x => new BitacoraRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]));
            services.AddTransient<IBitacoraAdmonRepository>(x => new BitacoraAdmonRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]));
            services.AddTransient<ISOEstimationRepository>(x => new SOEstimacionRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]));
            services.AddTransient<IDocumentosRepository>(x => new DocumentosRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"], Configuration));
            services.AddTransient<IESignRepository>(x => new ESignRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"], Configuration, new DocumentoFirmadoRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"])));
            services.AddTransient<ICopadeRepository>(x => new CopadeRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]));
            services.AddTransient<IReceptionAlmacenRepository>(x => new ReceptionRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]));
            services.AddTransient<IDocumentoFirmadoRepository>(x => new DocumentoFirmadoRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]));
            services.AddTransient<ISATRepository>(x => new SATRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]));
            services.AddTransient<IAnaliticoPagoRepository>(x => new AnaliticoPagoRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]));
            services.AddTransient<IFacturaPDFRepository>(x => new FacturaPDFRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]));
            services.AddTransient<ICFDIValidationCartaPorteRepository>(x => new CFDIValidationCartaPorteRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]));
            services.AddTransient<ICFDIValidationRepository>(x => new CFDIValidationRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"], Configuration,
                new CFDIValidationCartaPorteRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
                new CFDIValidation40Repository(Configuration["ConnectionStrings:SQLServerSQLDEV002"])));
            services.AddTransient<ICFDIRepository>(x => new CFDIRepository(
                Configuration["ConnectionStrings:SQLServerSQLDEV002"],
                Configuration,
                new BitacoraRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
                new SAPPIRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"], Configuration),
                new FacturaElectronicaRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
                new AdefasRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
                new SATRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
                new CorreoRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"], Configuration, new BitacoraRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"])),
                new UsuariosRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"], Configuration, new BitacoraAdmonRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"])),
                new AnaliticoPagoRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
                new InvoiceRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
                new CFDIValidationRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"], Configuration,
                    new CFDIValidationCartaPorteRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
                    new CFDIValidation40Repository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]))
                ));
            services.AddTransient<IFacturaElectronicaRepository>(x => new FacturaElectronicaRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]));
            services.AddTransient<IInvoiceCxPRepository>(x => new InvoiceCxPRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]));
            services.AddTransient<IInvoiceAPCxPRepository>(x => new InvoiceAPCxPRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]));
            services.AddTransient<IInvoiceRepository>(x => new InvoiceRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]));
            services.AddTransient<IRecepcionEPRepository>(x => new RecepcionEPRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]));
            services.AddTransient<IConsultaCopadeRepository>(x => new ConsultaCopadeRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]));
            services.AddTransient<IOrganismRepository>(x => new OrganismRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]));
            services.AddTransient<IConsultasRepository>(x => new ConsultasRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]));
            services.AddTransient<IPreFacturaRepository>(x => new PreFacturaRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]));
            services.AddTransient<IPrefacturaAPRepository>(x => new PrefacturaAPRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]));
            services.AddTransient<ICancelacionesRepository>(x => new CancelacionesRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]));
            services.AddTransient<IFacturaDocumentalAPSinCFDIRepository>(x => new FacturaDocumentalAPSinCFDIRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"], Configuration, new BitacoraRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]), new SAPPIRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"], Configuration), new FacturaElectronicaRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]), new AdefasRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]), new SATRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]), new InvoiceRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
                new CFDIRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"], Configuration,
                    new BitacoraRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
                    new SAPPIRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"], Configuration),
                    new FacturaElectronicaRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
                    new AdefasRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
                    new SATRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
                    new CorreoRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"], Configuration, new BitacoraRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"])),
                    new UsuariosRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"], Configuration, new BitacoraAdmonRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"])),
                    new AnaliticoPagoRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
                    new InvoiceRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
                    new CFDIValidationRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"], Configuration,
                        new CFDIValidationCartaPorteRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
                        new CFDIValidation40Repository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]))),
                new AnaliticoPagoRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
                new CorreoRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"], Configuration, new BitacoraRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"])),
                new CatalogosRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"])
                ));
            services.AddTransient<IFacturaDocumentalAPConCFDIRepository>(x => new FacturaDocumentalAPConCFDIRepository
                (
                Configuration["ConnectionStrings:SQLServerSQLDEV002"],
                Configuration, new BitacoraRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
                new SAPPIRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"], Configuration),
                new FacturaElectronicaRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
                new AdefasRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
                new SATRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
                new InvoiceRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
                new CFDIRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"], Configuration,
                    new BitacoraRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
                    new SAPPIRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"], Configuration),
                    new FacturaElectronicaRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
                    new AdefasRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
                    new SATRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
                    new CorreoRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"], Configuration, new BitacoraRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"])),
                    new UsuariosRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"], Configuration, new BitacoraAdmonRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"])),
                    new AnaliticoPagoRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
                    new InvoiceRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
                    new CFDIValidationRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"], Configuration,
                        new CFDIValidationCartaPorteRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
                        new CFDIValidation40Repository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]))),
                new AnaliticoPagoRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
                new CorreoRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"], Configuration, new BitacoraRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"])),
                new CFDIValidationCartaPorteRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"])
                ));
            services.AddTransient<IFacturaDocumentalSinCFDIRepository>(x => new FacturaDocumentalSinCFDIRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"], Configuration, new BitacoraRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]), new SAPPIRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"], Configuration), new FacturaElectronicaRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]), new AdefasRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]), new SATRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]), new InvoiceRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
                new CFDIRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"], Configuration,
                    new BitacoraRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
                    new SAPPIRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"], Configuration),
                    new FacturaElectronicaRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
                    new AdefasRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
                    new SATRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
                    new CorreoRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"], Configuration, new BitacoraRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"])),
                    new UsuariosRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"], Configuration, new BitacoraAdmonRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"])),
                    new AnaliticoPagoRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
                    new InvoiceRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
                    new CFDIValidationRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"], Configuration, new CFDIValidationCartaPorteRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
                    new CFDIValidation40Repository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]))),
                new AnaliticoPagoRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
                new CorreoRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"], Configuration, new BitacoraRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]))
                ));
            services.AddTransient<IFacturaDocumentalConCFDIRepository>(x => new FacturaDocumentalConCFDIRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"], Configuration, new BitacoraRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]), new SAPPIRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"], Configuration), new FacturaElectronicaRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]), new AdefasRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]), new SATRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]), new InvoiceRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
                new CFDIRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"], Configuration,
                    new BitacoraRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
                    new SAPPIRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"], Configuration),
                    new FacturaElectronicaRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
                    new AdefasRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
                    new SATRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
                    new CorreoRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"], Configuration, new BitacoraRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"])),
                    new UsuariosRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"], Configuration, new BitacoraAdmonRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"])),
                    new AnaliticoPagoRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
                    new InvoiceRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
                    new CFDIValidationRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"], Configuration,
                        new CFDIValidationCartaPorteRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
                        new CFDIValidation40Repository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]))),
                new AnaliticoPagoRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
                new CorreoRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"], Configuration, new BitacoraRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]))
                ));
            services.AddTransient<ICFDIAPRepository>(x =>
                new CFDIAPRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"], Configuration,
                new BitacoraRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
                new SAPPIRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"], Configuration),
                new FacturaElectronicaRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
                new AdefasRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
                new SATRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
                new CorreoRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"], Configuration,
                new BitacoraRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"])),
                new UsuariosRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"], Configuration, new BitacoraAdmonRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"])),
                new AnaliticoPagoRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
                new CFDIRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"], Configuration,
                    new BitacoraRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
                    new SAPPIRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"], Configuration),
                    new FacturaElectronicaRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
                    new AdefasRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
                    new SATRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
                    new CorreoRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"], Configuration, new BitacoraRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"])),
                    new UsuariosRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"], Configuration, new BitacoraAdmonRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"])),
                    new AnaliticoPagoRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
                    new InvoiceRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
                    new CFDIValidationRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"], Configuration,
                        new CFDIValidationCartaPorteRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
                        new CFDIValidation40Repository(Configuration["ConnectionStrings:SQLServerSQLDEV002"])))
                ));
            services.AddTransient<IExpedienteElectronicoRepository>(x => new ExpedienteElectronicoRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]));
            services.AddTransient<IConsultasRepository>(x => new ConsultasRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]));
            services.AddTransient<IPaymentScheduleRepository>(x => new PaymentScheduleRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]));
            services.AddTransient<IPaymentListRepository>(x => new PaymentListRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]));

            services.AddMvc(properties =>
            {
                properties.ModelBinderProviders.Insert(0, new JsonModelBinderProvider());
            });
            services.AddTransient<IProveedoresEmailRepository>(x => new ProveedoresEmailRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"]));
            services.AddTransient<ICartaPorteRepository>(x => new CartaPorteRepository(Configuration["ConnectionStrings:SQLServerSQLDEV002"], Configuration));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "API - Boveda Electronica Recepcion");
            });

            app.UseStaticFiles();

            app.UseRouting();

            app.UseCors(MyAllowSpecificOrigins); //Eliminar

            app.UseAuthorization();

            // Se agrega HealthChecks
            app.UseHealthChecksConfig();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            var cultureInfo = new CultureInfo("es-MX");
            CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
            CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
        }
    }
}
