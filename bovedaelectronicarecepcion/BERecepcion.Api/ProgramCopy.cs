using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace BERecepcion.Api
{
    public class ProgramCopy
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog()   //para habilitar el uso de la libreria SeriLog
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    // Para habilitar el protocolo TLS 1.2
                    webBuilder.ConfigureKestrel(serverOptions =>
                    {
                        serverOptions.ConfigureHttpsDefaults(configureOptions: co =>
                        {
                            co.SslProtocols = System.Security.Authentication.SslProtocols.Tls12;
                        });
                    });
                    webBuilder.UseStartup<Startup>();
                });
    }
}
