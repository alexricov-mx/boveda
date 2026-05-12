using System;
using System.Collections.Generic;
using System.Linq;

namespace BERecepcion.Core.Exceptions
{
    /// <summary>
    /// Excepción que se lanza cuando los datos de entrada no pasan la validación.
    /// Resultará en una respuesta HTTP 400 (Bad Request).
    /// </summary>
    public class ValidationException : Exception
    {
        public List<ValidationError> Errors { get; }

        public ValidationException()
            : base("Uno o más errores de validación ocurrieron.")
        {
            Errors = new List<ValidationError>();
        }

        public ValidationException(string message)
            : base(message)
        {
            Errors = new List<ValidationError>();
        }

        public ValidationException(string message, List<ValidationError> errors)
            : base(message)
        {
            Errors = errors ?? new List<ValidationError>();
        }

        public ValidationException(List<ValidationError> errors)
            : base("Uno o más errores de validación ocurrieron.")
        {
            Errors = errors ?? new List<ValidationError>();
        }

        public ValidationException(string field, string error)
            : base($"Error de validación en el campo '{field}': {error}")
        {
            Errors = new List<ValidationError>
            {
                new ValidationError { Field = field, Message = error }
            };
        }
    }

    public class ValidationError
    {
        public string Field { get; set; }
        public string Message { get; set; }
        public string Code { get; set; }
    }
}
