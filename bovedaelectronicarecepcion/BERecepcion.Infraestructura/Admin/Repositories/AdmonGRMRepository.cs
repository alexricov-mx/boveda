using BERecepcion.Core.Admin.Dto;
using BERecepcion.Core.Admin.Interfaces.Repositories;
using BERecepcion.Core.Dto;
using BERecepcion.Infraestructura.Repositories;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BERecepcion.Infraestructura.Admin.Repositories
{
    public class AdmonGRMRepository : BaseSQLServerSqlRepository, IAdmonGRMRepository
    {
        public AdmonGRMRepository(string cnnString) : base(cnnString)
        {
        }

        public async Task<DataResult<IEnumerable<AdmonGRMDto>>> GetFirmasGRMAsync(string AdministratorToken, int PageNum, int PageSize)
        {
            DataResult<IEnumerable<AdmonGRMDto>> resultItemIenum = new DataResult<IEnumerable<AdmonGRMDto>>()
            {
                Status = System.Net.HttpStatusCode.OK,
                Message = "Todo bien"
            };

            try
            {
                // Parametros
                DynamicParameters par = new DynamicParameters();
                par.Add("@AdministratorToken", AdministratorToken);
                par.Add("@pagenum", PageNum);
                par.Add("@pagesize", PageSize);

                //using para levantar la conexion al BD
                using (IDbConnection db = GetConnection())
                {
                    var result = await db.QueryMultipleAsync(sql: "SP_admongrm_seleccion ", param: par, commandType: CommandType.StoredProcedure);
                    var paging = await result.ReadAsync<Pager>();
                    var admonGRM = await result.ReadAsync<AdmonGRMDto>();

                    resultItemIenum.Data = admonGRM;
                    resultItemIenum.Pager = paging.FirstOrDefault();
                    return resultItemIenum;
                }
            }
            catch (Exception ext)
            {
                resultItemIenum.Message = ext.Message;
                resultItemIenum.Status = System.Net.HttpStatusCode.BadRequest;

                return resultItemIenum;
            }
            // llamado al storedProcedured -> cachas la respuesta del SP
            //throw new NotImplementedException();
        }

        public async Task<DataResult<string>> InsertaFirmaGRMAsync(AdmonGRMDto admonGRMDto)
        {
            DataResult<string> resultItem = new DataResult<string>()
            {
                Status = System.Net.HttpStatusCode.OK,
                Message = "Todo bien"
            };
            try
            {
                // Parametros
                DynamicParameters par = new DynamicParameters();
                par.Add("@Contract", admonGRMDto.Contract);
                if (admonGRMDto.SapOrder != null)
                    par.Add("@SapOrder", admonGRMDto.SapOrder);
                if (admonGRMDto.Reception != null)
                    par.Add("@Reception", admonGRMDto.Reception);
                par.Add("@CreditorNumber", admonGRMDto.CreditorNumber);
                par.Add("@DocumentType", admonGRMDto.DocumentType);
                par.Add("@AdministratorToken", admonGRMDto.AdministratorToken);

                //using para levantar la conexion al BD
                using (IDbConnection db = GetConnection())
                {
                    var result = await db.QueryFirstOrDefaultAsync<string>(sql: "SP_admongrm_inserta", param: par, commandType: CommandType.StoredProcedure);
                    resultItem.Message = "Datos Insertados correctamente";
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
            // llamado al storedProcedured -> cachas la respuesta del SP
            //throw new NotImplementedException();
        }
    }
}
