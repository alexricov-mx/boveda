using BERecepcion.Core.Consulta.Copades.Dto;
using BERecepcion.Core.Consulta.Dto;
using BERecepcion.Core.Consulta.Interfaces.Repositories;
using BERecepcion.Core.Dto;
using BERecepcion.Core.Facturas.Dto;
using BERecepcion.Core.OrdenSurtimiento.Dto;
using BERecepcion.Infraestructura.Repositories;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace BERecepcion.Infraestructura.Consulta.Repositories
{
    public class ExpedienteElectronicoRepository : BaseSQLServerSqlRepository, IExpedienteElectronicoRepositoryAsync
    {
        public ExpedienteElectronicoRepository(string cnnString) : base(cnnString)
        {
        }
        public async Task<DataResult<ExpedienteEViewModel>> ExpedienteElectronico(string SAPOrder = null, Guid? CopadeID = null, Guid? AnaliticoPagoID = null)
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
                    par.Add("@SAPOrder", SAPOrder);
                    par.Add("@CopadeID", CopadeID);
                    par.Add("@AnaliticoPagoID", AnaliticoPagoID);

                    /*  
                     *  SP_Expediente_Electronico_Selecciona => Devuelve multiples results sets
                        1 = SuppyOrder
                        2 = SOEstimation
                        3 = Copade
                        4 = InvoiceDto
                        5 = IEnumerable<InvoiceNotaCreditoDto>
                        6 = RecepcionElectronicaPDto
                     */
                    var result = await db.QueryMultipleAsync(sql: "SP_Expediente_Electronico_Selecciona", param: par, commandType: CommandType.StoredProcedure);
                    var supplyOrder = await result.ReadAsync<SupplyOrderDto>();
                    var estimation = await result.ReadAsync<SOEstimationDto>();
                    IEnumerable<CopadeDto> copade;
                    IEnumerable<AnaliticoPagoDto> analiticopago;
                    IEnumerable<InvoiceDto> invoice;
                    IEnumerable<InvoiceNotaCreditoDto> notasCredito;
                    IEnumerable<RecepcionElectronicaPDto> pagos;
                    resultItem.Data = new ExpedienteEViewModel
                    {
                        SupplyOrder = supplyOrder.FirstOrDefault(),
                        SOEstimation = estimation.FirstOrDefault(),
                    };
                    if (CopadeID != null)
                    {
                        copade = await result.ReadAsync<CopadeDto>();
                        invoice = await result.ReadAsync<InvoiceDto>();
                        notasCredito = await result.ReadAsync<InvoiceNotaCreditoDto>();
                        pagos = await result.ReadAsync<RecepcionElectronicaPDto>();
                        resultItem.Data.Copade = copade.FirstOrDefault();
                        resultItem.Data.Invoice = invoice.FirstOrDefault();
                        resultItem.Data.NotasCredito = notasCredito;
                        resultItem.Data.Pagos = pagos.FirstOrDefault();
                    }
                    else if (AnaliticoPagoID != null)
                    {
                        analiticopago = await result.ReadAsync<AnaliticoPagoDto>();
                        invoice = await result.ReadAsync<InvoiceDto>();
                        resultItem.Data.AnaliticoPago = analiticopago.FirstOrDefault();
                        resultItem.Data.Invoice = invoice.FirstOrDefault();
                    }
                }
                return resultItem;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
