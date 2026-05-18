using System.Data;

namespace BERecepcion.Core.Interfaces;

/// <summary>
/// Abstracción para la creación de conexiones a base de datos.
/// Permite desacoplar los repositorios del motor de BD concreto y facilita el testing.
/// </summary>
public interface IDbConnectionFactory
{
    /// <summary>
    /// Crea y retorna una nueva instancia de conexión sin abrirla.
    /// Cada repositorio es responsable de abrir y cerrar la conexión dentro de un using.
    /// </summary>
    IDbConnection CreateConnection();
}
