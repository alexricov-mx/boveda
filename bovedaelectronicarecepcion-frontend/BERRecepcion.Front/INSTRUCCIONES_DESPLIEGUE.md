# 🚀 Instrucciones de Despliegue - Solución AADSTS54005

## Para Desarrolladores

### 1. Publicar la Aplicación
```bash
# Visual Studio: Botón "Publicar" o desde consola:
dotnet publish -c Release
```

### 2. Desplegar en IIS
- Publicar en la carpeta habitual del sitio IIS
- **No se requiere configuración adicional**
- El sitio IIS ya configurado por infraestructura es suficiente

### 3. Verificar Funcionamiento
Acceder a: `https://ber.qa.pemex.com/`

**Buscar en logs de Serilog**:
```
[Info] Carpeta de Data Protection creada/verificada en: [ruta]\App_Data\DataProtectionKeys
```

✅ **Si aparece este mensaje** → Todo funcionó correctamente

⚠️ **Si aparece**: `No se pudo configurar persistencia de claves en archivo`
   → Funcional pero contactar a infraestructura para optimizar

---

## Para Infraestructura (solo si hay problemas)

### Verificación Opcional
Si el equipo de desarrollo reporta problemas con persistencia:

1. **Navegar a la carpeta del sitio IIS**:
   ```
   cd C:\inetpub\wwwroot\ber
   ```

2. **Verificar que existe `App_Data`**:
   ```powershell
   Test-Path "C:\inetpub\wwwroot\ber\App_Data"
   ```

3. **Verificar permisos del App Pool** (debería estar OK por defecto):
   ```powershell
   icacls "C:\inetpub\wwwroot\ber\App_Data"
   ```
   
   Debe incluir: `IIS AppPool\[NombreAppPool]:(OI)(CI)(M)`

4. **Si faltan permisos** (muy raro):
   ```powershell
   $path = "C:\inetpub\wwwroot\ber\App_Data"
   $appPool = "IIS AppPool\ber"  # Ajustar nombre del app pool
   icacls $path /grant "${appPool}:(OI)(CI)(M)" /T
   ```

---

## Checklist de Despliegue

### Pre-Despliegue
- [ ] Código compilado sin errores
- [ ] Tests de autenticación pasando
- [ ] Archivos publicados generados

### Despliegue
- [ ] Archivos copiados a servidor IIS
- [ ] App Pool reciclado (opcional)
- [ ] Sitio web iniciado

### Post-Despliegue
- [ ] Login con usuario de prueba exitoso
- [ ] Logs de Serilog verificados
- [ ] Mensaje de "Data Protection creada" aparece
- [ ] No hay errores en Event Viewer de Windows

### Prueba de Resiliencia (opcional)
- [ ] Reciclar App Pool manualmente
- [ ] Refrescar navegador
- [ ] Usuario puede re-autenticarse sin ver error técnico

---

## 🔴 Si algo sale mal

### Síntoma: Usuario ve error AADSTS54005
**Diagnóstico**:
1. Revisar logs de Serilog
2. Buscar: `Código de autorización ya redimido`
3. Buscar: `No se pudo configurar persistencia`

**Solución inmediata**: 
- El sistema se auto-recupera, usuario solo necesita volver a hacer login
- Si se repite constantemente, verificar permisos de `App_Data`

### Síntoma: "Access to the path is denied"
**Diagnóstico**: Permisos insuficientes en `App_Data`

**Solución**:
```powershell
# En servidor IIS como administrador:
icacls "C:\inetpub\wwwroot\ber\App_Data" /grant "IIS AppPool\ber:(OI)(CI)(M)" /T
iisreset
```

### Síntoma: Logs no aparecen
**Diagnóstico**: Serilog no configurado o sin permisos

**Solución**: Verificar `appsettings.json` y permisos de carpeta de logs

---

## 📞 Escalación

Si los pasos anteriores no resuelven:

1. **Capturar información**:
   - Logs de Serilog (últimas 100 líneas)
   - Event Viewer → Windows Logs → Application (filtrar por IIS)
   - Screenshot del error (si es visible al usuario)

2. **Información del ambiente**:
   - Versión de Windows Server
   - Versión de IIS
   - Nombre del App Pool
   - Ruta física del sitio

3. **Compartir** con equipo de desarrollo

---

## ✅ Criterios de Éxito

El despliegue es exitoso cuando:

1. ✅ Usuario puede autenticarse con Azure AD
2. ✅ No ve mensajes de error técnicos
3. ✅ Logs muestran: "Carpeta de Data Protection creada"
4. ✅ Después de reciclar App Pool, puede re-autenticarse
5. ✅ No hay errores en Event Viewer relacionados con la aplicación

---

## 📝 Notas Técnicas

- **Data Protection Keys** se guardan en: `[RutaSitio]\App_Data\DataProtectionKeys`
- Las claves se **rotan automáticamente** cada 90 días
- El sistema tiene **fallback** a protección en memoria si falla la persistencia
- Los errores de Azure AD se **manejan automáticamente** sin intervención

---

## 🎓 Para Referencia

**Documentación completa**: Ver `CONFIGURACION_IIS.md`  
**Resumen técnico**: Ver `RESUMEN_SOLUCION.md`  
**Código fuente**: `Startup.cs` (líneas 47-130)

---

**Última actualización**: Enero 2026  
**Responsable**: Equipo de Desarrollo BER  
**Estado**: ✅ Listo para producción
