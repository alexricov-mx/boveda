using BERecepcion.Core.Dto;
using BERecepcion.Core.Models;
using BERecepcion.Core.OrdenSurtimiento.Dto;
using BERecepcion.Core.OrdenSurtimiento.Interfaces.Repositories;
using BERecepcion.Infraestructura.Repositories;
using Dapper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BERecepcion.Infraestructura.OrdenSurtimiento.Repositories
{
    public class ReceptionRepository : BaseSQLServerSqlRepository, IReceptionAlmacenRepository
    {

        public ReceptionRepository(string cnnString) : base(cnnString)
        {
        }

        DataResult<ReceptionDto> resultItem = new DataResult<ReceptionDto>()
        {
            Status = System.Net.HttpStatusCode.OK,
            Message = "Actualizado con Exito"
        };

        public async Task<DataResult<IEnumerable<ReceptionDto>>> GetReceptionAsync(string Token, int pageSize, int pageNum = 1, string search = null)
        {
            DataResult<IEnumerable<ReceptionDto>> resultItem = new DataResult<IEnumerable<ReceptionDto>>()
            {
                Status = System.Net.HttpStatusCode.OK
            };
            try
            {
                using (IDbConnection db = GetConnection())
                {
                    DynamicParameters par = new DynamicParameters();
                    par.Add("@token", Token);
                    par.Add("@pagenum", pageNum);
                    par.Add("@pagesize", pageSize);
                    par.Add("@search", search);

                    var result = await db.QueryMultipleAsync(sql: "SP_Reception_selecciona", param: par, commandType: CommandType.StoredProcedure);

                    var paging = await result.ReadAsync<Pager>();
                    var reception = await result.ReadAsync<ReceptionDto>();

                    resultItem.Data = reception;
                    resultItem.Pager = paging.FirstOrDefault();
                }
                return resultItem;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<DataResult<ReceptionDto>> ReceptionFirmaAsync(Guid recepctionId)
        {
            try
            {
                using (IDbConnection db = GetConnection())
                {
                    //db.Open();
                    //using (var tran = db.BeginTransaction())
                    //{
                        try
                        {
                            DynamicParameters par = new DynamicParameters();
                            par.Add("@receptionId", recepctionId);
                            var res = await db.QueryAsync(sql: "SP_reception_firma", param: par, commandType: CommandType.StoredProcedure);
                            //tran.Commit();
                            resultItem.Message = "Firmado exitosamente";
                        }
                        catch (Exception ext)
                        {
                            //tran.Rollback();
                            resultItem.Message = ext.Message;
                            resultItem.Status = System.Net.HttpStatusCode.BadRequest;

                            return resultItem;

                        }
                    //}
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

        public async Task<ReceptionDto> RecuperaReceptionAsync(Guid ReceptionId)
        {
            try
            {
                using (IDbConnection db = GetConnection())
                {
                    DynamicParameters par = new DynamicParameters();
                    par.Add("@ReceptionId", ReceptionId);
                    var result = await db.QueryMultipleAsync(sql: "SP_Reception_Recupera", param: par, commandType: CommandType.StoredProcedure);
                    var reception = await result.ReadFirstAsync<ReceptionDto>();
                    var detail = await result.ReadFirstAsync<string>();
                    reception.Detail = !string.IsNullOrWhiteSpace(detail) ? JsonConvert.DeserializeObject<IEnumerable<RecepcionDetail>>(detail) : null;
                    return reception;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
