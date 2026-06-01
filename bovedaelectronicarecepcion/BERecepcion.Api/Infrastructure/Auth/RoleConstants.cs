namespace BERecepcion.Api.Infrastructure.Auth
{
    /// <summary>
    /// Constantes para todos los valores del campo <c>Rol</c> del catálogo <c>RolesCatalogo</c>.
    /// Los valores deben coincidir exactamente con los registrados en la BD (SP_rolescatalogo_selecciona).
    /// </summary>
    public static class RoleConstants
    {
        // ── Administración ────────────────────────────────────────────
        public const string AdministrationUsers      = "AdministrationUsers";
        public const string AdministrationProfiles   = "AdministrationProfiles";
        public const string AdministrationInterfaces = "AdministrationInterfaces";

        // ── Recepción ─────────────────────────────────────────────────
        public const string ReceptionElectronicInvoice = "ReceptionElectronicInvoice";
        public const string ReceptionDocumentalInvoice = "ReceptionDocumentalInvoice";
        public const string ReceptionSignReception     = "ReceptionSignReception";
        public const string ReceptionSignCopade        = "ReceptionSignCopade";
        public const string ReceptionSignSupplyOrders  = "ReceptionSignSupplyOrders";

        // ── Desvíos ───────────────────────────────────────────────────
        public const string DeviationSigns = "DeviationSigns";

        // ── Contratos ─────────────────────────────────────────────────
        public const string ContractsRegister = "ContractsRegister";
    }
}
