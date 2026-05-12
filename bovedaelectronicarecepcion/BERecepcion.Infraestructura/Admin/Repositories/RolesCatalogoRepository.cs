using BERecepcion.Core.Admin.Dto;
using BERecepcion.Core.Admin.Interfaces.Repositories;
using BERecepcion.Core.Dto;
using BERecepcion.Core.Interfaces.Repositories;
using BERecepcion.Infraestructura.Repositories;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace BERecepcion.Infraestructura.Admin.Repositories
{
    public class RolesCatalogoRepository : BaseSQLServerSqlRepository, IRolesCatalogoRepository
    {
        public RolesCatalogoRepository(string cnnString) : base(cnnString)
        {

        }
        public async Task<DataResult<IEnumerable<RolesCatalogoDto>>> GetRolesAsync()
        {
            DataResult<IEnumerable<RolesCatalogoDto>> resultItem = new DataResult<IEnumerable<RolesCatalogoDto>>()
            {
                Status = System.Net.HttpStatusCode.OK
            };
            try
            {
                using (IDbConnection db = GetConnection())
                {
                    var result = await db.QueryAsync<RolesCatalogoDto>(sql: "SP_rolescatalogo_selecciona ", commandType: CommandType.StoredProcedure);
                    resultItem.Data = result;
                }
                return resultItem;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
