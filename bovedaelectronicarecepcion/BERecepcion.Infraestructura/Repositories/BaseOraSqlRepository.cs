using BERecepcion.Core.Interfaces;
using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace BERecepcion.Infraestructura.Repositories;

/// <summary>
/// Clase base para repositorios Oracle con Dapper.
/// Constructor primario: recibe IDbConnectionFactory por DI (patrón nuevo — repositorios migrados).
/// Constructor secundario: recibe string de conexión (compatibilidad hacia atrás — repositorios no migrados aún).
/// </summary>
public abstract class BaseOraSqlRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    /// <summary>Constructor DI — usar para repositorios migrados al nuevo esquema.</summary>
    protected BaseOraSqlRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    /// <summary>
    /// Constructor de compatibilidad hacia atrás — permite que repositorios aún no migrados
    /// sigan compilando mientras se hace la migración incremental.
    /// </summary>
    protected BaseOraSqlRepository(string cnnString)
    {
        _connectionFactory = new InlineOracleConnectionFactory(cnnString);
    }

    /// <summary>
    /// Crea una nueva conexión Oracle sin abrirla.
    /// Usar siempre dentro de un bloque using: using (var db = GetConnection()) { ... }
    /// </summary>
    protected IDbConnection GetConnection() => _connectionFactory.CreateConnection();

    /// <summary>
    /// Factory interna para mantener compatibilidad con repositorios que aún reciben string.
    /// Se eliminará cuando todos los repositorios estén migrados a IDbConnectionFactory.
    /// </summary>
    private sealed class InlineOracleConnectionFactory : IDbConnectionFactory
    {
        private readonly string _cnnString;
        public InlineOracleConnectionFactory(string cnnString) => _cnnString = cnnString;
        public IDbConnection CreateConnection() => new OracleConnection(_cnnString);
    }
}
