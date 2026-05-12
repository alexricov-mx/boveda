using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BERRecepcion.Front.Controllers
{
    /// <summary>
    /// Controller para gestionar y descargar logs del frontend
    /// IMPORTANTE: Sin autenticación para permitir diagnóstico de problemas de login
    /// </summary>
    [AllowAnonymous]
    public class LogsController : Controller
    {
        private readonly string _logsPath;

        public LogsController()
        {
            // Ruta donde Serilog guarda los logs según appsettings.json
            _logsPath = Path.Combine(AppContext.BaseDirectory, "logs");
        }

        /// <summary>
        /// Vista principal para gestionar logs
        /// </summary>
        [HttpGet]
        public IActionResult Index()
        {
            try
            {
                ViewBag.LogsPath = _logsPath;
                ViewBag.LogsExist = Directory.Exists(_logsPath);
                
                if (Directory.Exists(_logsPath))
                {
                    var logFiles = Directory.GetFiles(_logsPath, "log-*.txt")
                        .Select(file => new FileInfo(file))
                        .OrderByDescending(f => f.LastWriteTime)
                        .Take(20) // Mostrar últimos 20 archivos
                        .Select(f => new
                        {
                            FileName = f.Name,
                            Size = FormatFileSize(f.Length),
                            SizeBytes = f.Length,
                            LastModified = f.LastWriteTime,
                            Created = f.CreationTime
                        })
                        .ToList();

                    ViewBag.LogFiles = logFiles;
                    ViewBag.TotalFiles = Directory.GetFiles(_logsPath, "log-*.txt").Length;
                }
                else
                {
                    ViewBag.LogFiles = new List<object>();
                    ViewBag.TotalFiles = 0;
                }

                return View();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al cargar la vista de logs");
                ViewBag.Error = $"Error al cargar logs: {ex.Message}";
                ViewBag.LogFiles = new List<object>();
                ViewBag.TotalFiles = 0;
                return View();
            }
        }

        /// <summary>
        /// API endpoint: Lista todos los archivos de log disponibles
        /// </summary>
        [HttpGet]
        public IActionResult List()
        {
            try
            {
                if (!Directory.Exists(_logsPath))
                {
                    return Json(new
                    {
                        success = false,
                        message = "No se encontró la carpeta de logs",
                        path = _logsPath,
                        files = new List<object>()
                    });
                }

                var logFiles = Directory.GetFiles(_logsPath, "log-*.txt")
                    .Select(file => new FileInfo(file))
                    .OrderByDescending(f => f.LastWriteTime)
                    .Select(f => new
                    {
                        fileName = f.Name,
                        size = FormatFileSize(f.Length),
                        sizeBytes = f.Length,
                        lastModified = f.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss"),
                        created = f.CreationTime.ToString("yyyy-MM-dd HH:mm:ss")
                    })
                    .ToList();

                return Json(new
                {
                    success = true,
                    message = $"Se encontraron {logFiles.Count} archivo(s) de log",
                    path = _logsPath,
                    files = logFiles
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al listar archivos de log");
                return Json(new
                {
                    success = false,
                    message = "Error al listar los archivos de log",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// API endpoint: Lista archivos de log por fecha
        /// </summary>
        /// <param name="fecha">Fecha en formato yyyy-MM-dd</param>
        [HttpGet]
        public IActionResult ListByDate(string fecha)
        {
            try
            {
                if (!DateTime.TryParse(fecha, out DateTime parsedDate))
                {
                    return Json(new
                    {
                        success = false,
                        message = "Formato de fecha inválido. Use yyyy-MM-dd"
                    });
                }

                if (!Directory.Exists(_logsPath))
                {
                    return Json(new
                    {
                        success = false,
                        message = "No se encontró la carpeta de logs",
                        files = new List<object>()
                    });
                }

                var datePattern = parsedDate.ToString("yyyyMMdd");
                var logFiles = Directory.GetFiles(_logsPath, $"log-{datePattern}*.txt")
                    .Select(file => new FileInfo(file))
                    .OrderByDescending(f => f.LastWriteTime)
                    .Select(f => new
                    {
                        fileName = f.Name,
                        size = FormatFileSize(f.Length),
                        sizeBytes = f.Length,
                        lastModified = f.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss"),
                        created = f.CreationTime.ToString("yyyy-MM-dd HH:mm:ss")
                    })
                    .ToList();

                return Json(new
                {
                    success = true,
                    message = logFiles.Any() 
                        ? $"Se encontraron {logFiles.Count} archivo(s) de log para {fecha}" 
                        : $"No se encontraron logs para {fecha}",
                    fecha = fecha,
                    files = logFiles
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al buscar archivos de log por fecha");
                return Json(new
                {
                    success = false,
                    message = "Error al buscar archivos de log",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Descarga un archivo de log específico
        /// </summary>
        /// <param name="fileName">Nombre del archivo de log</param>
        [HttpGet]
        public IActionResult Download(string fileName)
        {
            try
            {
                // Validar que el nombre del archivo no contenga caracteres peligrosos
                if (string.IsNullOrWhiteSpace(fileName) || 
                    fileName.Contains("..") || 
                    fileName.Contains("/") || 
                    fileName.Contains("\\"))
                {
                    return BadRequest("Nombre de archivo inválido");
                }

                // Solo permitir archivos .txt que empiecen con "log-"
                if (!fileName.StartsWith("log-") || !fileName.EndsWith(".txt"))
                {
                    return BadRequest("Solo se pueden descargar archivos de log válidos (log-*.txt)");
                }

                var filePath = Path.Combine(_logsPath, fileName);

                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound($"Archivo de log no encontrado: {fileName}");
                }

                byte[] fileBytes;
                using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var memoryStream = new MemoryStream())
                {
                    fileStream.CopyTo(memoryStream);
                    fileBytes = memoryStream.ToArray();
                }
                return File(fileBytes, "text/plain", fileName);
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error(ex, "Error de permisos al descargar log: {FileName}", fileName);
                return StatusCode(StatusCodes.Status403Forbidden, "No tiene permisos para acceder al archivo");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al descargar archivo de log: {FileName}", fileName);
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error al descargar el archivo: {ex.Message}");
            }
        }

        /// <summary>
        /// Visualiza el contenido de un archivo de log
        /// </summary>
        /// <param name="fileName">Nombre del archivo de log</param>
        /// <param name="lines">Número de líneas a mostrar (últimas N líneas)</param>
        [HttpGet]
        public IActionResult View(string fileName, int lines = 100)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(fileName) || 
                    fileName.Contains("..") || 
                    fileName.Contains("/") || 
                    fileName.Contains("\\"))
                {
                    ViewBag.Error = "Nombre de archivo inválido";
                    return View();
                }

                if (!fileName.StartsWith("log-") || !fileName.EndsWith(".txt"))
                {
                    ViewBag.Error = "Solo se pueden ver archivos de log válidos";
                    return View();
                }

                var filePath = Path.Combine(_logsPath, fileName);

                if (!System.IO.File.Exists(filePath))
                {
                    ViewBag.Error = $"Archivo de log no encontrado: {fileName}";
                    return View();
                }

                var fileInfo = new FileInfo(filePath);
                string[] allLines;
                using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var reader = new StreamReader(fileStream))
                {
                    var content = reader.ReadToEnd();
                    allLines = content.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
                }
                var lastLines = allLines.Skip(Math.Max(0, allLines.Length - lines)).ToArray();

                ViewBag.FileName = fileName;
                ViewBag.FileSize = FormatFileSize(fileInfo.Length);
                ViewBag.LastModified = fileInfo.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss");
                ViewBag.TotalLines = allLines.Length;
                ViewBag.ShowingLines = lastLines.Length;
                ViewBag.LogContent = lastLines;
                ViewBag.RequestedLines = lines;

                return View();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al ver contenido del log: {FileName}", fileName);
                ViewBag.Error = $"Error al leer el archivo: {ex.Message}";
                return View();
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
