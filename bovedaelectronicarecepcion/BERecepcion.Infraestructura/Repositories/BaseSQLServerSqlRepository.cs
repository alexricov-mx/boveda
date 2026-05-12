using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Data.SqlClient;

namespace BERecepcion.Infraestructura.Repositories
{
    public class BaseSQLServerSqlRepository
    {
        private readonly string _cnnString;

        public BaseSQLServerSqlRepository(string cnnString)
        {
            _cnnString = cnnString;
        }

        public IDbConnection GetConnection()
        {
            return new SqlConnection(_cnnString);
        }
    }
}
