using BERecepcion.Api.Infrastructure.Auth;
using Microsoft.Extensions.DependencyInjection;

namespace BERecepcion.Api.StartupExtensions;

public static class AuthorizationExtensions
{
    public static IServiceCollection AddAuthorizationPoliciesConfig(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            // Política base: solo requiere JWT válido (sin rol específico de BD)
            options.AddPolicy(PolicyConstants.RequireAuthenticatedUser, policy =>
                policy.RequireAuthenticatedUser());

            // ── Administración ──────────────────────────────────────────────────
            options.AddPolicy(PolicyConstants.RequireAdministrationUsers, policy =>
                policy.RequireAuthenticatedUser()
                      .RequireClaim(ApiAuthConstants.RoleBdClaim, RoleConstants.AdministrationUsers));

            options.AddPolicy(PolicyConstants.RequireAdministrationProfiles, policy =>
                policy.RequireAuthenticatedUser()
                      .RequireClaim(ApiAuthConstants.RoleBdClaim, RoleConstants.AdministrationProfiles));

            options.AddPolicy(PolicyConstants.RequireAdministrationManagementCenters, policy =>
                policy.RequireAuthenticatedUser()
                      .RequireClaim(ApiAuthConstants.RoleBdClaim, RoleConstants.AdministrationManagementCenters));

            options.AddPolicy(PolicyConstants.RequireAdministrationInterfaces, policy =>
                policy.RequireAuthenticatedUser()
                      .RequireClaim(ApiAuthConstants.RoleBdClaim, RoleConstants.AdministrationInterfaces));

            options.AddPolicy(PolicyConstants.RequireAdministrationLog, policy =>
                policy.RequireAuthenticatedUser()
                      .RequireClaim(ApiAuthConstants.RoleBdClaim, RoleConstants.AdministrationLog));

            options.AddPolicy(PolicyConstants.RequireAdministrationLogUsers, policy =>
                policy.RequireAuthenticatedUser()
                      .RequireClaim(ApiAuthConstants.RoleBdClaim, RoleConstants.AdministrationLogUsers));

            options.AddPolicy(PolicyConstants.RequireAdministrationAdefa, policy =>
                policy.RequireAuthenticatedUser()
                      .RequireClaim(ApiAuthConstants.RoleBdClaim, RoleConstants.AdministrationAdefa));

            options.AddPolicy(PolicyConstants.RequireAdministrationValidations, policy =>
                policy.RequireAuthenticatedUser()
                      .RequireClaim(ApiAuthConstants.RoleBdClaim, RoleConstants.AdministrationValidations));

            options.AddPolicy(PolicyConstants.RequireContractsRegister, policy =>
                policy.RequireAuthenticatedUser()
                      .RequireClaim(ApiAuthConstants.RoleBdClaim, RoleConstants.ContractsRegister));

            options.AddPolicy(PolicyConstants.RequireDeviationSigns, policy =>
                policy.RequireAuthenticatedUser()
                      .RequireClaim(ApiAuthConstants.RoleBdClaim, RoleConstants.DeviationSigns));

            // ── Consulta ─────────────────────────────────────────────────────────
            options.AddPolicy(PolicyConstants.RequireReportEmails, policy =>
                policy.RequireAuthenticatedUser()
                      .RequireClaim(ApiAuthConstants.RoleBdClaim, RoleConstants.ReportEmails));

            options.AddPolicy(PolicyConstants.RequireReportDesviationSigns, policy =>
                policy.RequireAuthenticatedUser()
                      .RequireClaim(ApiAuthConstants.RoleBdClaim, RoleConstants.ReportDesviationSigns));

            options.AddPolicy(PolicyConstants.RequireReportRejectedInvoice, policy =>
                policy.RequireAuthenticatedUser()
                      .RequireClaim(ApiAuthConstants.RoleBdClaim, RoleConstants.ReportRejectedInvoice));

            options.AddPolicy(PolicyConstants.RequireReportRejectedInvoiceAnalytic, policy =>
                policy.RequireAuthenticatedUser()
                      .RequireClaim(ApiAuthConstants.RoleBdClaim, RoleConstants.ReportRejectedInvoiceAnalytic));

            options.AddPolicy(PolicyConstants.RequireQueryPrefecture, policy =>
                policy.RequireAuthenticatedUser()
                      .RequireClaim(ApiAuthConstants.RoleBdClaim, RoleConstants.QueryPrefecture));

            options.AddPolicy(PolicyConstants.RequireQueryPrefectureAnalytic, policy =>
                policy.RequireAuthenticatedUser()
                      .RequireClaim(ApiAuthConstants.RoleBdClaim, RoleConstants.QueryPrefectureAnalytic));

            options.AddPolicy(PolicyConstants.RequireQueryTracing, policy =>
                policy.RequireAuthenticatedUser()
                      .RequireClaim(ApiAuthConstants.RoleBdClaim, RoleConstants.QueryTracing));

            options.AddPolicy(PolicyConstants.RequireStatisticsSupplyOrders, policy =>
                policy.RequireAuthenticatedUser()
                      .RequireClaim(ApiAuthConstants.RoleBdClaim, RoleConstants.StatisticsSupplyOrders));

            options.AddPolicy(PolicyConstants.RequireStatisticsCopade, policy =>
                policy.RequireAuthenticatedUser()
                      .RequireClaim(ApiAuthConstants.RoleBdClaim, RoleConstants.StatisticsCopade));

            options.AddPolicy(PolicyConstants.RequireStatisticsCopadeBanking, policy =>
                policy.RequireAuthenticatedUser()
                      .RequireClaim(ApiAuthConstants.RoleBdClaim, RoleConstants.StatisticsCopadeBanking));

            options.AddPolicy(PolicyConstants.RequireStatisticsReceptionInvoice, policy =>
                policy.RequireAuthenticatedUser()
                      .RequireClaim(ApiAuthConstants.RoleBdClaim, RoleConstants.StatisticsReceptionInvoice));

            options.AddPolicy(PolicyConstants.RequireStatisticsPaymentSchedule, policy =>
                policy.RequireAuthenticatedUser()
                      .RequireClaim(ApiAuthConstants.RoleBdClaim, RoleConstants.StatisticsPaymentSchedule));

            options.AddPolicy(PolicyConstants.RequireStatisticsPaymentList, policy =>
                policy.RequireAuthenticatedUser()
                      .RequireClaim(ApiAuthConstants.RoleBdClaim, RoleConstants.StatisticsPaymentList));

            options.AddPolicy(PolicyConstants.RequireStatisticsAnalyticalPayment, policy =>
                policy.RequireAuthenticatedUser()
                      .RequireClaim(ApiAuthConstants.RoleBdClaim, RoleConstants.StatisticsAnalyticalPayment));

            options.AddPolicy(PolicyConstants.RequireStatisticsSOEstimations, policy =>
                policy.RequireAuthenticatedUser()
                      .RequireClaim(ApiAuthConstants.RoleBdClaim, RoleConstants.StatisticsSOEstimations));

            options.AddPolicy(PolicyConstants.RequireStatisticsBankEstimate, policy =>
                policy.RequireAuthenticatedUser()
                      .RequireClaim(ApiAuthConstants.RoleBdClaim, RoleConstants.StatisticsBankEstimate));

            options.AddPolicy(PolicyConstants.RequireStatisticsBankOrder, policy =>
                policy.RequireAuthenticatedUser()
                      .RequireClaim(ApiAuthConstants.RoleBdClaim, RoleConstants.StatisticsBankOrder));

            // ── COPADE / Analítico ───────────────────────────────────────────────
            options.AddPolicy(PolicyConstants.RequireCancelCopade, policy =>
                policy.RequireAuthenticatedUser()
                      .RequireClaim(ApiAuthConstants.RoleBdClaim, RoleConstants.CancelCopade));

            options.AddPolicy(PolicyConstants.RequireAnalyticalPayment, policy =>
                policy.RequireAuthenticatedUser()
                      .RequireClaim(ApiAuthConstants.RoleBdClaim, RoleConstants.AnalyticalPayment));

            // ── Estadísticas ─────────────────────────────────────────────────────
            options.AddPolicy(PolicyConstants.RequireStatisticsUsuariosEPS, policy =>
                policy.RequireAuthenticatedUser()
                      .RequireClaim(ApiAuthConstants.RoleBdClaim, RoleConstants.StatisticsUsuariosEPS));

            options.AddPolicy(PolicyConstants.RequireStatisticsFacturasrecibidas, policy =>
                policy.RequireAuthenticatedUser()
                      .RequireClaim(ApiAuthConstants.RoleBdClaim, RoleConstants.StatisticsFacturasrecibidas));

            options.AddPolicy(PolicyConstants.RequireStatisticsCopadesingresados, policy =>
                policy.RequireAuthenticatedUser()
                      .RequireClaim(ApiAuthConstants.RoleBdClaim, RoleConstants.StatisticsCopadesingresados));

            options.AddPolicy(PolicyConstants.RequireStatisticsCopadespendientes, policy =>
                policy.RequireAuthenticatedUser()
                      .RequireClaim(ApiAuthConstants.RoleBdClaim, RoleConstants.StatisticsCopadespendientes));

            options.AddPolicy(PolicyConstants.RequireStatisticsOrdenesrecibidas, policy =>
                policy.RequireAuthenticatedUser()
                      .RequireClaim(ApiAuthConstants.RoleBdClaim, RoleConstants.StatisticsOrdenesrecibidas));

            options.AddPolicy(PolicyConstants.RequireStatisticsCopadescancelados, policy =>
                policy.RequireAuthenticatedUser()
                      .RequireClaim(ApiAuthConstants.RoleBdClaim, RoleConstants.StatisticsCopadescancelados));

            options.AddPolicy(PolicyConstants.RequireStatisticsEstimacionesrecibidas, policy =>
                policy.RequireAuthenticatedUser()
                      .RequireClaim(ApiAuthConstants.RoleBdClaim, RoleConstants.StatisticsEstimacionesrecibidas));

            // ── Facturas ─────────────────────────────────────────────────────────
            options.AddPolicy(PolicyConstants.RequireAdministrationCxpExecution, policy =>
                policy.RequireAuthenticatedUser()
                      .RequireClaim(ApiAuthConstants.RoleBdClaim, RoleConstants.AdministrationCxpExecution));

            options.AddPolicy(PolicyConstants.RequireAdministrationCxpExecutionAP, policy =>
                policy.RequireAuthenticatedUser()
                      .RequireClaim(ApiAuthConstants.RoleBdClaim, RoleConstants.AdministrationCxpExecutionAP));

            options.AddPolicy(PolicyConstants.RequireReceptionElectronicAnalyticPaymentInvoice, policy =>
                policy.RequireAuthenticatedUser()
                      .RequireClaim(ApiAuthConstants.RoleBdClaim, RoleConstants.ReceptionElectronicAnalyticPaymentInvoice));

            options.AddPolicy(PolicyConstants.RequireReceptionDocumentalInvoiceFromXML, policy =>
                policy.RequireAuthenticatedUser()
                      .RequireClaim(ApiAuthConstants.RoleBdClaim, RoleConstants.ReceptionDocumentalInvoiceFromXML));

            options.AddPolicy(PolicyConstants.RequireReceptionDocumentalAnalyticPaymentInvoice, policy =>
                policy.RequireAuthenticatedUser()
                      .RequireClaim(ApiAuthConstants.RoleBdClaim, RoleConstants.ReceptionDocumentalAnalyticPaymentInvoice));

            options.AddPolicy(PolicyConstants.RequireReceptionDocumentalAnalyticPaymentInvoiceFromXML, policy =>
                policy.RequireAuthenticatedUser()
                      .RequireClaim(ApiAuthConstants.RoleBdClaim, RoleConstants.ReceptionDocumentalAnalyticPaymentInvoiceFromXML));

            options.AddPolicy(PolicyConstants.RequireReceptionElectronicInvoiceREP, policy =>
                policy.RequireAuthenticatedUser()
                      .RequireClaim(ApiAuthConstants.RoleBdClaim, RoleConstants.ReceptionElectronicInvoiceREP));

            options.AddPolicy(PolicyConstants.RequireReceptionElectronicMultipleInvoice, policy =>
                policy.RequireAuthenticatedUser()
                      .RequireClaim(ApiAuthConstants.RoleBdClaim, RoleConstants.ReceptionElectronicMultipleInvoice));

            // ── Instrucciones ────────────────────────────────────────────────────
            options.AddPolicy(PolicyConstants.RequireReceptionSignPaymentSchedule, policy =>
                policy.RequireAuthenticatedUser()
                      .RequireClaim(ApiAuthConstants.RoleBdClaim, RoleConstants.ReceptionSignPaymentSchedule));

            options.AddPolicy(PolicyConstants.RequirePaymentList, policy =>
                policy.RequireAuthenticatedUser()
                      .RequireClaim(ApiAuthConstants.RoleBdClaim, RoleConstants.PaymentList));

            options.AddPolicy(PolicyConstants.RequireCancelPaymentSchedule, policy =>
                policy.RequireAuthenticatedUser()
                      .RequireClaim(ApiAuthConstants.RoleBdClaim, RoleConstants.CancelPaymentSchedule));

            options.AddPolicy(PolicyConstants.RequireCancelPaymentList, policy =>
                policy.RequireAuthenticatedUser()
                      .RequireClaim(ApiAuthConstants.RoleBdClaim, RoleConstants.CancelPaymentList));

            // ── OS / Recepción ───────────────────────────────────────────────────
            options.AddPolicy(PolicyConstants.RequireReceptionSignSOEstimations, policy =>
                policy.RequireAuthenticatedUser()
                      .RequireClaim(ApiAuthConstants.RoleBdClaim, RoleConstants.ReceptionSignSOEstimations));

            // ── Recepción ────────────────────────────────────────────────────────
            options.AddPolicy(PolicyConstants.RequireReceptionElectronicInvoice, policy =>
                policy.RequireAuthenticatedUser()
                      .RequireClaim(ApiAuthConstants.RoleBdClaim, RoleConstants.ReceptionElectronicInvoice));

            options.AddPolicy(PolicyConstants.RequireReceptionDocumentalInvoice, policy =>
                policy.RequireAuthenticatedUser()
                      .RequireClaim(ApiAuthConstants.RoleBdClaim, RoleConstants.ReceptionDocumentalInvoice));

            options.AddPolicy(PolicyConstants.RequireReceptionSignReception, policy =>
                policy.RequireAuthenticatedUser()
                      .RequireClaim(ApiAuthConstants.RoleBdClaim, RoleConstants.ReceptionSignReception));

            options.AddPolicy(PolicyConstants.RequireReceptionSignCopade, policy =>
                policy.RequireAuthenticatedUser()
                      .RequireClaim(ApiAuthConstants.RoleBdClaim, RoleConstants.ReceptionSignCopade));

            options.AddPolicy(PolicyConstants.RequireReceptionSignSupplyOrders, policy =>
                policy.RequireAuthenticatedUser()
                      .RequireClaim(ApiAuthConstants.RoleBdClaim, RoleConstants.ReceptionSignSupplyOrders));
        });

        return services;
    }
}
