using BERecepcion.Core.Consulta.Copades.Dto;
using BERecepcion.Core.Consulta.Dto;
using BERecepcion.Core.Consulta.Interfaces.Repositories;
using BERecepcion.Core.Dto;
using BERecepcion.Core.Facturas.Dto;
using BERecepcion.Core.Models;
using BERecepcion.Core.OrdenSurtimiento.Dto;
using BERecepcion.Infraestructura.Repositories;
using Dapper;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BERecepcion.Infraestructura.Consulta.Repositories
{
    public class ConsultaCopadeRepository : BaseSQLServerSqlRepository, IConsultaCopadeRepository
    {
        private readonly IConfiguration _configuration;

        public ConsultaCopadeRepository(string cnnString) : base(cnnString)
        {

        }
        public async Task<DataResult<IEnumerable<CopadeDto>>> GetConsultaCopadeAsync(DateTime fechaInicial, DateTime fechaFinal, string UserID, bool esDescarga, string pageSize, string search = null, int pageNum = 1)
        {
            DataResult<IEnumerable<CopadeDto>> resultItem = new DataResult<IEnumerable<CopadeDto>>()
            {
                Status = System.Net.HttpStatusCode.OK
            };
            if (!string.IsNullOrWhiteSpace(search))
                search = Core.Utils.SearchText.GetWhereClause(search, new List<string> { "Contract", "SapOrder", "Reception", "Exercise", "Clave" });
            try
            {
                using (IDbConnection db = GetConnection())
                {
                    DynamicParameters par = new DynamicParameters();
                    par.Add("@FechaInicial", fechaInicial);
                    par.Add("@FechaFinal", fechaFinal);
                    par.Add("@search", search);
                    par.Add("@UserID", UserID);
                    par.Add("@pageSize", pageSize);
                    par.Add("@pageNum", pageNum);
                    par.Add("@esDescarga", esDescarga);

                    var result = await db.QueryMultipleAsync(sql: "SP_Consulta_Copade_selecciona", param: par, commandType: CommandType.StoredProcedure);

                    var paging = await result.ReadAsync<Pager>();
                    var copaderesult = await result.ReadAsync<CopadeDto>();

                    resultItem.Pager = paging.FirstOrDefault();
                    resultItem.Data = copaderesult;

                }
                return resultItem;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<DataResult<IEnumerable<CopadeDto>>> GetListaFiltroConsultaCopadeAsync(string CopadeID, string UserID, string pageSize, int pageNum = 1)
        {
            DataResult<IEnumerable<CopadeDto>> resultItem = new DataResult<IEnumerable<CopadeDto>>()
            {
                Status = System.Net.HttpStatusCode.OK
            };
            try
            {
                using (IDbConnection db = GetConnection())
                {
                    DynamicParameters par = new DynamicParameters();
                    par.Add("@CopadeID", CopadeID);
                    par.Add("@UserID", UserID);
                    par.Add("@pageSize", pageSize);
                    par.Add("@pageNum", pageNum);
                    var result = await db.QueryMultipleAsync(sql: "SP_Consulta_CopadeById_selecciona", param: par, commandType: CommandType.StoredProcedure);
                    var paging = await result.ReadAsync<Pager>();
                    var copaderesult = await result.ReadAsync<CopadeDto>();
                    resultItem.Data = copaderesult;
                }
                return resultItem;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<DataResult<ExpedienteEViewModel>> ExpedienteElectronico(string SAPOrder, Guid CopadeID)
        {
            DataResult<ExpedienteEViewModel> resultItem = new DataResult<ExpedienteEViewModel>()
            {
                Status = System.Net.HttpStatusCode.OK
            };
            try
            {
                using (IDbConnection db = GetConnection())
                {
                    DynamicParameters par = new DynamicParameters();
                    par.Add("@CopadeID", CopadeID);
                    par.Add("@SAPOrder", SAPOrder);

                    var result = await db.QueryMultipleAsync(sql: "SP_Copade_Expediente_Selecciona", param: par, commandType: CommandType.StoredProcedure);
                    var Copade = await result.ReadAsync<CopadeDto>();
                    var Invoice = await result.ReadAsync<InvoiceDto>();
                    var NotasCredito = await result.ReadAsync<InvoiceNotaCreditoDto>();
                    var Pagos = await result.ReadAsync<RecepcionElectronicaPDto>();
                    resultItem.Data = new ExpedienteEViewModel
                    {
                        Copade = Copade.FirstOrDefault(),
                        Invoice = Invoice.FirstOrDefault(),
                        NotasCredito = NotasCredito,
                        Pagos = Pagos.FirstOrDefault()
                    }; ;
                }
                return resultItem;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<DataResult<ExpedienteEViewModel>> Seguimiento(Guid CopadeID)
        {
            DataResult<ExpedienteEViewModel> resultItem = new DataResult<ExpedienteEViewModel>()
            {
                Status = System.Net.HttpStatusCode.OK
            };
            try
            {
                using (IDbConnection db = GetConnection())
                {
                    DynamicParameters par = new DynamicParameters();
                    par.Add("@CopadeID", CopadeID);

                    var result = await db.QueryMultipleAsync(sql: "SP_Consulta_Seguimiento_Copade_selecciona", param: par, commandType: CommandType.StoredProcedure);
                    var supplyOrder = await result.ReadAsync<SupplyOrderDto>();
                    var soEstimation = await result.ReadAsync<SOEstimationDto>();
                    var copade = await result.ReadAsync<CopadeDto>();

                    resultItem.Data = new ExpedienteEViewModel
                    {
                        SupplyOrder = supplyOrder.FirstOrDefault(),
                        SOEstimation = soEstimation.FirstOrDefault(),
                        Copade = copade.FirstOrDefault(),
                    }; ;
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
