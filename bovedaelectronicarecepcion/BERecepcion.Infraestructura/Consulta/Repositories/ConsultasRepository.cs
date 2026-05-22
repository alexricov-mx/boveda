using BERecepcion.Core.Common.Results;
using BERecepcion.Core.Consulta.Dto;
using BERecepcion.Core.Consulta.Interfaces.Repositories;
using BERecepcion.Core.Dto;
using BERecepcion.Core.Facturas.Dto;
using BERecepcion.Core.Interfaces;
using BERecepcion.Core.OrdenSurtimiento.Dto;
using BERecepcion.Infraestructura.Repositories;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing.Printing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BERecepcion.Infraestructura.Consulta.Repositories
{
    public class ConsultasRepository
        : BaseSQLServerSqlRepository, IConsultasRepository
    {
        public ConsultasRepository(IDbConnectionFactory connectionFactory)
            : base(connectionFactory)
        {

        }


        public async Task<DataResult<IEnumerable<ReportePaymentScheduleDto>>> GetListaPaymentScheduleAsync(DateTime start, DateTime end, string search, Guid userId, int pageSize, int pageNum = 1, bool esDescarga = false)
        {
            DataResult<IEnumerable<ReportePaymentScheduleDto>> resultItem = new DataResult<IEnumerable<ReportePaymentScheduleDto>>
            {
                Status = System.Net.HttpStatusCode.OK,
            };

            if (!string.IsNullOrWhiteSpace(search))
                search = Core.Utils.SearchText.GetWhereClause(search, new List<string> { "Organismo", "ProgramaPago_Id" });


            try
            {

                using (IDbConnection db = GetConnection())
                {
                    DynamicParameters par = new DynamicParameters();
                    par.Add("@start", start);
                    par.Add("@end", end);
                    par.Add("@search", search);
                    par.Add("@pagenum", pageNum);
                    par.Add("@pagesize", pageSize);
                    par.Add("@userId", userId);
                    par.Add("@esDescarga", esDescarga);

                    var result = await db.QueryMultipleAsync(sql: "SP_paymentschedule_tabla_seleccion", param: par, commandType: CommandType.StoredProcedure);

                    var paging = await result.ReadAsync<Pager>();
                    var resultData = await result.ReadAsync<ReportePaymentScheduleDto>();

                    resultItem.Pager = paging.FirstOrDefault();

                    resultItem.Data = resultData;

                    return resultItem;
                }
            }
            catch (Exception ex)
            {
                resultItem.Status = System.Net.HttpStatusCode.BadRequest;
                resultItem.Message = $"Ocurrio un problema: {ex.Message}";
                return resultItem;
            }
        }

        public async Task<DataResult<IEnumerable<ReportePaymentListDto>>> GetListaPaymentListAsync(DateTime start, DateTime end, string search, Guid userId, int pageSize, int pageNum = 1, bool esDescarga = false)
        {
            DataResult<IEnumerable<ReportePaymentListDto>> resultItem = new DataResult<IEnumerable<ReportePaymentListDto>>
            {
                Status = System.Net.HttpStatusCode.OK,
            };

            if (!string.IsNullOrWhiteSpace(search))
                search = Core.Utils.SearchText.GetWhereClause(search, new List<string> { "Organismo", "ListaPago_Id" });

            try
            {
                using (IDbConnection db = GetConnection())
                {
                    DynamicParameters par = new DynamicParameters();
                    par.Add("@start", start);
                    par.Add("@end", end);
                    par.Add("@search", search);
                    par.Add("@pagenum", pageNum);
                    par.Add("@pagesize", pageSize);
                    par.Add("@userId", userId);
                    par.Add("@esDescarga", esDescarga);

                    var result = await db.QueryMultipleAsync(sql: "SP_paymentlist_tabla_seleccion", param: par, commandType: CommandType.StoredProcedure);

                    var paging = await result.ReadAsync<Pager>();
                    var resultData = await result.ReadAsync<ReportePaymentListDto>();

                    resultItem.Pager = paging.FirstOrDefault();

                    resultItem.Data = resultData;

                    return resultItem;
                }
            }
            catch (Exception ex)
            {
                resultItem.Status = System.Net.HttpStatusCode.BadRequest;
                resultItem.Message = $"Ocurrio un problema: {ex.Message}";
                return resultItem;
            }
        }

        public async Task<DataResult<IEnumerable<EstadoFacturaDto>>> GetEstadoFacturasAsync(DateTime start, DateTime end, string search, Guid userId, int pageSize, int pageNum = 1, bool esDescarga = false)
        {
            DataResult<IEnumerable<EstadoFacturaDto>> resultItem = new DataResult<IEnumerable<EstadoFacturaDto>>
            {
                Status = System.Net.HttpStatusCode.OK,
            };

            if (!string.IsNullOrWhiteSpace(search))
                search = Core.Utils.SearchText.GetWhereClause(search, new List<string> { "Documento", "SapOrder", "Organismo", "CorreoEnviado" });

            try
            {
                DateTime dt = new DateTime(end.Year, end.Month, end.Day, 23, 59, 59);
                using (IDbConnection db = GetConnection())
                {
                    DynamicParameters par = new DynamicParameters();
                    par.Add("@start", start);
                    par.Add("@end", dt);
                    par.Add("@search", search);
                    par.Add("@pagenum", pageNum);
                    par.Add("@pagesize", pageSize);
                    par.Add("@userId", userId);
                    par.Add("@esDescarga", esDescarga);

                    var result = await db.QueryMultipleAsync(sql: "SP_invoice_status_tabla_seleccion", param: par, commandType: CommandType.StoredProcedure);

                    var paging = await result.ReadAsync<Pager>();
                    var resultData = await result.ReadAsync<EstadoFacturaDto>();

                    resultItem.Pager = paging.FirstOrDefault();

                    resultItem.Data = resultData;

                    return resultItem;
                }
            }
            catch (Exception ex)
            {
                resultItem.Status = System.Net.HttpStatusCode.BadRequest;
                resultItem.Message = $"Ocurrio un problema: {ex.Message}";
                return resultItem;
            }
        }

        public async Task<PagedResult<SOEstimationDto>> GetEstimacionesObraAsyncRefactorAsync(
            EstimacionesObraRequest request,
            CancellationToken cancellationToken = default
            )
        {
            if (request.PageNumber < 1) request.PageNumber = 1;
            if (request.PageSize < 1) request.PageSize = 5;

            using IDbConnection db = GetConnection();
            DynamicParameters par = new();
            par.Add("@pagenum", request.PageNumber);
            par.Add("@pagesize", request.PageSize);
            par.Add("@userId", request.UserId);
            par.Add("@fechaInicial", request.FechaInicial);
            par.Add("@fechaFinal", request.FechaFinal);
            par.Add("@search", request.Search);
            par.Add("@esDescarga", request.EsDescarga);

            var multi = await db.QueryMultipleAsync(
                sql: "SP_SOEstimation_Consulta_Selecciona",
                param: par,
                commandType: CommandType.StoredProcedure
                );

            var pager = (await multi.ReadAsync<Pager>()).FirstOrDefault();
            var items = (await multi.ReadAsync<SOEstimationDto>()).ToList();


            return new PagedResult<SOEstimationDto>(
                items: items,
                totalItems: pager?.TotalItems ?? items.Count,
                pageNumber: pager?.CurrentPage ?? request.PageNumber,
                pageSize: pager?.PageSize ?? request.PageSize
                );

        }

        public async Task<PagedResult<SupplyOrderDto>> GetOrdenesSurtimientoAsync(
            OrdenSurtimientoRequest request,
            CancellationToken cancellationToken = default
            )
        {
            if (request.PageNumber < 1) request.PageNumber = 1;
            if (request.PageSize < 1) request.PageSize = 5;
            using IDbConnection db = GetConnection();

            DynamicParameters par = new();
            par.Add("@pagenum", request.PageNumber);
            par.Add("@pagesize", request.PageSize);
            par.Add("@userId", request.UserId);
            par.Add("@fechaInicial", request.FechaInicial);
            par.Add("@fechaFinal", request.FechaFinal);
            par.Add("@search", request.Search);
            par.Add("@esDescarga", request.EsDescarga);

            var multi = await db.QueryMultipleAsync(
                sql: "SP_SupplyOrder_Consulta_Selecciona",
                param: par,
                commandType: CommandType.StoredProcedure);

            var totalItems = (await multi.ReadAsync<int>()).FirstOrDefault();
            var items = (await multi.ReadAsync<SupplyOrderDto>()).ToList();
            var pager = new Pager(totalItems, request.PageNumber, request.PageSize);

            return new PagedResult<SupplyOrderDto>(
                items: items,
                totalItems: pager.TotalItems,
                pageNumber: pager.CurrentPage,
                pageSize: pager.PageSize);
        }

        public async Task<PagedResult<SOEstimationDto>> GetEstimacionesBancariasAsync(
            EstimacionBancariaRequest request, 
            CancellationToken cancellationToken = default
            )
        {
            if (request.PageNumber < 1) request.PageNumber = 1;
            if (request.PageSize < 1) request.PageSize = 5;
            using IDbConnection db = GetConnection();

            DynamicParameters par = new();
            par.Add("UserID", request.UserId);
            par.Add("@start", request.FechaInicial);
            par.Add("@end", request.FechaFinal);
            par.Add("@search", request.Search);
            par.Add("@pagenum", request.PageNumber);
            par.Add("@pagesize", request.PageSize);
            par.Add("@claveOrganismo", request.ClaveOrganismo);
            par.Add("@creditorNumber", request.CreditorNumber);
            par.Add("@esDescarga", request.EsDescarga);

            var multi = await db.QueryMultipleAsync(
                sql: "SP_SOEstimation_bancario_tabla_seleccion", 
                param: par, commandType: CommandType.StoredProcedure
                );
            var pager = (await multi.ReadAsync<Pager>()).FirstOrDefault();
            var items = (await multi.ReadAsync<SOEstimationDto>()).ToList();


            return new PagedResult<SOEstimationDto>(
                items: items,
                totalItems: pager?.TotalItems ?? items.Count,
                pageNumber: pager?.CurrentPage ?? request.PageNumber,
                pageSize: pager?.PageSize ?? request.PageSize
                );
        }
    }
}
