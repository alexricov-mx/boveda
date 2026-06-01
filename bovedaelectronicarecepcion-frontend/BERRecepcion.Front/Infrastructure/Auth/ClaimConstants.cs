namespace BERRecepcion.Front.Infrastructure.Auth;

/// <summary>
/// Constantes centralizadas para tipos de claims customizados.
/// Elimina magic strings y reduce riesgo de typos en validación de seguridad.
/// </summary>
public static class ClaimConstants
{
    // ========================================
    // AUTHENTICATION TYPES
    // ========================================

    /// <summary>
    /// Tipo de autenticación para los claims de validación de negocio.
    /// </summary>
    public const string ValidationClaimsAuthType = "ValidationClaims";

    /// <summary>
    /// Tipo de autenticación para claims de error de login.
    /// </summary>
    public const string LoginErrorAuthType = "LoginError";

    // ========================================
    // CLAIM TYPES - Datos del Usuario
    // ========================================

    /// <summary>
    /// Claim que contiene el objeto completo UsersDto serializado como JSON.
    /// </summary>
    public const string UserClaim = "User";

    /// <summary>
    /// Claim que contiene los roles del usuario como CSV (e.g. "Admin,Editor,Viewer").
    /// </summary>
    public const string RolesClaim = "Roles";

    // ========================================
    // CLAIM TYPES - Validaciones de Negocio
    // ========================================

    /// <summary>
    /// Indica si el usuario existe en la base de datos (true/false).
    /// </summary>
    public const string UserExistsClaim = "UserExists";

    /// <summary>
    /// Indica si el usuario está bloqueado administrativamente (true/false).
    /// </summary>
    public const string UserIsBlockedClaim = "UserIsBlocked";

    /// <summary>
    /// Indica si el usuario ha sido marcado como eliminado (true/false).
    /// </summary>
    public const string UserIsDeletedClaim = "UserIsDeleted";

    /// <summary>
    /// Indica si la vigencia del usuario (DateInitialValid/DateEndValid) es válida hoy (true/false).
    /// </summary>
    public const string UserdateIsValidClaim = "UserdateIsValid";

    /// <summary>
    /// Indica si la interfaz SAP está habilitada (true/false).
    /// </summary>
    public const string IsSapInterfaceEnabledClaim = "IsSapInterfaceEnabled";

    // ========================================
    // CLAIM TYPES - Errores
    // ========================================

    /// <summary>
    /// Indica que ocurrió un error durante el proceso de login (true/false).
    /// </summary>
    public const string LoginErrorClaim = "LoginError";
}
