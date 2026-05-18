using BERecepcion.Core.Interfaces;
using BERecepcion.Core.Options;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using System.Data;

namespace BERecepcion.Infraestructura.Repositories;

/// <summary>
/// Implementación de IDbConnectionFactory para SQL Server usando Microsoft.Data.SqlClient.
/// Lee la cadena de conexión desde IOptions&lt;ConnectionStringsOptions&gt;
/// en lugar de recibirla como string crudo en el constructor.
/// </summary>
public class SqlServerConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public SqlServerConnectionFactory(IOptions<ConnectionStringsOptions> options)
    {
        _connectionString = options.Value.SQLServerSQLDEV002;
    }

    /// <inheritdoc/>
    public IDbConnection CreateConnection() => new SqlConnection(_connectionString);
}
