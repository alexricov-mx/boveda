using BERecepcion.Core.Cancelaciones.Dto;
using BERecepcion.Core.Cancelaciones.Interfaces.Repositories;
using BERecepcion.Core.Dto;
using BERecepcion.Core.Instrucciones.Dto;
using BERecepcion.Infraestructura.Repositories;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BERecepcion.Infraestructura.Cancelaciones.Repositories
{
    public class CancelacionesRepository : BaseSQLServerSqlRepository, ICancelacionesRepository
    {
        DataResult<IEnumerable<CancelacionesCopadeDto>> resultItemIenum = new DataResult<IEnumerable<CancelacionesCopadeDto>>()
        {
            Status = System.Net.HttpStatusCode.OK,
            Message = "Todo bien"
        };

        DataResult<CancelacionesCopadeDto> resultItem = new DataResult<CancelacionesCopadeDto>()
        {
            Status = System.Net.HttpStatusCode.OK,
            Message = "Todo bien"
        };

        DataResult<IEnumerable<PaymentScheduleDto>> resultItemEnumPaymentSchedule = new DataResult<IEnumerable<PaymentScheduleDto>>()
        {
            Status = System.Net.HttpStatusCode.OK,
            Message = "Todo bien"
        };

        DataResult<PaymentScheduleDto> resultItemPaymentSchedule = new DataResult<PaymentScheduleDto>()
        {
            Status = System.Net.HttpStatusCode.OK,
            Message = "Todo bien"
        };
        DataResult<IEnumerable<PaymentListDto>> resultItemEnumPaymentList = new DataResult<IEnumerable<PaymentListDto>>()
        {
            Status = System.Net.HttpStatusCode.OK,
            Message = "Todo bien"
        };

        DataResult<PaymentListDto> resultItemPaymentList = new DataResult<PaymentListDto>()
        {
            Status = System.Net.HttpStatusCode.OK,
            Message = "Todo bien"
        };

        public CancelacionesRepository(string cnnString) : base(cnnString)
        {

        }

        public async Task<DataResult<IEnumerable<CancelacionesCopadeDto>>> GetBusquedaCancelacionesCopadeAsync(string token, Guid userId, DateTime? fechaInicial, DateTime? fechaFinal, string busqueda, int pageSize, int pageNum)
        {

            try
            {
                using (IDbConnection db = GetConnection())
                {
                    DynamicParameters par = new DynamicParameters();
                    par.Add("@token", token);
                    par.Add("@FechaInicial", fechaInicial);
                    par.Add("@FechaFinal", fechaFinal);
                    par.Add("@Busqueda", busqueda);
                    par.Add("@pagenum", pageNum);
                    par.Add("@pagesize", pageSize);
                    par.Add("@userId", userId);

                    var result = await db.QueryMultipleAsync(sql: "SP_Cancela_Copade_Busqueda_Selecciona ", param: par, commandType: CommandType.StoredProcedure);

                    var paging = await result.ReadAsync<Pager>();
                    var copade = await result.ReadAsync<CancelacionesCopadeDto>();

                    resultItemIenum.Data = copade;
                    resultItemIenum.Pager = paging.FirstOrDefault();
                }
                return resultItemIenum;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<DataResult<CancelacionesCopadeDto>> CancelacionesCopadeMasiva(CancelacionesCopadeDto dto)
        {
            try
            {
                using (IDbConnection db = GetConnection())
                {
                    DynamicParameters par = new DynamicParameters();
                    foreach (var item in dto.listCopades)
                    {
                        if (item.Status)
                        {
                            par.Add("@CopadeID", item.CopadeID);
                            par.Add("@token", dto.user);

                            var result = await db.QueryFirstAsync<CancelacionesCopadeDto>(sql: "SP_Cancela_Copade_Actualiza", param: par, commandType: CommandType.StoredProcedure);

                        }
                    }
                    resultItem.Message = "Copades Cancelados con Exito";

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

        #region PaymentSchedule
        public async Task<DataResult<IEnumerable<PaymentScheduleDto>>> GetBusquedaCancelacionesPaymentScheduleAsync(string token, Guid userId, DateTime? fechaInicial, DateTime? fechaFinal, string busqueda, int pageSize, int pageNum)
        {

            try
            {
                using (IDbConnection db = GetConnection())
                {
                    DynamicParameters par = new DynamicParameters();
                    par.Add("@token", token);
                    par.Add("@FechaInicial", fechaInicial);
                    par.Add("@FechaFinal", fechaFinal);
                    par.Add("@Busqueda", busqueda);
                    par.Add("@pagenum", pageNum);
                    par.Add("@pagesize", pageSize);
                    par.Add("@userId", userId);

                    var result = await db.QueryMultipleAsync(sql: "SP_Cancela_PaymentSchedule_Busqueda_Selecciona", param: par, commandType: CommandType.StoredProcedure);

                    var paging = await result.ReadAsync<Pager>();
                    var PaymentSchedule = await result.ReadAsync<PaymentScheduleDto>();

                    resultItemEnumPaymentSchedule.Data = PaymentSchedule;
                    resultItemEnumPaymentSchedule.Pager = paging.FirstOrDefault();
                }
                return resultItemEnumPaymentSchedule;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<DataResult<PaymentScheduleDto>> CancelacionesPaymentScheduleMasivaAsync(PaymentScheduleDto dto)
        {
            try
            {
                using (IDbConnection db = GetConnection())
                {
                    DynamicParameters par = new DynamicParameters();
                    foreach (var item in dto.CPP)
                    {
                        if (item.Status)
                        {
                            par.Add("@PaymentScheduleID", item.PaymentScheduleID);
                            par.Add("@token", dto.TokenTo);
                            var result = await db.QueryFirstAsync<PaymentScheduleDto>(sql: "SP_Cancela_PaymentSchedule_Actualiza", param: par, commandType: CommandType.StoredProcedure);

                        }
                    }
                    resultItemPaymentSchedule.Message = "Programa de Pago Cancelados con Exito";

                    return resultItemPaymentSchedule;
                }

            }
            catch (Exception ext)
            {

                resultItemPaymentSchedule.Message = ext.Message;
                resultItemPaymentSchedule.Status = System.Net.HttpStatusCode.BadRequest;

                return resultItemPaymentSchedule;

            }

        }

        #endregion

        #region PaymentList
        public async Task<DataResult<IEnumerable<PaymentListDto>>> GetBusquedaCancelacionesPaymentListAsync(string token, Guid userId, DateTime? fechaInicial, DateTime? fechaFinal, string busqueda, int pageSize, int pageNum)
        {

            try
            {
                using (IDbConnection db = GetConnection())
                {
                    DynamicParameters par = new DynamicParameters();
                    par.Add("@token", token);
                    par.Add("@FechaInicial", fechaInicial);
                    par.Add("@FechaFinal", fechaFinal);
                    par.Add("@Busqueda", busqueda);
                    par.Add("@pagenum", pageNum);
                    par.Add("@pagesize", pageSize);
                    par.Add("@userId", userId);

                    var result = await db.QueryMultipleAsync(sql: "SP_Cancela_PaymentList_Busqueda_Selecciona ", param: par, commandType: CommandType.StoredProcedure);

                    var paging = await result.ReadAsync<Pager>();
                    var PaymentList = await result.ReadAsync<PaymentListDto>();

                    resultItemEnumPaymentList.Data = PaymentList;
                    resultItemEnumPaymentList.Pager = paging.FirstOrDefault();
                }
                return resultItemEnumPaymentList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<DataResult<PaymentListDto>> CancelacionesPaymentListMasivaAsync(PaymentListDto dto)
        {
            try
            {
                using (IDbConnection db = GetConnection())
                {
                    DynamicParameters par = new DynamicParameters();
                    foreach (var item in dto.LPP)
                    {
                        if (item.Status)
                        {
                            par.Add("@PaymentListID", item.PaymentListID);
                            par.Add("@token", dto.TokenTo);
                            var result = await db.QueryFirstAsync<PaymentListDto>(sql: "SP_Cancela_PaymentList_Actualiza", param: par, commandType: CommandType.StoredProcedure);

                        }
                    }
                    resultItemPaymentList.Message = "Lista de Pago Cancelada con Exito";

                    return resultItemPaymentList;
                }

            }
            catch (Exception ext)
            {

                resultItemPaymentList.Message = ext.Message;
                resultItemPaymentList.Status = System.Net.HttpStatusCode.BadRequest;

                return resultItemPaymentList;

            }

        }

        #endregion

    }
}
