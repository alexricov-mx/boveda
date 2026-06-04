namespace BERecepcion.Api.Infrastructure.Auth
{
    /// <summary>
    /// Constantes para los nombres de las políticas de autorización registradas en Program.cs.
    /// Se usan en los atributos <c>[Authorize(Policy = PolicyConstants.X)]</c> de los controllers.
    /// </summary>
    public static class PolicyConstants
    {
        /// <summary>El usuario debe estar autenticado (JWT válido). Sin restricción de rol.</summary>
        public const string RequireAuthenticatedUser = "RequireAuthenticatedUser";

        // ── Administración ────────────────────────────────────────────────────
        public const string RequireAdministrationUsers            = "RequireAdministrationUsers";
        public const string RequireAdministrationProfiles         = "RequireAdministrationProfiles";
        public const string RequireAdministrationManagementCenters = "RequireAdministrationManagementCenters";
        public const string RequireAdministrationInterfaces       = "RequireAdministrationInterfaces";
        public const string RequireAdministrationLog              = "RequireAdministrationLog";
        public const string RequireAdministrationLogUsers         = "RequireAdministrationLogUsers";
        public const string RequireAdministrationAdefa            = "RequireAdministrationAdefa";
        public const string RequireAdministrationValidations      = "RequireAdministrationValidations";
        public const string RequireContractsRegister              = "RequireContractsRegister";
        public const string RequireDeviationSigns                 = "RequireDeviationSigns";

        // ── Consulta ──────────────────────────────────────────────────────────
        public const string RequireReportEmails                       = "RequireReportEmails";
        public const string RequireReportDesviationSigns              = "RequireReportDesviationSigns";
        public const string RequireReportRejectedInvoice              = "RequireReportRejectedInvoice";
        public const string RequireReportRejectedInvoiceAnalytic      = "RequireReportRejectedInvoiceAnalytic";
        public const string RequireQueryPrefecture                    = "RequireQueryPrefecture";
        public const string RequireQueryPrefectureAnalytic            = "RequireQueryPrefectureAnalytic";
        public const string RequireQueryTracing                       = "RequireQueryTracing";
        public const string RequireStatisticsSupplyOrders             = "RequireStatisticsSupplyOrders";
        public const string RequireStatisticsCopade                   = "RequireStatisticsCopade";
        public const string RequireStatisticsCopadeBanking            = "RequireStatisticsCopadeBanking";
        public const string RequireStatisticsReceptionInvoice         = "RequireStatisticsReceptionInvoice";
        public const string RequireStatisticsPaymentSchedule          = "RequireStatisticsPaymentSchedule";
        public const string RequireStatisticsPaymentList              = "RequireStatisticsPaymentList";
        public const string RequireStatisticsAnalyticalPayment        = "RequireStatisticsAnalyticalPayment";
        public const string RequireStatisticsSOEstimations            = "RequireStatisticsSOEstimations";
        public const string RequireStatisticsBankEstimate             = "RequireStatisticsBankEstimate";
        public const string RequireStatisticsBankOrder                = "RequireStatisticsBankOrder";

        // ── COPADE / Analítico ────────────────────────────────────────────────
        public const string RequireReceptionSignCopade = "RequireReceptionSignCopade";
        public const string RequireCancelCopade        = "RequireCancelCopade";
        public const string RequireAnalyticalPayment   = "RequireAnalyticalPayment";

        // ── Estadísticas ──────────────────────────────────────────────────────
        public const string RequireStatisticsUsuariosEPS          = "RequireStatisticsUsuariosEPS";
        public const string RequireStatisticsFacturasrecibidas    = "RequireStatisticsFacturasrecibidas";
        public const string RequireStatisticsCopadesingresados    = "RequireStatisticsCopadesingresados";
        public const string RequireStatisticsCopadespendientes    = "RequireStatisticsCopadespendientes";
        public const string RequireStatisticsOrdenesrecibidas     = "RequireStatisticsOrdenesrecibidas";
        public const string RequireStatisticsCopadescancelados    = "RequireStatisticsCopadescancelados";
        public const string RequireStatisticsEstimacionesrecibidas = "RequireStatisticsEstimacionesrecibidas";

        // ── Facturas ──────────────────────────────────────────────────────────
        public const string RequireAdministrationCxpExecution                          = "RequireAdministrationCxpExecution";
        public const string RequireAdministrationCxpExecutionAP                        = "RequireAdministrationCxpExecutionAP";
        public const string RequireReceptionElectronicInvoice                          = "RequireReceptionElectronicInvoice";
        public const string RequireReceptionElectronicAnalyticPaymentInvoice           = "RequireReceptionElectronicAnalyticPaymentInvoice";
        public const string RequireReceptionDocumentalInvoice                          = "RequireReceptionDocumentalInvoice";
        public const string RequireReceptionDocumentalInvoiceFromXML                   = "RequireReceptionDocumentalInvoiceFromXML";
        public const string RequireReceptionDocumentalAnalyticPaymentInvoice           = "RequireReceptionDocumentalAnalyticPaymentInvoice";
        public const string RequireReceptionDocumentalAnalyticPaymentInvoiceFromXML    = "RequireReceptionDocumentalAnalyticPaymentInvoiceFromXML";
        public const string RequireReceptionElectronicInvoiceREP                       = "RequireReceptionElectronicInvoiceREP";
        public const string RequireReceptionElectronicMultipleInvoice                  = "RequireReceptionElectronicMultipleInvoice";

        // ── Instrucciones ─────────────────────────────────────────────────────
        public const string RequireReceptionSignPaymentSchedule = "RequireReceptionSignPaymentSchedule";
        public const string RequirePaymentList                  = "RequirePaymentList";
        public const string RequireCancelPaymentSchedule        = "RequireCancelPaymentSchedule";
        public const string RequireCancelPaymentList            = "RequireCancelPaymentList";

        // ── OS / Recepción ────────────────────────────────────────────────────
        public const string RequireReceptionSignSupplyOrders  = "RequireReceptionSignSupplyOrders";
        public const string RequireReceptionSignSOEstimations = "RequireReceptionSignSOEstimations";
        public const string RequireReceptionSignReception     = "RequireReceptionSignReception";
    }
}
