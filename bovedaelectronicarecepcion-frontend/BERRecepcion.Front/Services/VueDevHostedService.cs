using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace BERRecepcion.Front.Services
{
    public class VueDevHostedService : BackgroundService
    {
        private readonly ILogger<VueDevHostedService> _logger;
        private readonly string _frontPath;

        public VueDevHostedService(ILogger<VueDevHostedService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _frontPath = configuration["VueApp:FrontPath"] ?? string.Empty;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (string.IsNullOrWhiteSpace(_frontPath))
            {
                _logger.LogWarning("[VueApp] VueApp:FrontPath no configurado. El build Vue no se ejecutará.");
                return;
            }

            if (!Directory.Exists(_frontPath))
            {
                _logger.LogWarning("[VueApp] Directorio no encontrado: {Path}", _frontPath);
                return;
            }

            _logger.LogInformation("[VueApp] Compilando proyecto Vue en: {Path}", _frontPath);

            var psi = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                // Carga nvm, va al directorio del proyecto Vue y ejecuta el build apuntando a wwwroot
                Arguments = $"-c \"source ~/.nvm/nvm.sh 2>/dev/null || true; cd '{_frontPath}' && npm run gen:legacy\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
            };

            using var process = new Process { StartInfo = psi };

            process.OutputDataReceived += (_, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    _logger.LogInformation("[VueApp] {Line}", e.Data);
            };

            process.ErrorDataReceived += (_, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    _logger.LogWarning("[VueApp] {Line}", e.Data);
            };

            try
            {
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                await process.WaitForExitAsync(stoppingToken);

                if (process.ExitCode == 0)
                    _logger.LogInformation("[VueApp] Build completado. Archivos en wwwroot/BERVueDist");
                else
                    _logger.LogError("[VueApp] Build falló con código de salida {Code}", process.ExitCode);
            }
            catch (OperationCanceledException)
            {
                if (!process.HasExited)
                    process.Kill(entireProcessTree: true);

                _logger.LogInformation("[VueApp] Build cancelado al cerrar la aplicación");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[VueApp] Error al ejecutar el build Vue");
            }
        }
    }
}
