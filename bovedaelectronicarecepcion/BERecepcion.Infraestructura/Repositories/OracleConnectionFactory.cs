using BERecepcion.Core.Interfaces;
using BERecepcion.Core.Options;
using Microsoft.Extensions.Options;
using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace BERecepcion.Infraestructura.Repositories;

/// <summary>
/// Implementación de IDbConnectionFactory para Oracle.
/// Preparada para cuando se migren repositorios Oracle al nuevo esquema de DI.
/// Nota: extender ConnectionStringsOptions con la key Oracle cuando sea necesario.
/// </summary>
public class OracleConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public OracleConnectionFactory(IOptions<ConnectionStringsOptions> options)
    {
        _connectionString = options.Value.SQLServerSQLDEV002;
    }

    /// <inheritdoc/>
    public IDbConnection CreateConnection() => new OracleConnection(_connectionString);
}
