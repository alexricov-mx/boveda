using Microsoft.AspNetCore.Http;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace BERRecepcion.Front.Middleware
{
    /// <summary>
    /// Middleware para prevenir el doble procesamiento del callback de Azure AD
    /// que causa el error AADSTS54005
    /// </summary>
    public class PreventDoublePostMiddleware
    {
        private readonly RequestDelegate _next;
        private static readonly ConcurrentDictionary<string, DateTime> _processingRequests = new ConcurrentDictionary<string, DateTime>();
        
        // Limpiar entradas antiguas cada 2 minutos
        private static DateTime _lastCleanup = DateTime.UtcNow;
        private static readonly TimeSpan CleanupInterval = TimeSpan.FromMinutes(2);
        private static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(20); // Timeout ampliado para NetScaler

        public PreventDoublePostMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Solo aplicar a POST requests a signin-oidc
            if (context.Request.Method == "POST" && 
                context.Request.Path.StartsWithSegments("/signin-oidc"))
            {
                Log.Warning($"⚠️ POST a /signin-oidc recibido - Verificando duplicates");
                
                // Limpiar entradas antiguas periódicamente
                if (DateTime.UtcNow - _lastCleanup > CleanupInterval)
                {
                    CleanupOldRequests();
                }

                // Crear identificador basado SOLO en IP + ventana de 2 segundos
                // Esto bloqueará CUALQUIER POST duplicado del mismo IP en 2 segundos
                var requestId = GetRequestIdentifier(context);
                
                Log.Warning($"🔑 RequestId: {requestId}, Requests en proceso: {_processingRequests.Count}");
                
                // Verificar si ya se está procesando este request
                if (_processingRequests.TryGetValue(requestId, out DateTime startTime))
                {
                    var elapsed = DateTime.UtcNow - startTime;
                    
                    if (elapsed < RequestTimeout)
                    {
                        // Duplicate request dentro del timeout - ignorarlo
                        Log.Warning($"⚠ Duplicate POST a /signin-oidc detectado y bloqueado. RequestId: {requestId}, Elapsed: {elapsed.TotalSeconds}s");
                        
                        // Si el usuario ya está autenticado, redirigir
                        if (context.User?.Identity?.IsAuthenticated == true)
                        {
                            Log.Information($"✓ Usuario ya autenticado. Redirigiendo a inicio.");
                            context.Response.Redirect("/");
                            return;
                        }
                        
                        // Esperar un momento y redirigir
                        await Task.Delay(500);
                        context.Response.Redirect("/?wait=1");
                        return;
                    }
                    else
                    {
                        // Timeout alcanzado, permitir reintentar
                        _processingRequests.TryRemove(requestId, out _);
                        Log.Information($"Request timeout alcanzado. Permitiendo reintento. RequestId: {requestId}");
                    }
                }
                
                // Marcar como en procesamiento
                _processingRequests.TryAdd(requestId, DateTime.UtcNow);
                Log.Warning($"✅ Request {requestId} EN PROCESAMIENTO");
                
                try
                {
                    // Procesar el request normalmente
                    await _next(context);
                    Log.Warning($"✓ Request {requestId} COMPLETADO");
                }
                finally
                {
                    // Mantener el lock por 25 segundos después de completar (NetScaler varía entre 10-21 segundos según logs)
                    await Task.Delay(25000);
                    _processingRequests.TryRemove(requestId, out _);
                    Log.Warning($"🗑 Request {requestId} REMOVIDO del tracking");
                }
            }
            else
            {
                // No es un POST a signin-oidc, procesar normalmente
                await _next(context);
            }
        }

        private string GetRequestIdentifier(HttpContext context)
        {
            // Estrategia: Leer el código OAuth del form body
            // Este código es único por intento de autenticación
            string oauthCode = null;
            
            try
            {
                // Habilitar buffering para poder leer el body múltiples veces
                context.Request.EnableBuffering();
                
                // Leer el body
                using (var reader = new StreamReader(
                    context.Request.Body,
                    encoding: Encoding.UTF8,
                    detectEncodingFromByteOrderMarks: false,
                    bufferSize: 1024,
                    leaveOpen: true))
                {
                    var body = reader.ReadToEndAsync().Result;
                    
                    // Resetear la posición del stream para que el siguiente middleware pueda leerlo
                    context.Request.Body.Position = 0;
                    
                    // Buscar el parámetro 'code=' en el body
                    if (!string.IsNullOrEmpty(body) && body.Contains("code="))
                    {
                        var codeStart = body.IndexOf("code=") + 5;
                        var codeEnd = body.IndexOf("&", codeStart);
                        if (codeEnd == -1) codeEnd = body.Length;
                        
                        oauthCode = body.Substring(codeStart, codeEnd - codeStart);
                        
                        // Tomar solo los primeros 20 caracteres para el log
                        var codePreview = oauthCode.Length > 20 ? oauthCode.Substring(0, 20) + "..." : oauthCode;
                        Log.Warning($"🔐 Código OAuth detectado: {codePreview}");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Warning($"⚠ Error al leer body: {ex.Message}");
            }
            
            // Si logramos obtener el código OAuth, usarlo como ID
            if (!string.IsNullOrEmpty(oauthCode))
            {
                var requestId = $"code_{oauthCode.GetHashCode():X}";
                Log.Warning($"📝 RequestId basado en OAuth code: {requestId}");
                return requestId;
            }
            
            // Fallback: IP + ventana de 10 segundos (más amplia para NetScaler)
            var ip = context.Connection.RemoteIpAddress?.ToString() ?? "no-ip";
            var now = DateTime.UtcNow;
            var windowSeconds = (now.Second / 10) * 10; // 0, 10, 20, 30, 40, 50
            var timeWindow = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, windowSeconds);
            
            var fallbackId = $"{ip}_{timeWindow:yyyyMMddHHmmss}";
            Log.Warning($"📝 RequestId fallback (sin OAuth code): {fallbackId}");
            
            return fallbackId;
        }

        private void CleanupOldRequests()
        {
            _lastCleanup = DateTime.UtcNow;
            var cutoff = DateTime.UtcNow.Add(-RequestTimeout);
            
            foreach (var kvp in _processingRequests)
            {
                if (kvp.Value < cutoff)
                {
                    _processingRequests.TryRemove(kvp.Key, out _);
                }
            }
            
            Log.Debug($"Cleanup completado. Requests activos: {_processingRequests.Count}");
        }
    }
}
