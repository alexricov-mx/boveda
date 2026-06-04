# 📋 Mapeo Completo: Roles de BD → Políticas de Autorización

Este documento proporciona una referencia rápida del mapeo 1:1 entre:
- **Roles** (`RoleConstants.cs`): valores del campo `Rol` en la tabla `RolesCatalogo` de la BD
- **Políticas** (`PolicyConstants.cs`): nombres de políticas registradas en `Program.cs`

## 🔍 Tabla de Mapeo Completo

| # | Rol de BD (RoleConstants) | Política de Autorización (PolicyConstants) | Categoría |
|---|---------------------------|-------------------------------------------|-----------|
| 0 | *(ninguno)* | `RequireAuthenticatedUser` | Base |
| 1 | `AdministrationUsers` | `RequireAdministrationUsers` | Administración |
| 2 | `AdministrationProfiles` | `RequireAdministrationProfiles` | Administración |
| 3 | `AdministrationManagementCenters` | `RequireAdministrationManagementCenters` | Administración |
| 4 | `AdministrationInterfases` ⚠️ | `RequireAdministrationInterfaces` | Administración |
| 5 | `AdministrationLog` | `RequireAdministrationLog` | Administración |
| 6 | `AdministrationLogUsers` | `RequireAdministrationLogUsers` | Administración |
| 7 | `AdministrationAdefa` | `RequireAdministrationAdefa` | Administración |
| 8 | `AdministrationValidations` | `RequireAdministrationValidations` | Administración |
| 9 | `ContractsRegister` | `RequireContractsRegister` | Administración |
| 10 | `DeviationSigns` | `RequireDeviationSigns` | Administración |
| 11 | `ReportEmails` | `RequireReportEmails` | Consulta |
| 12 | `ReportDesviationSigns` | `RequireReportDesviationSigns` | Consulta |
| 13 | `ReportRejectedInvoice` | `RequireReportRejectedInvoice` | Consulta |
| 14 | `ReportRejectedInvoiceAnalytic` | `RequireReportRejectedInvoiceAnalytic` | Consulta |
| 15 | `QueryPrefecture` | `RequireQueryPrefecture` | Consulta |
| 16 | `QueryPrefectureAnalytic` | `RequireQueryPrefectureAnalytic` | Consulta |
| 17 | `QueryTracing` | `RequireQueryTracing` | Consulta |
| 18 | `StatisticsSupplyOrders` | `RequireStatisticsSupplyOrders` | Consulta |
| 19 | `StatisticsCopade` | `RequireStatisticsCopade` | Consulta |
| 20 | `StatisticsCopadeBanking` | `RequireStatisticsCopadeBanking` | Consulta |
| 21 | `StatisticsReceptionInvoice` | `RequireStatisticsReceptionInvoice` | Consulta |
| 22 | `StatisticsPaymentSchedule` | `RequireStatisticsPaymentSchedule` | Consulta |
| 23 | `StatisticsPaymentList` | `RequireStatisticsPaymentList` | Consulta |
| 24 | `StatisticsAnalyticalPayment` | `RequireStatisticsAnalyticalPayment` | Consulta |
| 25 | `StatisticsSOEstimations` | `RequireStatisticsSOEstimations` | Consulta |
| 26 | `StatisticsBankEstimate` | `RequireStatisticsBankEstimate` | Consulta |
| 27 | `StatisticsBankOrder` | `RequireStatisticsBankOrder` | Consulta |
| 28 | `ReceptionSignCopade` | `RequireReceptionSignCopade` | COPADE |
| 29 | `CancelCopade` | `RequireCancelCopade` | COPADE |
| 30 | `AnalyticalPayment` | `RequireAnalyticalPayment` | COPADE |
| 31 | `StatisticsUsuariosEPS` | `RequireStatisticsUsuariosEPS` | Estadísticas |
| 32 | `StatisticsFacturasrecibidas` | `RequireStatisticsFacturasrecibidas` | Estadísticas |
| 33 | `StatisticsCopadesingresados` | `RequireStatisticsCopadesingresados` | Estadísticas |
| 34 | `StatisticsCopadespendientes` | `RequireStatisticsCopadespendientes` | Estadísticas |
| 35 | `StatisticsOrdenesrecibidas` | `RequireStatisticsOrdenesrecibidas` | Estadísticas |
| 36 | `StatisticsCopadescancelados` | `RequireStatisticsCopadescancelados` | Estadísticas |
| 37 | `StatisticsEstimacionesrecibidas` | `RequireStatisticsEstimacionesrecibidas` | Estadísticas |
| 38 | `AdministrationCxpExecution` | `RequireAdministrationCxpExecution` | Facturas |
| 39 | `AdministrationCxpExecutionAP` | `RequireAdministrationCxpExecutionAP` | Facturas |
| 40 | `ReceptionElectronicInvoice` | `RequireReceptionElectronicInvoice` | Facturas |
| 41 | `ReceptionElectronicAnalyticPaymentInvoice` | `RequireReceptionElectronicAnalyticPaymentInvoice` | Facturas |
| 42 | `ReceptionDocumentalInvoice` | `RequireReceptionDocumentalInvoice` | Facturas |
| 43 | `ReceptionDocumentalInvoiceFromXML` | `RequireReceptionDocumentalInvoiceFromXML` | Facturas |
| 44 | `ReceptionDocumentalAnalyticPaymentInvoice` | `RequireReceptionDocumentalAnalyticPaymentInvoice` | Facturas |
| 45 | `ReceptionDocumentalAnalyticPaymentInvoiceFromXML` | `RequireReceptionDocumentalAnalyticPaymentInvoiceFromXML` | Facturas |
| 46 | `ReceptionElectronicInvoiceREP` | `RequireReceptionElectronicInvoiceREP` | Facturas |
| 47 | `ReceptionElectronicMultipleInvoice` | `RequireReceptionElectronicMultipleInvoice` | Facturas |
| 48 | `ReceptionSignPaymentSchedule` | `RequireReceptionSignPaymentSchedule` | Instrucciones |
| 49 | `PaymentList` | `RequirePaymentList` | Instrucciones |
| 50 | `CancelPaymentSchedule` | `RequireCancelPaymentSchedule` | Instrucciones |
| 51 | `CancelPaymentList` | `RequireCancelPaymentList` | Instrucciones |
| 52 | `ReceptionSignSupplyOrders` | `RequireReceptionSignSupplyOrders` | OS / Recepción |
| 53 | `ReceptionSignSOEstimations` | `RequireReceptionSignSOEstimations` | OS / Recepción |
| 54 | `ReceptionSignReception` | `RequireReceptionSignReception` | OS / Recepción |

⚠️ **Nota sobre typo en BD**: El rol #4 en la BD se llama `AdministrationInterfases` (con "s"), pero la constante pública usa el nombre correcto `AdministrationInterfaces` para evitar propagar el error.

---

## 📖 Uso en Código

### En Controllers (atributo `[Authorize]`)

```csharp
[Authorize(Policy = PolicyConstants.RequireAdministrationUsers)]
public IActionResult GetUsers() { ... }
```

### En Lógica de Negocio (validación manual)

```csharp
if (_currentUser.IsInRole(RoleConstants.AdministrationUsers))
{
    // Usuario tiene el rol
}
```

### En Program.cs (registro de política)

```csharp
options.AddPolicy(PolicyConstants.RequireAdministrationUsers, policy =>
    policy.RequireAuthenticatedUser()
          .RequireClaim(ApiAuthConstants.RoleBdClaim, RoleConstants.AdministrationUsers));
```

---

## 🔍 Búsqueda Rápida por Categoría

### Administración (10 roles)
- Usuarios, Perfiles, Centros de gestión, Interfaces, Bitácora, Bitácora usuarios, ADEFA, Validaciones, Contratos, Desvíos

### Consulta (17 roles)
- Reportes (emails, desvíos, facturas rechazadas), Consultas (prefectura, trazabilidad), Estadísticas (OS, COPADE, facturas, instrucciones, estimaciones)

### COPADE / Analítico (3 roles)
- Firma COPADE, Cancelación COPADE, Pago analítico

### Estadísticas (7 roles)
- Usuarios EPS, Facturas recibidas, COPADEs (ingresados, pendientes, cancelados), Órdenes recibidas, Estimaciones recibidas

### Facturas (10 roles)
- Ejecución CxP, Facturas electrónicas, Facturas documentales, Facturas con XML, Facturas REP, Facturas múltiples

### Instrucciones (4 roles)
- Firma programación de pago, Lista de pagos, Cancelación programación, Cancelación lista

### OS / Recepción (3 roles)
- Órdenes de surtimiento, Estimaciones de OS, Recepción de almacén

---

## ✅ Validación de Consistencia

- ✅ Cada rol en `RoleConstants` tiene su política correspondiente en `PolicyConstants`
- ✅ Cada política está registrada en `Program.cs` con `AddPolicy(...)`
- ✅ Los valores de `RoleConstants` coinciden exactamente con los valores de la BD
- ✅ La convención de nombres es consistente: `Require{RoleName}`

---

**Última actualización**: 2025  
**Total de políticas**: 54 + 1 base = **55 políticas de autorización**
