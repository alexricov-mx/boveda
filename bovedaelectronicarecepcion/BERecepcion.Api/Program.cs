using BERecepcion.Api.StartupExtensions;
using BERecepcion.Infraestructura.StartupExtensions;
using Microsoft.AspNetCore.Builder;
using Serilog;
using System;

try
{
    var builder = WebApplication.CreateBuilder(args);

    Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .CreateLogger();

    builder.Host.UseSerilog();

    builder.Services
        .AddCorsConfig()
        .AddAuthenticationConfig(builder.Configuration)
        .AddAuthorizationPoliciesConfig()
        .AddAuthServicesConfig()
        .AddControllersConfig()
        .AddSwaggerConfig()
        .AddInfrastructure(builder.Configuration)
        .AddLegacyRepositories(builder.Configuration);

    builder.Services.AddHealthChecksConfig(builder.Configuration);

    var app = builder.Build();

    app.ConfigurePipeline(CorsExtensions.PolicyName);

    app.Run();
}
catch (Exception ex)
{
    throw;
}
