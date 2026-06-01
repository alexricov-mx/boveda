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

        // ── Administración ────────────────────────────────────────────
        public const string RequireAdministrationUsers      = "RequireAdministrationUsers";
        public const string RequireAdministrationProfiles   = "RequireAdministrationProfiles";
        public const string RequireAdministrationInterfaces = "RequireAdministrationInterfaces";

        // ── Recepción ─────────────────────────────────────────────────
        public const string RequireReceptionElectronicInvoice = "RequireReceptionElectronicInvoice";
        public const string RequireReceptionDocumentalInvoice = "RequireReceptionDocumentalInvoice";
        public const string RequireReceptionSignReception     = "RequireReceptionSignReception";
        public const string RequireReceptionSignCopade        = "RequireReceptionSignCopade";
        public const string RequireReceptionSignSupplyOrders  = "RequireReceptionSignSupplyOrders";

        // ── Desvíos ───────────────────────────────────────────────────
        public const string RequireDeviationSigns = "RequireDeviationSigns";

        // ── Contratos ─────────────────────────────────────────────────
        public const string RequireContractsRegister = "RequireContractsRegister";
    }
}
