using System;

namespace BERecepcion.Core.Exceptions
{
    /// <summary>
    /// Excepción que se lanza cuando un recurso solicitado no se encuentra.
    /// Resultará en una respuesta HTTP 404 (Not Found).
    /// </summary>
    public class NotFoundException : Exception
    {
        public string ResourceName { get; }
        public object ResourceId { get; }

        public NotFoundException()
            : base("El recurso solicitado no fue encontrado.")
        {
        }

        public NotFoundException(string message)
            : base(message)
        {
        }

        public NotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public NotFoundException(string resourceName, object resourceId)
            : base($"El recurso '{resourceName}' con id '{resourceId}' no fue encontrado.")
        {
            ResourceName = resourceName;
            ResourceId = resourceId;
        }
    }
}
