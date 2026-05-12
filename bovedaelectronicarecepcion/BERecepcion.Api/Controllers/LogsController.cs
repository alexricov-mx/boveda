using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BERecepcion.Api.Controllers
{
    /// <summary>
    /// Controller para gestionar y descargar logs de la aplicación
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class LogsController : ControllerBase
    {
        private readonly string _logsPath;

        public LogsController()
        {
            // Ruta donde Serilog guarda los logs según appsettings.json
            _logsPath = Path.Combine(AppContext.BaseDirectory, "logs");
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
                        fileName = f.Name,
                        fullPath = f.FullName,
                        size = FormatFileSize(f.Length),
                        sizeBytes = f.Length,
                        lastModified = f.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss"),
                        created = f.CreationTime.ToString("yyyy-MM-dd HH:mm:ss")
                    })
                    .ToList();

                if (!logFiles.Any())
                {
                    return Ok(new
                    {
                        message = "No se encontraron archivos de log",
                        path = _logsPath,
                        files = new List<object>()
                    });
                }

                return Ok(new
                {
                    message = $"Se encontraron {logFiles.Count} archivo(s) de log",
                    path = _logsPath,
                    files = logFiles
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "Error al listar los archivos de log",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
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
                        fileName = f.Name,
                        fullPath = f.FullName,
                        size = FormatFileSize(f.Length),
                        sizeBytes = f.Length,
                        lastModified = f.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss"),
                        created = f.CreationTime.ToString("yyyy-MM-dd HH:mm:ss")
                    })
                    .ToList();

                return Ok(new
                {
                    message = logFiles.Any() 
                        ? $"Se encontraron {logFiles.Count} archivo(s) de log para {fecha}" 
                        : $"No se encontraron logs para {fecha}",
                    fecha = fecha,
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
            try
            {
                // Validar que el nombre del archivo no contenga caracteres peligrosos
                if (fileName.Contains("..") || fileName.Contains("/") || fileName.Contains("\\"))
                {
                    return BadRequest(new
                    {
                        message = "Nombre de archivo inválido"
                    });
                }

                // Solo permitir archivos .txt que empiecen con "log-"
                if (!fileName.StartsWith("log-") || !fileName.EndsWith(".txt"))
                {
                    return BadRequest(new
                    {
                        message = "Solo se pueden descargar archivos de log válidos (log-*.txt)"
                    });
                }

                var filePath = Path.Combine(_logsPath, fileName);

                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound(new
                    {
                        message = "Archivo de log no encontrado",
                        fileName = fileName,
                        searchPath = _logsPath
                    });
                }

                var fileBytes = System.IO.File.ReadAllBytes(filePath);
                return File(fileBytes, "text/plain", fileName);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    message = "No tiene permisos para acceder al archivo",
                    error = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "Error al descargar el archivo de log",
                    error = ex.Message
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
            try
            {
                if (fileName.Contains("..") || fileName.Contains("/") || fileName.Contains("\\"))
                {
                    return BadRequest(new { message = "Nombre de archivo inválido" });
                }

                if (!fileName.StartsWith("log-") || !fileName.EndsWith(".txt"))
                {
                    return BadRequest(new { message = "Solo se pueden leer archivos de log válidos" });
                }

                var filePath = Path.Combine(_logsPath, fileName);

                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound(new { message = "Archivo de log no encontrado", fileName = fileName });
                }

                var allLines = System.IO.File.ReadAllLines(filePath);
                var lastLines = allLines.Skip(Math.Max(0, allLines.Length - lines)).ToArray();

                return Ok(new
                {
                    fileName = fileName,
                    totalLines = allLines.Length,
                    showing = lastLines.Length,
                    lines = lastLines
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "Error al leer el archivo de log",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Formatea el tamaño del archivo a un formato legible
        /// </summary>
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
