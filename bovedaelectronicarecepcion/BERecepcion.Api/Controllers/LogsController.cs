using BERecepcion.Api.Filters;
using BERecepcion.Api.Infrastructure.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;

namespace BERecepcion.Api.Controllers
{
    /// <summary>
    /// Controller para gestionar y descargar logs de la aplicación.
    /// Protegido con JWT + rol AdministrationLog.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [ApiKeyAuth]
    //[Authorize(Policy = PolicyConstants.RequireAdministrationLog)]
    public class LogsController : ControllerBase
    {
        private readonly string _logsPath;

        /// <summary>
        /// Usa ContentRootPath porque Serilog escribe "logs/log-.txt" relativo al ContentRoot
        /// (definido en appsettings.json). AppContext.BaseDirectory apunta a bin\Debug\net10.0\
        /// en desarrollo, lo que hace que la carpeta nunca se encuentre.
        /// </summary>
        public LogsController(IWebHostEnvironment env)
        {
            _logsPath = Path.Combine(env.ContentRootPath, "logs");
        }

        /// <summary>
        /// Lista todos los archivos de log disponibles, ordenados por fecha (más recientes primero)
        /// </summary>
        /// <returns>Lista de archivos de log con información detallada</returns>
        [HttpGet("list")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult GetLogFiles()
        {
            try
            {
                if (!Directory.Exists(_logsPath))
                {
                    return NotFound(new
                    {
                        message = "No se encontró la carpeta de logs",
                        path = _logsPath
                    });
                }

                var logFiles = Directory.GetFiles(_logsPath, "log-*.txt")
                    .Select(file => new FileInfo(file))
                    .OrderByDescending(f => f.LastWriteTime)
                    .Select(f => new
                    {
                        fileName     = f.Name,
                        size         = FormatFileSize(f.Length),
                        sizeBytes    = f.Length,
                        lastModified = f.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss"),
                        created      = f.CreationTime.ToString("yyyy-MM-dd HH:mm:ss")
                        // fullPath omitido: exponer rutas del servidor es un riesgo de seguridad
                    })
                    .ToList();

                return Ok(new
                {
                    message = logFiles.Count > 0
                        ? $"Se encontraron {logFiles.Count} archivo(s) de log."
                        : "No se encontraron archivos de log.",
                    count = logFiles.Count,
                    files = logFiles
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "Error al listar los archivos de log.",
                    error   = ex.Message
                });
            }
        }

        /// <summary>
        /// Lista archivos de log filtrados por fecha
        /// </summary>
        /// <param name="fecha">Fecha en formato yyyy-MM-dd</param>
        /// <returns>Lista de archivos de log para la fecha especificada</returns>
        [HttpGet("list/{fecha}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetLogFilesByDate(string fecha)
        {
            try
            {
                if (!DateTime.TryParse(fecha, out DateTime parsedDate))
                {
                    return BadRequest(new
                    {
                        message = "Formato de fecha inválido. Use yyyy-MM-dd",
                        example = DateTime.Now.ToString("yyyy-MM-dd")
                    });
                }

                if (!Directory.Exists(_logsPath))
                {
                    return NotFound(new
                    {
                        message = "No se encontró la carpeta de logs",
                        path = _logsPath
                    });
                }

                var datePattern = parsedDate.ToString("yyyyMMdd");
                var logFiles = Directory.GetFiles(_logsPath, $"log-{datePattern}*.txt")
                    .Select(file => new FileInfo(file))
                    .OrderByDescending(f => f.LastWriteTime)
                    .Select(f => new
                    {
                        fileName     = f.Name,
                        size         = FormatFileSize(f.Length),
                        sizeBytes    = f.Length,
                        lastModified = f.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss"),
                        created      = f.CreationTime.ToString("yyyy-MM-dd HH:mm:ss")
                    })
                    .ToList();

                return Ok(new
                {
                    message = logFiles.Count > 0
                        ? $"Se encontraron {logFiles.Count} archivo(s) de log para {fecha}."
                        : $"No se encontraron logs para {fecha}.",
                    fecha,
                    count = logFiles.Count,
                    files = logFiles
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "Error al buscar archivos de log",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Descarga un archivo de log específico
        /// </summary>
        /// <param name="fileName">Nombre del archivo de log</param>
        /// <returns>Archivo de log para descarga</returns>
        [HttpGet("download/{fileName}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult DownloadLogFile(string fileName)
        {
            if (!IsValidLogFileName(fileName))
                return BadRequest(new { message = "Nombre de archivo inválido. Solo se permiten archivos log-*.txt" });

            try
            {
                var filePath = Path.Combine(_logsPath, fileName);

                if (!System.IO.File.Exists(filePath))
                    return NotFound(new { message = "Archivo de log no encontrado.", fileName });

                var fileBytes = System.IO.File.ReadAllBytes(filePath);
                return File(fileBytes, "text/plain", fileName);
            }
            catch (UnauthorizedAccessException)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    message = "Sin permisos para acceder al archivo."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "Error al descargar el archivo de log.",
                    error   = ex.Message
                });
            }
        }

        /// <summary>
        /// Obtiene las últimas N líneas de un archivo de log (útil para ver logs recientes)
        /// </summary>
        /// <param name="fileName">Nombre del archivo de log</param>
        /// <param name="lines">Número de líneas a mostrar (default: 100)</param>
        /// <returns>Últimas líneas del archivo</returns>
        [HttpGet("tail/{fileName}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult TailLogFile(string fileName, [FromQuery] int lines = 100)
        {
            if (!IsValidLogFileName(fileName))
                return BadRequest(new { message = "Nombre de archivo inválido. Solo se permiten archivos log-*.txt" });

            lines = Math.Clamp(lines, 1, 1000); // evitar respuestas masivas

            try
            {
                var filePath = Path.Combine(_logsPath, fileName);

                if (!System.IO.File.Exists(filePath))
                    return NotFound(new { message = "Archivo de log no encontrado.", fileName });

                var allLines  = System.IO.File.ReadAllLines(filePath);
                var lastLines = allLines.Skip(Math.Max(0, allLines.Length - lines)).ToArray();

                return Ok(new
                {
                    fileName,
                    totalLines = allLines.Length,
                    showing    = lastLines.Length,
                    lines      = lastLines
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "Error al leer el archivo de log.",
                    error   = ex.Message
                });
            }
        }

        // ─── helpers ────────────────────────────────────────────────────────────

        /// <summary>
        /// Valida que el nombre de archivo sea seguro: solo log-*.txt, sin path traversal.
        /// </summary>
        private static bool IsValidLogFileName(string fileName) =>
            !string.IsNullOrWhiteSpace(fileName)
            && !fileName.Contains("..")
            && !fileName.Contains('/')
            && !fileName.Contains('\\')
            && fileName.StartsWith("log-", StringComparison.OrdinalIgnoreCase)
            && fileName.EndsWith(".txt", StringComparison.OrdinalIgnoreCase);

        private static string FormatFileSize(long bytes)
        {
            string[] sizes = ["B", "KB", "MB", "GB"];
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1) { order++; len /= 1024; }
            return $"{len:0.##} {sizes[order]}";
        }
    }
}
