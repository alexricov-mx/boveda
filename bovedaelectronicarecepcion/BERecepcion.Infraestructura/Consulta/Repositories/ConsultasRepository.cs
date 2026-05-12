using BERecepcion.Core.Consulta.Dto;
using BERecepcion.Core.Consulta.Interfaces.Repositories;
using BERecepcion.Core.Dto;
using BERecepcion.Core.Facturas.Dto;
using BERecepcion.Infraestructura.Repositories;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BERecepcion.Infraestructura.Consulta.Repositories
{
    public class ConsultasRepository : BaseSQLServerSqlRepository, IConsultasRepository
    {
        public ConsultasRepository(string cnnString) : base(cnnString)
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
    }
}
