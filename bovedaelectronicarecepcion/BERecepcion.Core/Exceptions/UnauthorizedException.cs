using System;

namespace BERecepcion.Core.Exceptions
{
    /// <summary>
    /// Excepción que se lanza cuando el usuario no tiene permisos para realizar la operación.
    /// Resultará en una respuesta HTTP 403 (Forbidden).
    /// </summary>
    public class UnauthorizedException : Exception
    {
        public string Resource { get; }
        public string Action { get; }

        public UnauthorizedException()
            : base("No tiene permisos para realizar esta operación.")
        {
        }

        public UnauthorizedException(string message)
            : base(message)
        {
        }

        public UnauthorizedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public UnauthorizedException(string resource, string action)
            : base($"No tiene permisos para '{action}' en '{resource}'.")
        {
            Resource = resource;
            Action = action;
        }
    }
}
