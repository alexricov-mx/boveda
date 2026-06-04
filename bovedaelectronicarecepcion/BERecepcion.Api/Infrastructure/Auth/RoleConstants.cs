namespace BERecepcion.Api.Infrastructure.Auth
{
    /// <summary>
    /// Constantes para todos los valores del campo <c>Rol</c> del catálogo <c>RolesCatalogo</c>.
    /// Los valores deben coincidir exactamente con los registrados en la BD (SP_rolescatalogo_selecciona).
    /// </summary>
    public static class RoleConstants
    {
        // ── Administración ────────────────────────────────────────────────────
        public const string AdministrationUsers            = "AdministrationUsers";
        public const string AdministrationProfiles         = "AdministrationProfiles";
        public const string AdministrationManagementCenters = "AdministrationManagementCenters";
        public const string AdministrationInterfaces       = "AdministrationInterfases"; // ⚠️ Typo en BD
        public const string AdministrationLog              = "AdministrationLog";
        public const string AdministrationLogUsers         = "AdministrationLogUsers";
        public const string AdministrationAdefa            = "AdministrationAdefa";
        public const string AdministrationValidations      = "AdministrationValidations";
        public const string ContractsRegister              = "ContractsRegister";
        public const string DeviationSigns                 = "DeviationSigns";

        // ── Consulta ──────────────────────────────────────────────────────────
        public const string ReportEmails                       = "ReportEmails";
        public const string ReportDesviationSigns              = "ReportDesviationSigns";
        public const string ReportRejectedInvoice              = "ReportRejectedInvoice";
        public const string ReportRejectedInvoiceAnalytic      = "ReportRejectedInvoiceAnalytic";
        public const string QueryPrefecture                    = "QueryPrefecture";
        public const string QueryPrefectureAnalytic            = "QueryPrefectureAnalytic";
        public const string QueryTracing                       = "QueryTracing";
        public const string StatisticsSupplyOrders             = "StatisticsSupplyOrders";
        public const string StatisticsCopade                   = "StatisticsCopade";
        public const string StatisticsCopadeBanking            = "StatisticsCopadeBanking";
        public const string StatisticsReceptionInvoice         = "StatisticsReceptionInvoice";
        public const string StatisticsPaymentSchedule          = "StatisticsPaymentSchedule";
        public const string StatisticsPaymentList              = "StatisticsPaymentList";
        public const string StatisticsAnalyticalPayment        = "StatisticsAnalyticalPayment";
        public const string StatisticsSOEstimations            = "StatisticsSOEstimations";
        public const string StatisticsBankEstimate             = "StatisticsBankEstimate";
        public const string StatisticsBankOrder                = "StatisticsBankOrder";

        // ── COPADE / Analítico ────────────────────────────────────────────────
        public const string ReceptionSignCopade = "ReceptionSignCopade";
        public const string CancelCopade        = "CancelCopade";
        public const string AnalyticalPayment   = "AnalyticalPayment";

        // ── Estadísticas ──────────────────────────────────────────────────────
        public const string StatisticsUsuariosEPS          = "StatisticsUsuariosEPS";
        public const string StatisticsFacturasrecibidas    = "StatisticsFacturasrecibidas";
        public const string StatisticsCopadesingresados    = "StatisticsCopadesingresados";
        public const string StatisticsCopadespendientes    = "StatisticsCopadespendientes";
        public const string StatisticsOrdenesrecibidas     = "StatisticsOrdenesrecibidas";
        public const string StatisticsCopadescancelados    = "StatisticsCopadescancelados";
        public const string StatisticsEstimacionesrecibidas = "StatisticsEstimacionesrecibidas";

        // ── Facturas ──────────────────────────────────────────────────────────
        public const string AdministrationCxpExecution                          = "AdministrationCxpExecution";
        public const string AdministrationCxpExecutionAP                        = "AdministrationCxpExecutionAP";
        public const string ReceptionElectronicInvoice                          = "ReceptionElectronicInvoice";
        public const string ReceptionElectronicAnalyticPaymentInvoice           = "ReceptionElectronicAnalyticPaymentInvoice";
        public const string ReceptionDocumentalInvoice                          = "ReceptionDocumentalInvoice";
        public const string ReceptionDocumentalInvoiceFromXML                   = "ReceptionDocumentalInvoiceFromXML";
        public const string ReceptionDocumentalAnalyticPaymentInvoice           = "ReceptionDocumentalAnalyticPaymentInvoice";
        public const string ReceptionDocumentalAnalyticPaymentInvoiceFromXML    = "ReceptionDocumentalAnalyticPaymentInvoiceFromXML";
        public const string ReceptionElectronicInvoiceREP                       = "ReceptionElectronicInvoiceREP";
        public const string ReceptionElectronicMultipleInvoice                  = "ReceptionElectronicMultipleInvoice";

        // ── Instrucciones ─────────────────────────────────────────────────────
        public const string ReceptionSignPaymentSchedule = "ReceptionSignPaymentSchedule";
        public const string PaymentList                  = "PaymentList";
        public const string CancelPaymentSchedule        = "CancelPaymentSchedule";
        public const string CancelPaymentList            = "CancelPaymentList";

        // ── OS / Recepción ────────────────────────────────────────────────────
        public const string ReceptionSignSupplyOrders  = "ReceptionSignSupplyOrders";
        public const string ReceptionSignSOEstimations = "ReceptionSignSOEstimations";
        public const string ReceptionSignReception     = "ReceptionSignReception";
    }
}
