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
    public class PaymentScheduleRepository : BaseSQLServerSqlRepository, IPaymentScheduleRepository
    {

        public PaymentScheduleRepository(string cnnString) : base(cnnString)
        {
        }
        public async Task<DataResult<IEnumerable<PaymentScheduleDto>>> GetPayments(string Token, int pageSize, string search = null, int pageNum = 1)
        {
            DataResult<IEnumerable<PaymentScheduleDto>> resultItem = new DataResult<IEnumerable<PaymentScheduleDto>>()
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

                    var result = await db.QueryMultipleAsync(sql: "SP_PaymentSchedule_selecciona", param: par, commandType: CommandType.StoredProcedure);

                    var paging = await result.ReadAsync<Pager>();
                    var payments = await result.ReadAsync<PaymentScheduleDto>();

                    resultItem.Data = payments;
                    resultItem.Pager = paging.FirstOrDefault();
                }
                return resultItem;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<DataResult<PaymentScheduleDto>> PaymentSignAsync(Guid PaymentScheduleID)
        {
            DataResult<PaymentScheduleDto> resultItem = new DataResult<PaymentScheduleDto>()
            {
                Status = System.Net.HttpStatusCode.OK,
                Message = "Firmado exitosamente"
            };
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
                            par.Add("@PaymentScheduleID", PaymentScheduleID);
                            await db.QueryAsync(sql: "SP_PaymentSchedule_Firma_Actualiza", param: par, commandType: CommandType.StoredProcedure);
                            //tran.Commit();
                            resultItem.Message = "Se actualizaron los datos correctamente";
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
        public async Task<DataResult<PaymentScheduleDto>> PaymentMailAsync(Guid PaymentScheduleID, bool? isNotification = null)
        {
            DataResult<PaymentScheduleDto> resultItem = new DataResult<PaymentScheduleDto>()
            {
                Status = System.Net.HttpStatusCode.OK,
                Message = "Firmado exitosamente"
            };
            try
            {
                using (IDbConnection db = GetConnection())
                {
                    DynamicParameters par = new DynamicParameters();
                    par.Add("@PaymentScheduleID", PaymentScheduleID);
                    par.Add("@isNotification", isNotification);
                    await db.QueryAsync(sql: "SP_PaymentSchedule_Mail_Actualiza", param: par, commandType: CommandType.StoredProcedure);
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
