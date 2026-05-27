using System;
using System.IO;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;

namespace BERRecepcion.Front.Modules.Directories;

public static class DirectoriesExtensions
{
    public static IServiceCollection AddDirectoriesExtensions(this IServiceCollection services)
    {
        DirectoryInfo keysDirectory = null;
        var keysPaths = new[]
        {
            Path.Combine(AppContext.BaseDirectory, "App_Data", "DataProtectionKeys"),
            Path.Combine(AppContext.BaseDirectory, "DataProtectionKeys"),
            Path.Combine(Path.GetTempPath(), "BERRecepcion", "DataProtectionKeys")
        };
            
        foreach (var path in keysPaths)
        {
            try
            {
                Directory.CreateDirectory(path);
                // Intentar escribir un archivo de prueba
                var testFile = Path.Combine(path, "test.txt");
                File.WriteAllText(testFile, "test");
                File.Delete(testFile);
                    
                keysDirectory = new DirectoryInfo(path);
                Serilog.Log.Information($"✓ Data Protection Keys configuradas en: {path}");
                break;
            }
            catch (Exception ex)
            {
                Serilog.Log.Warning($"✗ No se pudo usar {path}: {ex.Message}");
            }
        }
            
        if (keysDirectory != null)
        {
            services.AddDataProtection()
                .PersistKeysToFileSystem(keysDirectory)
                .SetApplicationName("BERRecepcion.Front")
                .SetDefaultKeyLifetime(TimeSpan.FromDays(90));
        }
        else
        {
            Serilog.Log.Error("⚠ ADVERTENCIA: No se pudo configurar persistencia de claves. Usando protección en memoria (las sesiones se perderán al reciclar App Pool)");
            services.AddDataProtection()
                .SetApplicationName("BERRecepcion.Front");
        }

        return services;
    }
}