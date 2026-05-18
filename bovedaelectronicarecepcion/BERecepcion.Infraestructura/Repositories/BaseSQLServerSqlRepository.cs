using BERecepcion.Core.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;

namespace BERecepcion.Infraestructura.Repositories;

/// <summary>
/// Clase base para repositorios SQL Server con Dapper.
/// Constructor primario: recibe IDbConnectionFactory por DI (patrón nuevo — repositorios migrados).
/// Constructor secundario: recibe string de conexión (compatibilidad hacia atrás — repositorios no migrados aún).
/// </summary>
public abstract class BaseSQLServerSqlRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    /// <summary>Constructor DI — usar para repositorios migrados al nuevo esquema.</summary>
    protected BaseSQLServerSqlRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    /// <summary>
    /// Constructor de compatibilidad hacia atrás — permite que repositorios aún no migrados
    /// sigan compilando mientras se hace la migración incremental.
    /// </summary>
    protected BaseSQLServerSqlRepository(string cnnString)
    {
        _connectionFactory = new InlineStringConnectionFactory(cnnString);
    }

    /// <summary>
    /// Crea una nueva conexión SQL Server sin abrirla.
    /// Usar siempre dentro de un bloque using: using (var db = GetConnection()) { ... }
    /// </summary>
    protected IDbConnection GetConnection() => _connectionFactory.CreateConnection();

    /// <summary>
    /// Factory interna para mantener compatibilidad con repositorios que aún reciben string.
    /// Se eliminará cuando todos los repositorios estén migrados a IDbConnectionFactory.
    /// </summary>
    private sealed class InlineStringConnectionFactory : IDbConnectionFactory
    {
        private readonly string _cnnString;
        public InlineStringConnectionFactory(string cnnString) => _cnnString = cnnString;
        public IDbConnection CreateConnection() => new SqlConnection(_cnnString);
    }
}
