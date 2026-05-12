using System;

namespace BERecepcion.Core.Exceptions
{
    /// <summary>
    /// Excepción que se lanza cuando hay un conflicto con el estado actual del recurso.
    /// Resultará en una respuesta HTTP 409 (Conflict).
    /// Ejemplos: recurso duplicado, operación no permitida en el estado actual.
    /// </summary>
    public class ConflictException : Exception
    {
        public string ResourceName { get; }
        public object ConflictValue { get; }

        public ConflictException()
            : base("La operación genera un conflicto con el estado actual del recurso.")
        {
        }

        public ConflictException(string message)
            : base(message)
        {
        }

        public ConflictException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public ConflictException(string resourceName, object conflictValue)
            : base($"El recurso '{resourceName}' con valor '{conflictValue}' ya existe.")
        {
            ResourceName = resourceName;
            ConflictValue = conflictValue;
        }
    }
}
