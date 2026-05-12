using System.Data;
using Oracle.ManagedDataAccess.Client;

namespace BERecepcion.Infraestructura.Repositories
{
    public class BaseOraSqlRepository
    {
        private readonly string _cnnString;

        public BaseOraSqlRepository(string cnnString)
        {
            _cnnString = cnnString;
        }

        public IDbConnection GetConnection()
        {
            return new OracleConnection(_cnnString);
        }
    }
}
