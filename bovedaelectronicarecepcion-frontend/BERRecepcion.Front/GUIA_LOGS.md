# 📋 Guía de Uso - Sistema de Gestión de Logs

## ✅ Implementación Completada

Se han creado controllers en **Frontend** y **Backend** para gestionar y descargar logs sin necesidad de acceso al servidor.

---

## 🎯 Frontend (Interfaz Web)

### Acceso
```
https://ber.dev.pemex.com/Logs
https://ber.qa.pemex.com/Logs
```

### Funcionalidades Disponibles

#### 1. **Vista Principal** (`/Logs`)
- Lista todos los archivos de log
- Muestra últimos 20 archivos más recientes
- Filtro por fecha
- Botones de acción: Ver y Descargar

#### 2. **Ver Contenido** (`/Logs/View?fileName=log-20260114.txt&lines=100`)
- Visualiza el contenido del log en el navegador
- Muestra las últimas N líneas (configurable)
- Función de búsqueda en tiempo real
- Copiar al portapapeles
- Scroll automático al final

#### 3. **Descargar** (`/Logs/Download?fileName=log-20260114.txt`)
- Descarga directa del archivo de log
- Formato texto plano (.txt)

### Métodos del Controller

```csharp
// Frontend: BERRecepcion.Front/Controllers/LogsController.cs

GET  /Logs/Index                     // Vista principal
GET  /Logs/List                      // API: Lista todos los logs (JSON)
GET  /Logs/ListByDate?fecha=yyyy-MM-dd  // API: Filtrar por fecha (JSON)
GET  /Logs/Download?fileName=X       // Descarga archivo
GET  /Logs/View?fileName=X&lines=100 // Vista del contenido
```

---

## 🔧 Backend (API)

### Endpoints Disponibles

```
Base URL Dev: https://ber.back.dev.pemex.com/api
Base URL QA:  https://ber.back.qa.pemex.com/api
```

#### 1. **Listar Todos los Logs**
```http
GET /api/Logs/list
```

**Respuesta:**
```json
{
  "message": "Se encontraron 15 archivo(s) de log",
  "path": "C:\\inetpub\\wwwroot\\ber\\logs",
  "files": [
    {
      "fileName": "log-20260114.txt",
      "fullPath": "C:\\...",
      "size": "256.5 KB",
      "sizeBytes": 262656,
      "lastModified": "2026-01-14 15:30:45",
      "created": "2026-01-14 00:00:01"
    }
  ]
}
```

#### 2. **Listar Logs por Fecha**
```http
GET /api/Logs/list/2026-01-14
```

**Respuesta:**
```json
{
  "message": "Se encontraron 3 archivo(s) de log para 2026-01-14",
  "fecha": "2026-01-14",
  "files": [...]
}
```

#### 3. **Descargar Log**
```http
GET /api/Logs/download/log-20260114.txt
```

**Respuesta:** Archivo de texto plano

#### 4. **Ver Últimas Líneas (Tail)**
```http
GET /api/Logs/tail/log-20260114.txt?lines=100
```

**Respuesta:**
```json
{
  "fileName": "log-20260114.txt",
  "totalLines": 5000,
  "showing": 100,
  "lines": [
    "2026-01-14 15:30:45.123 [Error] ...",
    "..."
  ]
}
```

---

## 🚀 Uso Práctico

### Escenario 1: Revisar logs de hoy

**Opción A - Frontend (Recomendado):**
1. Ir a `https://ber.dev.pemex.com/Logs`
2. Ver lista automáticamente ordenada por fecha
3. Click en el icono 👁️ para ver el contenido
4. Buscar errores con Ctrl+F

**Opción B - API directa:**
```bash
curl https://ber.back.dev.pemex.com/api/Logs/list
```

### Escenario 2: Descargar logs de una fecha específica

**Frontend:**
1. Ir a `/Logs`
2. Seleccionar fecha en el calendario
3. Click "Buscar por Fecha"
4. Click en 📥 para descargar

**API:**
```bash
# Listar
curl https://ber.back.dev.pemex.com/api/Logs/list/2026-01-14

# Descargar
curl https://ber.back.dev.pemex.com/api/Logs/download/log-20260114.txt --output log.txt
```

### Escenario 3: Ver errores recientes rápidamente

**Frontend:**
1. Ir a `/Logs/View?fileName=log-20260114.txt&lines=50`
2. Ctrl+F
3. Buscar: "Error" o "Exception"

**API:**
```bash
curl "https://ber.back.dev.pemex.com/api/Logs/tail/log-20260114.txt?lines=50"
```

---

## 🔐 Seguridad

### Validaciones Implementadas

✅ **Autenticación requerida** en Frontend (atributo `[Authorize]`)  
✅ **Validación de nombres de archivo** (solo `log-*.txt`)  
✅ **Protección contra path traversal** (no permite `..`, `/`, `\`)  
✅ **Solo lectura** (no se pueden modificar o eliminar logs)  
✅ **Logs detallados de accesos** con Serilog  

### Permisos del Servidor

El usuario del App Pool debe tener permisos de **lectura** en:
```
[RutaDelSitio]\logs\
```

---

## 📊 Formato de Logs

Los logs generados por Serilog siguen este formato:

```
2026-01-14 15:30:45.123 +00:00 [Error] Error de autenticación: message.State is null
System.Exception: ...
   at BERRecepcion.Front.Startup...
```

**Buscar por:**
- `[Error]` - Todos los errores
- `[Warning]` - Advertencias
- `[Information]` - Eventos informativos
- `AADSTS54005` - Error específico de Azure AD
- `message.State is null` - Error de correlación
- `Data Protection` - Problemas con claves de encriptación

---

## 🧪 Testing Post-Despliegue

### 1. Verificar que los endpoints funcionan

**Frontend:**
```
✓ https://ber.dev.pemex.com/Logs
✓ https://ber.dev.pemex.com/Logs/List
```

**Backend:**
```
✓ https://ber.back.dev.pemex.com/api/Logs/list
```

### 2. Buscar mensajes clave en los logs

Después de intentar autenticarte en el servidor, busca en los logs:

```
✓ Data Protection Keys configuradas en: [ruta]
```
Si aparece → Todo OK, autenticación debería funcionar

```
✗ ADVERTENCIA: No se pudo configurar persistencia de claves
```
Si aparece → Problema de permisos, usar ubicación alternativa

```
❌ OnRemoteFailure disparado. Failure: AADSTS54005
```
Si aparece → Error de Azure AD, revisar Redirect URIs

---

## 📞 Troubleshooting

### Error: "No se encontró la carpeta de logs"

**Causa:** La carpeta `logs\` no existe en el servidor

**Solución:**
1. Verificar que Serilog esté escribiendo logs
2. Revisar ruta en `appsettings.json` → `Serilog:WriteTo:File:Args:path`
3. Verificar permisos de escritura del App Pool

### Error: "No tiene permisos para acceder al archivo"

**Causa:** App Pool sin permisos de lectura

**Solución:**
```powershell
# En el servidor (con permisos de admin)
icacls "C:\inetpub\wwwroot\ber\logs" /grant "IIS AppPool\ber:(OI)(CI)R"
```

### No aparecen logs recientes

**Causa:** Los logs se escriben en otra ubicación

**Solución:**
1. Ir a `/Logs` en el frontend
2. Ver la ruta mostrada en "📂 Ubicación de Logs"
3. Verificar si esa carpeta existe y tiene archivos

---

## 🎉 Resumen

✅ **Frontend:** Interfaz amigable en `/Logs`  
✅ **Backend:** API REST en `/api/Logs/`  
✅ **Funciones:** Listar, Filtrar, Descargar, Ver  
✅ **Seguridad:** Autenticación + validaciones  
✅ **Sin acceso al servidor requerido** 🎯  

**Próximo paso:** Publicar y probar en Development/Staging
