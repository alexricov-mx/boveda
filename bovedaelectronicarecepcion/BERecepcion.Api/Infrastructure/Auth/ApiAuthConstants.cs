namespace BERecepcion.Api.Infrastructure.Auth
{
    /// <summary>
    /// Constantes para las magic strings del mecanismo de autenticación dual (JWT Bearer + ApiKey).
    /// Centraliza todos los nombres de headers y tipos de claims para evitar errores por typos.
    /// </summary>
    public static class ApiAuthConstants
    {
        /// <summary>Nombre del header HTTP para el ApiKey fallback.</summary>
        public const string ApiKeyHeaderName = "ApiKey";

        /// <summary>Clave de configuración donde se almacena el ApiKey.</summary>
        public const string ApiKeyConfigKey = "Seguridad:ApiKey";

        /// <summary>Prefijo del esquema Bearer en el header Authorization.</summary>
        public const string BearerPrefix = "Bearer ";

        /// <summary>Nombre del header de autorización HTTP estándar.</summary>
        public const string AuthorizationHeader = "Authorization";

        /// <summary>Claim de Azure AD con el nombre de usuario preferido (UPN / email corporativo).</summary>
        public const string PreferredUsernameClaim = "preferred_username";

        /// <summary>Claim estándar de email.</summary>
        public const string EmailClaim = "email";
        // ─── AGREGAR ESTAS DOS ────────────────────────────────────────────────

        /// <summary>Claim de Azure AD v1.0 / tokens de tenant externo.</summary>
        public const string UniqueNameClaim = "unique_name";

        /// <summary>
        /// Claim al que ASP.NET Core mapea automáticamente el claim 'email' del JWT.
        /// El JWT handler transforma los nombres de claim por defecto.
        /// </summary>
        public const string EmailSchemaClaim =
            "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress";
        /// <summary>
        /// Claim de rol de BD agregado por <see cref="UserClaimsTransformation"/> al principal.
        /// Se usa en las políticas de autorización registradas en Program.cs.
        /// </summary>
        public const string RoleBdClaim = "role_bd";

        /// <summary>Claim de Azure AD que contiene el Object ID único del usuario en Azure AD (UserId global).</summary>
        public const string OidClaim = "oid";

        /// <summary>Claim de Azure AD que contiene el nombre completo del usuario.</summary>
        public const string NameClaim = "name";
    }
}
