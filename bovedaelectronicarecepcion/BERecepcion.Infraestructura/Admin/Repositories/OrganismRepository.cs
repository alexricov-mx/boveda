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
    public class OrganismRepository : BaseSQLServerSqlRepository, IOrganismRepository
    {

        DataResult<OrganismDto> resultItem = new DataResult<OrganismDto>()
        {
            Status = System.Net.HttpStatusCode.OK,
            Message = "Todo bien"
        };

        public OrganismRepository(string cnnString) : base(cnnString)
        {
        }

        public async Task<DataResult<IEnumerable<OrganismDto>>> GetOrganismAsync()
        {
            DataResult<IEnumerable<OrganismDto>> resultItemIenum = new DataResult<IEnumerable<OrganismDto>>()
            {
                Status = System.Net.HttpStatusCode.OK,
                Message = "Todo bien"
            };

            try
            {

                using (IDbConnection db = GetConnection())
                {
                    var result = await db.QueryAsync<OrganismDto>(sql: "SP_AdminOrganism_selecciona ", commandType: CommandType.StoredProcedure);
                    resultItemIenum.Data = result;

                    return resultItemIenum;
                }

            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<DataResult<OrganismDto>> ActualizaOrganismAsync(OrganismDto organismDto)
        {

            try
            {
                // Parametros
                DynamicParameters par = new DynamicParameters();
                par.Add("@OrganismID", organismDto.OrganismID);
                par.Add("@name", organismDto.Name);
                par.Add("@clave", organismDto.Clave);
                par.Add("@Address", organismDto.Address);
                par.Add("@rfc", organismDto.Rfc);


                //using para levantar la conexion al BD
                using (IDbConnection db = GetConnection())
                {
                    var result = await db.QueryFirstAsync<OrganismDto>(sql: "SP_AdminOrganism_Actualiza", param: par, commandType: CommandType.StoredProcedure);

                    resultItem.Message = "Datos Actualizados Correctamente";
                    resultItem.Data = result;

                    return resultItem;
                }
                // llamado al storedProcedured -> cachas la respuesta del SP
                //throw new NotImplementedException();
            }
            catch (Exception ext)
            {
                resultItem.Message = ext.Message;
                resultItem.Status = System.Net.HttpStatusCode.BadRequest;

                return resultItem;
            }
        }


    }
}
