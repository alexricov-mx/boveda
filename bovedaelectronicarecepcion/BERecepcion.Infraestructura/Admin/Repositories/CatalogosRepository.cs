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
    public class CatalogosRepository : BaseSQLServerSqlRepository, ICatalogosRepository
    {

        public CatalogosRepository(string cnnString) : base(cnnString)
        {

        }

        DataResult<IEnumerable<OrganismDto>> resultItemIenum = new DataResult<IEnumerable<OrganismDto>>()
        {
            Status = System.Net.HttpStatusCode.OK,
            Message = "Todo bien"
        };


        DataResult<IEnumerable<ProfilesDto>> resultItem = new DataResult<IEnumerable<ProfilesDto>>()
        {
            Status = System.Net.HttpStatusCode.OK,
            Message = "Todo bien"
        };

        public async Task<DataResult<IEnumerable<OrganismDto>>> GetOrganismosAsync()
        {
            try
            {
                //using para levantar la conexion al BD
                using (IDbConnection db = GetConnection())
                {
                    var result = await db.QueryAsync<OrganismDto>(sql: "SP_Organism_selecciona ", commandType: CommandType.StoredProcedure);
                    resultItemIenum.Data = result;

                    return resultItemIenum;
                }
            }
            catch (Exception ext)
            {
                resultItemIenum.Message = ext.Message;
                resultItemIenum.Status = System.Net.HttpStatusCode.BadRequest;

                return resultItemIenum;
            }

        }

        public async Task<DataResult<IEnumerable<ProfilesDto>>> GetProfilesAsync()
        {
            try
            {
                //using para levantar la conexion al BD
                using (IDbConnection db = GetConnection())
                {
                    var result = await db.QueryAsync<ProfilesDto>(sql: "SP_Profiles_selecciona", commandType: CommandType.StoredProcedure);
                    resultItem.Data = result;

                    return resultItem;
                }
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
