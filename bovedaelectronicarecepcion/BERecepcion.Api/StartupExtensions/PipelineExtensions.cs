using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System.Globalization;

namespace BERecepcion.Api.StartupExtensions;

public static class PipelineExtensions
{
    public static WebApplication ConfigurePipeline(this WebApplication app, string corsPolicy)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "API - Boveda Electronica Recepcion");
        });

        app.UseStaticFiles();
        app.UseRouting();

        app.UseCors(corsPolicy);

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseHealthChecksConfig();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });

        var cultureInfo = new CultureInfo("es-MX");
        CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
        CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

        return app;
    }
}
