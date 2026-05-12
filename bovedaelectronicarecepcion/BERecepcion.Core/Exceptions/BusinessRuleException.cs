using System;

namespace BERecepcion.Core.Exceptions
{
    /// <summary>
    /// Excepción que se lanza cuando una regla de negocio no se cumple.
    /// Resultará en una respuesta HTTP 422 (Unprocessable Entity).
    /// </summary>
    public class BusinessRuleException : Exception
    {
        public string RuleName { get; }
        public object RuleValue { get; }

        public BusinessRuleException()
            : base("La operación viola una regla de negocio.")
        {
        }

        public BusinessRuleException(string message)
            : base(message)
        {
        }

        public BusinessRuleException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public BusinessRuleException(string ruleName, object ruleValue, string message)
            : base(message)
        {
            RuleName = ruleName;
            RuleValue = ruleValue;
        }
    }
}
