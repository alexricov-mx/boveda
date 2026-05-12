using BERecepcion.Core.Dto;
using BERecepcion.Core.Instrucciones.Dto;
using BERecepcion.Core.Instrucciones.Interfaces.Repositories;
using BERecepcion.Infraestructura.Repositories;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;


namespace BERecepcion.Infraestructura.Instrucciones.Repositories
{
    public class PaymentListRepository : BaseSQLServerSqlRepository, IPaymentListRepository
    {

        public PaymentListRepository(string cnnString) : base(cnnString)
        {
        }
        public async Task<DataResult<IEnumerable<PaymentListDto>>> GetPayments(string Token, int pageSize, string search = null, int pageNum = 1)
        {
            DataResult<IEnumerable<PaymentListDto>> resultItem = new DataResult<IEnumerable<PaymentListDto>>()
            {
                Status = System.Net.HttpStatusCode.OK
            };
            try
            {
                using (IDbConnection db = GetConnection())
                {
                    DynamicParameters par = new DynamicParameters();
                    par.Add("@Token", Token);
                    par.Add("@pagenum", pageNum);
                    par.Add("@pagesize", pageSize);
                    par.Add("@search", search);

                    var result = await db.QueryMultipleAsync(sql: "SP_PaymentList_selecciona", param: par, commandType: CommandType.StoredProcedure);

                    var paging = await result.ReadAsync<Pager>();
                    var payments = await result.ReadAsync<PaymentListDto>();

                    resultItem.Data = payments;
                    resultItem.Pager = paging.FirstOrDefault();
                }
                return resultItem;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<DataResult<PaymentListDto>> PaymentSignAsync(Guid PaymentListID)
        {
            DataResult<PaymentListDto> resultItem = new DataResult<PaymentListDto>()
            {
                Status = System.Net.HttpStatusCode.OK,
                Message = "Firmado exitosamente"
            };
            try
            {
                using (IDbConnection db = GetConnection())
                {
                    try
                    {
                        DynamicParameters par = new DynamicParameters();
                        par.Add("@PaymentListID", PaymentListID);
                        await db.QueryAsync(sql: "SP_PaymentList_Firma_Actualiza", param: par, commandType: CommandType.StoredProcedure);
                        resultItem.Message = "Se actualizaron los datos correctamente";
                    }
                    catch (Exception ext)
                    {
                        resultItem.Message = ext.Message;
                        resultItem.Status = System.Net.HttpStatusCode.BadRequest;
                        return resultItem;
                    }
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
        public async Task<DataResult<PaymentListDto>> PaymentMailAsync(Guid PaymentListID, bool? isNotification = null)
        {
            DataResult<PaymentListDto> resultItem = new DataResult<PaymentListDto>()
            {
                Status = System.Net.HttpStatusCode.OK,
                Message = "Firmado exitosamente"
            };
            try
            {
                using (IDbConnection db = GetConnection())
                {
                    DynamicParameters par = new DynamicParameters();
                    par.Add("@PaymentListID", PaymentListID);
                    par.Add("@isNotification", isNotification);
                    await db.QueryAsync(sql: "SP_PaymentListMail_Actualiza", param: par, commandType: CommandType.StoredProcedure);
                    resultItem.Message = "Se actualizaron los datos correctamente";
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
