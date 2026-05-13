using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BERRecepcion.Front.Controllers
{
    /// <summary>
    /// Controller de diagnóstico para verificar el estado del sistema sin autenticación
    /// </summary>
    [AllowAnonymous]
    public class DiagnosticoController : Controller
    {
        private readonly IConfiguration _configuration;

        public DiagnosticoController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Página de diagnóstico accesible sin autenticación
        /// </summary>
        [HttpGet]
        public IActionResult Index()
        {
            try
            {
                var diagnostico = new
                {
                    Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    
                    // Información de Azure AD
                    AzureAD = new
                    {
                        Instance = _configuration["AzureAd:Instance"],
                        TenantId = _configuration["AzureAd:TenantId"],
                        ClientId = _configuration["AzureAd:ClientId"],
                        WebAppURI = _configuration["AzureAd:WebAppURI"],
                        CallbackPath = _configuration["AzureAd:CallbackPath"],
                        ExpectedRedirectUri = $"{_configuration["AzureAd:WebAppURI"]?.TrimEnd('/')}{_configuration["AzureAd:CallbackPath"]}"
                    },
                    
                    // Información de ambiente
                    Ambiente = new
                    {
                        Aplicativo = _configuration["infoAplicativo:Aplicativo"],
                        Version = _configuration["infoAplicativo:Version"],
                        Ambiente = _configuration["infoAplicativo:Ambiente"],
                        ApiUrl = _configuration["ApiUrl"]
                    },
                    
                    // Data Protection Keys
                    DataProtection = CheckDataProtectionKeys(),
                    
                    // Logs disponibles
                    Logs = CheckLogsFolder()
                };

                ViewBag.Diagnostico = diagnostico;
                
                Log.Information("Acceso a página de diagnóstico desde {IP}", 
                    HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown");
                
                return View();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al generar diagnóstico");
                ViewBag.Error = ex.Message;
                return View();
            }
        }

        /// <summary>
        /// Endpoint JSON para consultas automáticas
        /// </summary>
        [HttpGet]
        public IActionResult Status()
        {
            try
            {
                var status = new
                {
                    status = "OK",
                    timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    azureAd = new
                    {
                        configured = !string.IsNullOrEmpty(_configuration["AzureAd:TenantId"]),
                        webAppUri = _configuration["AzureAd:WebAppURI"],
                        redirectUri = $"{_configuration["AzureAd:WebAppURI"]?.TrimEnd('/')}{_configuration["AzureAd:CallbackPath"]}"
                    },
                    dataProtection = CheckDataProtectionKeys(),
                    logs = CheckLogsFolder()
                };

                return Json(status);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al obtener status");
                return Json(new
                {
                    status = "ERROR",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Muestra los últimos logs directamente (sin autenticación)
        /// </summary>
        [HttpGet]
        public IActionResult VerLogs(string origen = "frontend", int lines = 50)
        {
            try
            {
                string logsPath;
                string origenLabel;
                
                // Determinar la ruta de logs según el origen
                if (origen.ToLower() == "backend")
                {
                    // Asumimos que el backend está en un directorio paralelo
                    var backendPath = AppContext.BaseDirectory;
                    var berPath = Directory.GetParent(backendPath)?.Parent?.Parent?.Parent?.Parent?.Parent?.FullName;
                    logsPath = Path.Combine(berPath,"bovedaelectronicarecepcion", "BERecepcion.Api", "logs");                    
                    
                    origenLabel = "Backend (API)";
                }
                else
                {
                    // Logs del frontend
                    var frontendPath = AppContext.BaseDirectory;
                    var berPath = Directory.GetParent(frontendPath)?.Parent?.Parent?.Parent?.FullName;
                    logsPath = Path.Combine(berPath ?? "", "logs");
                    origenLabel = "Frontend";
                }
                
                if (!Directory.Exists(logsPath))
                {
                    ViewBag.Error = $"Carpeta de logs no encontrada: {logsPath}";
                    ViewBag.Origen = origen;
                    ViewBag.OrigenLabel = origenLabel;
                    return View();
                }

                var logFiles = Directory.GetFiles(logsPath, "log*.txt")
                    .Select(f => new FileInfo(f))
                    .OrderByDescending(f => f.LastWriteTime)
                    .ToList();

                if (!logFiles.Any())
                {
                    ViewBag.Error = "No se encontraron archivos de log";
                    ViewBag.Origen = origen;
                    ViewBag.OrigenLabel = origenLabel;
                    ViewBag.LogsPath = logsPath;
                    return View();
                }

                // Leer el archivo más reciente
                var latestLog = logFiles.First();
                string[] allLines;
                using (var fileStream = new FileStream(latestLog.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var reader = new StreamReader(fileStream))
                {
                    var content = reader.ReadToEnd();
                    allLines = content.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
                }
                var recentLines = allLines.Skip(Math.Max(0, allLines.Length - lines)).ToArray();

                // Información de todos los archivos de log disponibles
                var logFilesInfo = logFiles.Select(f => new 
                {
                    Name = f.Name,
                    Size = FormatFileSize(f.Length),
                    LastModified = f.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    FullPath = f.FullName
                }).ToList();

                ViewBag.FileName = latestLog.Name;
                ViewBag.FileSize = FormatFileSize(latestLog.Length);
                ViewBag.LastModified = latestLog.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss");
                ViewBag.TotalLines = allLines.Length;
                ViewBag.ShowingLines = recentLines.Length;
                ViewBag.LogContent = recentLines;
                ViewBag.Origen = origen;
                ViewBag.OrigenLabel = origenLabel;
                ViewBag.LogsPath = logsPath;
                ViewBag.LogFiles = logFilesInfo;

                return View();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al leer logs de {Origen}", origen);
                ViewBag.Error = $"Error al leer logs: {ex.Message}";
                ViewBag.Origen = origen;
                return View();
            }
        }

        private object CheckDataProtectionKeys()
        {
            var paths = new[]
            {
                Path.Combine(AppContext.BaseDirectory, "App_Data", "DataProtectionKeys"),
                Path.Combine(AppContext.BaseDirectory, "DataProtectionKeys"),
                Path.Combine(Path.GetTempPath(), "BERRecepcion", "DataProtectionKeys")
            };

            foreach (var path in paths)
            {
                try
                {
                    if (Directory.Exists(path))
                    {
                        var keyFiles = Directory.GetFiles(path, "*.xml");
                        return new
                        {
                            status = "OK",
                            location = path,
                            keysCount = keyFiles.Length,
                            exists = true
                        };
                    }
                }
                catch { }
            }

            return new
            {
                status = "NOT_FOUND",
                location = "Ninguna ubicación disponible",
                keysCount = 0,
                exists = false,
                message = "⚠️ Las claves de Data Protection no se encontraron. Esto causará problemas de autenticación."
            };
        }

        private object CheckLogsFolder()
        {
            try
            {
                var logsPath = Path.Combine(AppContext.BaseDirectory, "logs");
                
                if (!Directory.Exists(logsPath))
                {
                    return new
                    {
                        status = "NOT_FOUND",
                        path = logsPath,
                        exists = false
                    };
                }

                var logFiles = Directory.GetFiles(logsPath, "log-*.txt");
                var latestFile = logFiles
                    .Select(f => new FileInfo(f))
                    .OrderByDescending(f => f.LastWriteTime)
                    .FirstOrDefault();

                return new
                {
                    status = "OK",
                    path = logsPath,
                    exists = true,
                    filesCount = logFiles.Length,
                    latestFile = latestFile?.Name,
                    latestModified = latestFile?.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss")
                };
            }
            catch (Exception ex)
            {
                return new
                {
                    status = "ERROR",
                    error = ex.Message
                };
            }
        }

        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
    }
}
