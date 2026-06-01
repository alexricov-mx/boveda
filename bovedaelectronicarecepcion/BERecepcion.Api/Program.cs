using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using BERecepcion.Api;
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
using BERecepcion.Core.Exceptions;
using BERecepcion.Infraestructura.StartupExtensions;
using FluentValidation;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Serilog;
using BERecepcion.Api.Infrastructure.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Tokens;

try
{
    string MyAllowSpecificOrigins = "_myAllowSpecificOrigins"; 
    var builder = WebApplication.CreateBuilder(args);

    Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .CreateLogger();
    
    builder.Host.UseSerilog();
    
    builder.Services.AddCors(options =>
    {
        options.AddPolicy(name: MyAllowSpecificOrigins,
                          builder =>
                          {
                              builder.AllowAnyOrigin()
                                     .AllowAnyMethod()
                                     .AllowAnyHeader();
                          });
    });
    
    // Configurar autenticación JWT con Azure Entra ID
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddMicrosoftIdentityWebApi(options =>
        {
            builder.Configuration.Bind("AzureAd", options);
            
            // Configurar validación de audience para aceptar ambos formatos
            options.TokenValidationParameters.ValidateAudience = true;
            var clientId = builder.Configuration["AzureAd:ClientId"];
            options.TokenValidationParameters.ValidAudiences = new[]
            {
                clientId,
                $"api://{clientId}"
            };
        },
        options => { builder.Configuration.Bind("AzureAd", options); });
    
    // Políticas de autorización: una por cada rol de BD + política base de usuario autenticado
    builder.Services.AddAuthorization(options =>
    {
        // Política base: solo requiere JWT válido (sin rol específico de BD)
        options.AddPolicy(PolicyConstants.RequireAuthenticatedUser, policy =>
            policy.RequireAuthenticatedUser());

        // ── Administración ──────────────────────────────────────────────
        options.AddPolicy(PolicyConstants.RequireAdministrationUsers, policy =>
            policy.RequireAuthenticatedUser()
                  .RequireClaim(ApiAuthConstants.RoleBdClaim, RoleConstants.AdministrationUsers));

        options.AddPolicy(PolicyConstants.RequireAdministrationProfiles, policy =>
            policy.RequireAuthenticatedUser()
                  .RequireClaim(ApiAuthConstants.RoleBdClaim, RoleConstants.AdministrationProfiles));

        options.AddPolicy(PolicyConstants.RequireAdministrationInterfaces, policy =>
            policy.RequireAuthenticatedUser()
                  .RequireClaim(ApiAuthConstants.RoleBdClaim, RoleConstants.AdministrationInterfaces));

        // ── Recepción ────────────────────────────────────────────────────
        options.AddPolicy(PolicyConstants.RequireReceptionElectronicInvoice, policy =>
            policy.RequireAuthenticatedUser()
                  .RequireClaim(ApiAuthConstants.RoleBdClaim, RoleConstants.ReceptionElectronicInvoice));

        options.AddPolicy(PolicyConstants.RequireReceptionDocumentalInvoice, policy =>
            policy.RequireAuthenticatedUser()
                  .RequireClaim(ApiAuthConstants.RoleBdClaim, RoleConstants.ReceptionDocumentalInvoice));

        options.AddPolicy(PolicyConstants.RequireReceptionSignReception, policy =>
            policy.RequireAuthenticatedUser()
                  .RequireClaim(ApiAuthConstants.RoleBdClaim, RoleConstants.ReceptionSignReception));

        options.AddPolicy(PolicyConstants.RequireReceptionSignCopade, policy =>
            policy.RequireAuthenticatedUser()
                  .RequireClaim(ApiAuthConstants.RoleBdClaim, RoleConstants.ReceptionSignCopade));

        options.AddPolicy(PolicyConstants.RequireReceptionSignSupplyOrders, policy =>
            policy.RequireAuthenticatedUser()
                  .RequireClaim(ApiAuthConstants.RoleBdClaim, RoleConstants.ReceptionSignSupplyOrders));

        // ── Desvíos ──────────────────────────────────────────────────────
        options.AddPolicy(PolicyConstants.RequireDeviationSigns, policy =>
            policy.RequireAuthenticatedUser()
                  .RequireClaim(ApiAuthConstants.RoleBdClaim, RoleConstants.DeviationSigns));

        // ── Contratos ────────────────────────────────────────────────────
        options.AddPolicy(PolicyConstants.RequireContractsRegister, policy =>
            policy.RequireAuthenticatedUser()
                  .RequireClaim(ApiAuthConstants.RoleBdClaim, RoleConstants.ContractsRegister));
    });

    // Registrar servicios de infraestructura de autenticación (SOLID: SRP + DIP)
    // Scoped: ciclo de vida por request, alineado con IClaimsTransformation y los repositorios
    builder.Services.AddScoped<IBackendUserService, BackendUserService>();
    builder.Services.AddScoped<IClaimsTransformation, UserClaimsTransformation>();
    
    builder.Services.AddHealthChecksConfig(builder.Configuration);
    
    // Agregar el filtro global de excepciones para manejo de errores HTTP apropiados
    builder.Services.AddControllers(options =>
    {
        options.Filters.Add<BERecepcion.Api.Filters.GlobalExceptionFilter>();
    // [FV12] Registrar filtro de auto-validación (reemplaza AddFluentValidationAutoValidation de FV11)
    options.Filters.Add<BERecepcion.Api.Filters.FluentValidationActionFilter>();
    });

    // [FV12] Escaneo de validators — BERecepcion.Api
    builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

    // [FV12] Escaneo de validators — BERecepcion.Core completo (auto-descubre todo AbstractValidator<T>)
    // Tipo ancla: ValidationException vive en BERecepcion.Core.Exceptions
    builder.Services.AddValidatorsFromAssemblyContaining<BERecepcion.Core.Exceptions.ValidationException>();

    // Unificar el formato de errores de model-state con GlobalExceptionFilter y FluentValidationActionFilter.
    // Los tres caminos retornan ValidationProblemDetails RFC 7807.
    builder.Services.Configure<ApiBehaviorOptions>(options =>
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
                Title  = "Errores de validación",
                Detail = "Uno o más errores de validación ocurrieron.",
                Status = 400
            });
        };
    });
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "API - Boveda Electronica Recepcion", Version = "v1" });
        
        // Configurar JWT Bearer en Swagger UI
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header usando el esquema Bearer. Ejemplo: \"Authorization: Bearer {token}\"",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT"
        });
        
        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    });
    // Infraestructura — Módulo OrdenSurtimiento migrado a DI limpio con IDbConnectionFactory
    builder.Services.AddInfrastructure(builder.Configuration);

    builder.Services.AddTransient<IAdefasRepository>(x => new AdefasRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]));    builder.Services.AddTransient<IDesvioFirmasRepository>(x => new DesvioFirmasRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]));
    builder.Services.AddTransient<IUsuariosRepository>(x => new UsuariosRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"], builder.Configuration, new BitacoraAdmonRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"])));

    builder.Services.AddTransient<IOldUsuariosRepository>(x => new OldUsuariosRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"], builder.Configuration, new BitacoraAdmonRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"])));

    builder.Services.AddTransient<ILoginRepository>(x =>  new LoginRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"], builder.Configuration));
    builder.Services.AddTransient<ICorreoRepository>(x => new CorreoRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"], builder.Configuration, new BitacoraRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"])));
    builder.Services.AddTransient<IAdmonGRMRepository>(x => new AdmonGRMRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]));
    builder.Services.AddTransient<ICentrosGestoresRepository>(x => new CentrosGestoresRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]));
    builder.Services.AddTransient<ICatalogosRepository>(x => new CatalogosRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]));
    builder.Services.AddTransient<ICatalogosRepository>(x => new CatalogosRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]));
    builder.Services.AddTransient<IPerfilesRepository>(x => new PerfilesRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]));
    builder.Services.AddTransient<IRolesCatalogoRepository>(x => new RolesCatalogoRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]));
    builder.Services.AddTransient<IAltaContratosRepository>(x => new AltaContratosRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]));
    builder.Services.AddTransient<ISAPPIRepository>(x => new SAPPIRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"], builder.Configuration));
    builder.Services.AddTransient<IInterfacesRepository>(x => new InterfacesRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]));
    builder.Services.AddTransient<IBitacoraRepository>(x => new BitacoraRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]));
    builder.Services.AddTransient<IBitacoraAdmonRepository>(x => new BitacoraAdmonRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]));
    // ISOEstimationRepository registrado en AddInfrastructure() arriba
    builder.Services.AddTransient<IDocumentosRepository>(x => new DocumentosRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"], builder.Configuration));
    builder.Services.AddTransient<IESignRepository>(x => new ESignRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"], builder.Configuration, new DocumentoFirmadoRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"])));
    builder.Services.AddTransient<ICopadeRepository>(x => new CopadeRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]));
    builder.Services.AddTransient<IReceptionAlmacenRepository>(x => new ReceptionRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]));
    builder.Services.AddTransient<IDocumentoFirmadoRepository>(x => new DocumentoFirmadoRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]));
    builder.Services.AddTransient<ISATRepository>(x => new SATRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]));
    builder.Services.AddTransient<IAnaliticoPagoRepository>(x => new AnaliticoPagoRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]));
    builder.Services.AddTransient<IFacturaPDFRepository>(x => new FacturaPDFRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]));
    builder.Services.AddTransient<ICFDIValidationCartaPorteRepository>(x => new CFDIValidationCartaPorteRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]));
    builder.Services.AddTransient<ICFDIValidationRepository>(x => new CFDIValidationRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"], builder.Configuration,
        new CFDIValidationCartaPorteRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
        new CFDIValidation40Repository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"])));
    builder.Services.AddTransient<ICFDIRepository>(x => new CFDIRepository(
        builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"],
        builder.Configuration,
        new BitacoraRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
        new SAPPIRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"], builder.Configuration),
        new FacturaElectronicaRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
        new AdefasRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
        new SATRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
        new CorreoRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"], builder.Configuration, new BitacoraRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"])),
        new UsuariosRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"], builder.Configuration, new BitacoraAdmonRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"])),
        new AnaliticoPagoRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
        new InvoiceRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
        new CFDIValidationRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"], builder.Configuration,
            new CFDIValidationCartaPorteRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
            new CFDIValidation40Repository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]))
        ));
    builder.Services.AddTransient<IFacturaElectronicaRepository>(x => new FacturaElectronicaRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]));
    builder.Services.AddTransient<IInvoiceCxPRepository>(x => new InvoiceCxPRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]));
    builder.Services.AddTransient<IInvoiceAPCxPRepository>(x => new InvoiceAPCxPRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]));
    builder.Services.AddTransient<IInvoiceRepository>(x => new InvoiceRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]));
    builder.Services.AddTransient<IRecepcionEPRepository>(x => new RecepcionEPRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]));
    builder.Services.AddTransient<IConsultaCopadeRepository>(x => new ConsultaCopadeRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]));
    builder.Services.AddTransient<IOrganismRepository>(x => new OrganismRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]));
    builder.Services.AddTransient<IPreFacturaRepository>(x => new PreFacturaRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]));
    builder.Services.AddTransient<IPrefacturaAPRepository>(x => new PrefacturaAPRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]));
    builder.Services.AddTransient<ICancelacionesRepository>(x => new CancelacionesRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]));
    builder.Services.AddTransient<IFacturaDocumentalAPSinCFDIRepository>(x => new FacturaDocumentalAPSinCFDIRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"], builder.Configuration, new BitacoraRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]), new SAPPIRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"], builder.Configuration), new FacturaElectronicaRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]), new AdefasRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]), new SATRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]), new InvoiceRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
        new CFDIRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"], builder.Configuration,
            new BitacoraRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
            new SAPPIRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"], builder.Configuration),
            new FacturaElectronicaRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
            new AdefasRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
            new SATRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
            new CorreoRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"], builder.Configuration, new BitacoraRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"])),
            new UsuariosRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"], builder.Configuration, new BitacoraAdmonRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"])),
            new AnaliticoPagoRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
            new InvoiceRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
            new CFDIValidationRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"], builder.Configuration,
                new CFDIValidationCartaPorteRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
                new CFDIValidation40Repository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]))),
        new AnaliticoPagoRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
        new CorreoRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"], builder.Configuration, new BitacoraRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"])),
        new CatalogosRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"])
        ));
    builder.Services.AddTransient<IFacturaDocumentalAPConCFDIRepository>(x => new FacturaDocumentalAPConCFDIRepository
        (
        builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"],
        builder.Configuration, new BitacoraRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
        new SAPPIRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"], builder.Configuration),
        new FacturaElectronicaRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
        new AdefasRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
        new SATRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
        new InvoiceRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
        new CFDIRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"], builder.Configuration,
            new BitacoraRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
            new SAPPIRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"], builder.Configuration),
            new FacturaElectronicaRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
            new AdefasRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
            new SATRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
            new CorreoRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"], builder.Configuration, new BitacoraRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"])),
            new UsuariosRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"], builder.Configuration, new BitacoraAdmonRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"])),
            new AnaliticoPagoRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
            new InvoiceRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
            new CFDIValidationRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"], builder.Configuration,
                new CFDIValidationCartaPorteRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
                new CFDIValidation40Repository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]))),
        new AnaliticoPagoRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
        new CorreoRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"], builder.Configuration, new BitacoraRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"])),
        new CFDIValidationCartaPorteRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"])
        ));
    builder.Services.AddTransient<IFacturaDocumentalSinCFDIRepository>(x => new FacturaDocumentalSinCFDIRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"], builder.Configuration, new BitacoraRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]), new SAPPIRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"], builder.Configuration), new FacturaElectronicaRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]), new AdefasRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]), new SATRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]), new InvoiceRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
        new CFDIRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"], builder.Configuration,
            new BitacoraRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
            new SAPPIRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"], builder.Configuration),
            new FacturaElectronicaRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
            new AdefasRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
            new SATRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
            new CorreoRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"], builder.Configuration, new BitacoraRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"])),
            new UsuariosRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"], builder.Configuration, new BitacoraAdmonRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"])),
            new AnaliticoPagoRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
            new InvoiceRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
            new CFDIValidationRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"], builder.Configuration, new CFDIValidationCartaPorteRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
            new CFDIValidation40Repository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]))),
        new AnaliticoPagoRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
        new CorreoRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"], builder.Configuration, new BitacoraRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]))
        ));
    builder.Services.AddTransient<IFacturaDocumentalConCFDIRepository>(x => new FacturaDocumentalConCFDIRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"], builder.Configuration, new BitacoraRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]), new SAPPIRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"], builder.Configuration), new FacturaElectronicaRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]), new AdefasRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]), new SATRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]), new InvoiceRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
        new CFDIRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"], builder.Configuration,
            new BitacoraRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
            new SAPPIRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"], builder.Configuration),
            new FacturaElectronicaRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
            new AdefasRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
            new SATRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
            new CorreoRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"], builder.Configuration, new BitacoraRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"])),
            new UsuariosRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"], builder.Configuration, new BitacoraAdmonRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"])),
            new AnaliticoPagoRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
            new InvoiceRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
            new CFDIValidationRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"], builder.Configuration,
                new CFDIValidationCartaPorteRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
                new CFDIValidation40Repository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]))),
        new AnaliticoPagoRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
        new CorreoRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"], builder.Configuration, new BitacoraRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]))
        ));
    builder.Services.AddTransient<ICFDIAPRepository>(x =>
        new CFDIAPRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"], builder.Configuration,
        new BitacoraRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
        new SAPPIRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"], builder.Configuration),
        new FacturaElectronicaRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
        new AdefasRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
        new SATRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
        new CorreoRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"], builder.Configuration,
        new BitacoraRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"])),
        new UsuariosRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"], builder.Configuration, new BitacoraAdmonRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"])),
        new AnaliticoPagoRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
        new CFDIRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"], builder.Configuration,
            new BitacoraRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
            new SAPPIRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"], builder.Configuration),
            new FacturaElectronicaRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
            new AdefasRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
            new SATRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
            new CorreoRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"], builder.Configuration, new BitacoraRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"])),
            new UsuariosRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"], builder.Configuration, new BitacoraAdmonRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"])),
            new AnaliticoPagoRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
            new InvoiceRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
            new CFDIValidationRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"], builder.Configuration,
                new CFDIValidationCartaPorteRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]),
                new CFDIValidation40Repository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"])))
        ));
    builder.Services.AddTransient<IExpedienteElectronicoRepository>(x => new ExpedienteElectronicoRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]));
    builder.Services.AddTransient<IPaymentScheduleRepository>(x => new PaymentScheduleRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]));
    builder.Services.AddTransient<IPaymentListRepository>(x => new PaymentListRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]));

    builder.Services.AddMvc(properties =>
    {
        properties.ModelBinderProviders.Insert(0, new JsonModelBinderProvider());
    });
    builder.Services.AddTransient<IProveedoresEmailRepository>(x => new ProveedoresEmailRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"]));
    builder.Services.AddTransient<ICartaPorteRepository>(x => new CartaPorteRepository(builder.Configuration["ConnectionStrings:SQLServerSQLDEV002"], builder.Configuration));

    var app = builder.Build();
    if (app.Environment.IsDevelopment())
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

    app.UseCors(MyAllowSpecificOrigins);

    app.UseAuthentication(); // Debe ir antes de UseAuthorization
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

    app.Run();
}
catch (Exception ex)
{
    throw;
}